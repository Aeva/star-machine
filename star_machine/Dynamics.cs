

using Vector3 = System.Numerics.Vector3;

using SDL3;
using static SDL3.SDL;

using FixedInt = FixedPoint.FixedInt;
using Fixie = FixedPoint.Fixie;


namespace StarMachine;


class CharacterController
{
    private HighLevelRenderer HighRenderer;
    public float CurrentHeading = 0.0f;

    // Units per second
    private float Acceleration = 15.0f;
    private float TopSpeed = 4.0f * 1609.34f * 600.0f;

#if true
    private Vector3 LinearVelocity = new Vector3(0.0f, 10.0f, 0.0f);
#else
    // Rough estimate of 600 mph, assuming one of the cubes is about a meter across.
    private Vector3 LinearVelocity = new Vector3(0.0f, 4.0f * 1609.34f * 600.0f, 0.0f);
#endif

    // Degrees per second
    private float TurnSpeed = 30.0f;

    public CharacterController(HighLevelRenderer InHighRenderer)
    {
        HighRenderer = InHighRenderer;
    }

    public void Advance(FrameInfo Frame, PerformerStatus PlayerState)
    {
        if (PlayerState.Reset)
        {
            CurrentHeading = 0.0f;
            HighRenderer.Eye = new Fixie(0.0f, -8.0f, 2.0f);
            HighRenderer.EyeDir = new Vector3(0.0f, 1.0f, 0.0f);
        }
        if (PlayerState.HardStop)
        {
            LinearVelocity = new Vector3(0.0f, 0.0f, 0.0f);
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

        if (PlayerState.Gas > 0.0f)
        {
            LinearVelocity += HighRenderer.EyeDir * Acceleration * Seconds * PlayerState.Gas;
        }
        else
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

                for (int i = 0; i < 100 && Remainder > 0.01f; ++i)
                {
                    (bool Hit, float Travel) = HighRenderer.Model.TravelTrace(HighRenderer.Eye.ToVector3(), Dir, Remainder, 0.125f);
                    Remainder = Math.Max(0.0f, Remainder - Travel);
                    HighRenderer.Eye += Dir * Travel;

                    if (Hit)
                    {
                        Vector3 Normal = HighRenderer.Model.Gradient(HighRenderer.Eye.ToVector3());
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

                Magnitude = LinearVelocity.Length();
                LinearVelocity = Dir * Magnitude;
            }

            if (Magnitude > 0.01f)
            {
                HighRenderer.Tunneling += 1.0f * Seconds;
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
            }
            HighRenderer.Tunneling = Math.Clamp(HighRenderer.Tunneling, 0.0f, 1.0f);
            HighRenderer.GrainAlpha = HighRenderer.Tunneling * 0.4f;
            HighRenderer.GrainAlpha *= HighRenderer.GrainAlpha;
#if true
            // This is cheating and will not generalize, but it at least proves the "shadow world" phenomena that
            // is experienced while moving at high speeds is caused by numeric issues due to floating point
            // coordinates.  This Could hypothetically be made to be slightly less cheating by performing the tracing
            // relative to the eye position and making the appropriate adjustments to place relative positions in the
            // correct part of the repeating area, this is stil good news because extremely high speed movement
            // is gameplay viable provided that the world is constructed piecewise.
            {
                int Iterations = 100;
                float Span = 10.0f * (float)Iterations;
                Fixie Splice = ((Fixie.Round(HighRenderer.Eye / Span) * Span));
                HighRenderer.Eye.X -= Splice.X;
                HighRenderer.Eye.Y -= Splice.Y;
            }
#endif
        }
    }
}
