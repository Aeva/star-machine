
using Vector3 = System.Numerics.Vector3;

using SDL3;
using static SDL3.SDL;

using FixedInt = FixedPoint.FixedInt;
using Fixie = FixedPoint.Fixie;


namespace StarMachine;


class CharacterController
{
    private HighLevelRenderer HighRenderer;
    private LowLevelRenderer LowRenderer;
    public float CurrentHeading = 0.0f;

    public double MilesPerHour = 0.0; // Current display speed
    public double SpeedOfLight = 0.0; // Current display speed

    // Units per second
    private float Acceleration = 15.0f;
    private float TopSpeed = 4.0f * 1609.34f * 600.0f;

    private Vector3 LinearVelocity = Vector3.Zero;

    // Interpolated starting velocity, which peaks at 150 mph (assuming the cube things are a meter wide).
    private Vector3 FastVelocity = new Vector3(0.0f, 4.0f * 0.44704f * 150.0f, 0.0f);

    // Interpolated starting velocity, which peaks at 600 miles per second (assuming the cube things are a meter wide).
    private Vector3 WarpVelocity = new Vector3(0.0f, 4.0f * 1609.344f * 600.0f, 0.0f);

    // Degrees per second
    private float TurnSpeed = 30.0f;

    // Warp demo is active
    public bool WarpMode = true;

    public CharacterController(HighLevelRenderer InHighRenderer, LowLevelRenderer InLowRenderer)
    {
        LowRenderer = InLowRenderer;
        HighRenderer = InHighRenderer;
    }

    public void Advance(FrameInfo Frame, PerformerStatus PlayerState)
    {
        if (PlayerState.Reset)
        {
            CurrentHeading = 0.0f;
            HighRenderer.Eye = new Fixie(0.0f, -8.0f, 2.0f);
            HighRenderer.EyeDir = new Vector3(0.0f, 1.0f, 0.0f);
            WarpMode = false;
        }
        if (PlayerState.HardStop)
        {
            WarpMode = false;
        }
        if (PlayerState.HardStop || Frame.Number == 0)
        {
            LinearVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        }
        if (PlayerState.Align)
        {
            float TargetHeading = Single.Round(CurrentHeading / 90.0f) * 90.0f;
            CurrentHeading = Single.Lerp(CurrentHeading, TargetHeading, 1.0f * (float)(Frame.ElapsedMs / 1000.0));
            bool Snap = Single.Abs(TargetHeading - CurrentHeading) <= 0.5f;

            float Radians = (float)(Math.PI / 180.0) * CurrentHeading;
            HighRenderer.EyeDir.X = Single.Sin(Radians);
            HighRenderer.EyeDir.Y = Single.Cos(Radians);
            HighRenderer.EyeDir.Z = 0.0f;
            HighRenderer.EyeDir = Vector3.Normalize(HighRenderer.EyeDir);

            if (Snap)
            {
                CurrentHeading = TargetHeading;
                HighRenderer.EyeDir.X = Single.Round(HighRenderer.EyeDir.X);
                HighRenderer.EyeDir.Y = Single.Round(HighRenderer.EyeDir.Y);
                LinearVelocity = HighRenderer.EyeDir * LinearVelocity.Length();
                PlayerState.Align = false;
            }
        }
        if (WarpMode)
        {
            if (Frame.Number > 0 && Frame.Number <= 800)
            {
                float Alpha = (float)(Frame.Number) / 800.0f;
                LinearVelocity = Vector3.Lerp(Vector3.Zero, FastVelocity, Alpha);
            }
            else if (Frame.Number > 800 && Frame.Number <= 1600)
            {
                float Alpha = (float)(Frame.Number - 800) / 800.0f;
                float Fnord = Single.Pow(Alpha, 16.0f);
                LinearVelocity = Vector3.Lerp(Vector3.Lerp(FastVelocity, FastVelocity * 2.0f, Alpha), WarpVelocity, Fnord);
            }
            else if (Frame.Number > 1600)
            {
                WarpMode = false;
            }
        }

        float Seconds = (float)(Frame.ElapsedMs / 1000.0);

        float Turn = 0.0f;

        if (Math.Abs(PlayerState.Turn) > 0.0f)
        {
            Turn = TurnSpeed * Seconds * PlayerState.Turn * 2.0f;
            PlayerState.Turn = 0.0f;
        }
        else
        {
            if (PlayerState.Left)
            {
                Turn -= TurnSpeed * Seconds;
            }
            if (PlayerState.Right)
            {
                Turn += TurnSpeed * Seconds;
            }
        }

        if (Math.Abs(Turn) > 0.001)
        {
            HighRenderer.Turning = Math.Clamp(HighRenderer.Turning += Turn, -1.0f, 1.0f);
            CurrentHeading = (CurrentHeading + Turn) % 360.0f;
            float Radians = (float)(Math.PI / 180.0) * CurrentHeading;
            HighRenderer.EyeDir.X = (float)Math.Sin(Radians);
            HighRenderer.EyeDir.Y = (float)Math.Cos(Radians);
            HighRenderer.EyeDir.Z = 0.0f;
            HighRenderer.EyeDir = Vector3.Normalize(HighRenderer.EyeDir);
        }
        else
        {
            HighRenderer.Turning = 0.0f;
        }

        if (PlayerState.Gas > 0.0f && !PlayerState.HardStop)
        {
            LinearVelocity += HighRenderer.EyeDir * Acceleration * Seconds * PlayerState.Gas;
        }
        else if (!WarpMode)
        {
            LinearVelocity *= 0.99f;
        }

        if (PlayerState.Brake > 0.0f)
        {
            float Magnitude = LinearVelocity.Length();
            if (Magnitude > 0.0f)
            {
                float NewMagnitude = Math.Max(Magnitude - (Acceleration * 0.5f * Seconds), 0.0f);
                LinearVelocity = (LinearVelocity / Magnitude) * Single.Lerp(Magnitude, NewMagnitude, PlayerState.Brake);
            }
        }

        if (!PlayerState.Paused)
        {
            float Magnitude = LinearVelocity.Length();
            if (Magnitude > TopSpeed)
            {
                LinearVelocity /= Magnitude;
                LinearVelocity *= TopSpeed;
                Magnitude = TopSpeed;
            }

            if (Magnitude > 0.01f)
            {
                float Remainder = Magnitude * Seconds + 0.1f;
                Vector3 Dir = Vector3.Normalize(LinearVelocity);

                Fixie RelativeTracingOrigin = HighRenderer.Model.RelativeTracingOrigin(HighRenderer.Eye);
                Fixie EffectiveEye = HighRenderer.Eye - RelativeTracingOrigin;

                for (int i = 0; i < 100 && Remainder > 0.01f; ++i)
                {
                    (bool Hit, float Travel) = HighRenderer.Model.TravelTrace(EffectiveEye.ToVector3(), Dir, Remainder, 0.125f);
                    Remainder = Math.Max(0.0f, Remainder - Travel);
                    EffectiveEye += Dir * Travel;

                    if (Hit)
                    {
                        Vector3 Normal = HighRenderer.Model.Gradient(EffectiveEye.ToVector3());
                        Normal.Z = 0.0f;
                        float LenSquared = Vector3.Dot(Normal, Normal);
                        if (LenSquared > 0.0f)
                        {
                            Normal /= (float)Math.Sqrt(LenSquared);
                            Dir = Vector3.Reflect(Dir, Normal);
                            LinearVelocity *= 0.75f;
                        }
                        else
                        {
                            Dir = -Dir;
                            LinearVelocity *= 0.5f;
                        }
                    }
                }

                HighRenderer.Eye = EffectiveEye + RelativeTracingOrigin;

                Magnitude = LinearVelocity.Length();
                LinearVelocity = Dir * Magnitude;
            }

            if (Magnitude > 0.01f)
            {
                HighRenderer.Tunneling += 1.0f * Seconds;
                float ElapsedSeconds = (float)(Frame.ElapsedMs / 1000.0);
                HighRenderer.MovementProjection = new Fixie(LinearVelocity * ElapsedSeconds);
            }
            else
            {
                if (HighRenderer.Tunneling > 0.001f)
                {
                    HighRenderer.Tunneling -= 0.25f * Seconds;
                }
                else
                {
                    HighRenderer.Tunneling = 0.0f;
                }
                HighRenderer.MovementProjection = Fixie.Zero;
            }
            HighRenderer.Tunneling = Math.Clamp(HighRenderer.Tunneling, 0.0f, 1.0f);
            HighRenderer.GrainAlpha = HighRenderer.Tunneling * 0.4f;
            HighRenderer.GrainAlpha *= HighRenderer.GrainAlpha;

            const double ToMilesPerHour = 1.0 / (1609.344 / 3600.0);
            const double ToSpeedOfLight = 1.0 / (299792458.0);
            double MetersPerSecond = LinearVelocity.Length() * 0.25;
            MilesPerHour = MetersPerSecond * ToMilesPerHour;
            SpeedOfLight = Math.Round(MetersPerSecond * ToSpeedOfLight, 4);
        }
    }
}
