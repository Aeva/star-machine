
using System.Collections.Concurrent;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Matrix4x4 = System.Numerics.Matrix4x4;

using static StarMachine.MoreMath;
using static Evaluator.ProgramBuffer;
using PerfCounter = Perf.PerfCounter;

using FixedInt = FixedPoint.FixedInt;
using Fixie = FixedPoint.Fixie;


namespace StarMachine;

using SurfelList = List<(Fixie Position, Vector3 Color)>;
using ConcurrentSurfelQueue = ConcurrentQueue<List<(Fixie Position, Vector3 Color)>>;


class HighLevelRenderer
{
    public TracingGrist Model;
    public TracingGrist Floor;
    public TracingGrist Stuff;

    private RenderingConfig Settings;

    public float AspectRatio = 1.0f;
    private int FrustaCountX = 0;
    private int FrustaCountY = 0;
    private float FineDiameter = 0.0f;   // Fine grain diameter.
    private float CoarseDiameter = 0.0f; // Coarse grain diameter.

    // Maybe these belong on the character controller class instead?
    public float Turning = 0.0f;
    public float Tunneling = 0.25f;
    public float GrainAlpha = 1.0f;

    // Stats for display
    public double CadenceHz = 0.0f;
    public double CadenceMs = 0.0f;
    public double UpdatesPerFrame = 0.0;
    public double Efficiency = 0.0;
    public double UpdateProcessingMs = 0.0;
    public double ConvergenceTimeMs = 0.0;
    public string? Analysis = null;

    // These are ring buffers for splat rendering.
    public Fixie[] PositionUpload = Array.Empty<Fixie>();
    public Vector3[] ColorUpload = Array.Empty<Vector3>();
    public uint LiveSurfels = 0;
    public uint WriteCursor = 0;
    public float SplatDiameter = 0.1f;

    private PerfCounter FrameRate = new PerfCounter();
    private PerfCounter SplatCopyCount = new PerfCounter();
    private PerfCounter SplatCopyTime = new PerfCounter();
    private long LastPerfLog = 0;

    private CancellationTokenSource CancelSource = new CancellationTokenSource();

    public ConcurrentSurfelQueue PendingSurfels;

    public Fixie[] LightPoints = new Fixie[3];
    public Vector3[] LightColors = new Vector3[3];
    public Fixie Eye = new Fixie(0.0f, -8.0f, 2.0f);
    public Vector3 EyeDir = new Vector3(0.0f, 1.0f, 0.0f);
    public Fixie MovementProjection = Fixie.Zero;

    public Matrix4x4 WorldToView = Matrix4x4.Identity;
    public Matrix4x4 ViewToClip = Matrix4x4.Identity;
    public Matrix4x4 ClipToView = Matrix4x4.Identity;

    public HighLevelRenderer(RenderingConfig InSettings)
    {
        Settings = InSettings;

        LightColors[0] = new Vector3(1.0f, 0.0f, 0.0f);
        LightColors[1] = new Vector3(0.0f, 1.0f, 0.0f);
        LightColors[2] = new Vector3(0.0f, 0.0f, 1.0f);

        PendingSurfels = new ConcurrentSurfelQueue();

        {
#if true
            var BasicThing =
            Diff(
                Inter(
                    Cube(4.0f),
                    Sphere(5.5f)),
                 Union(
                     Cylinder(3.0f, 5.0f),
                    Union(
                        RotateX(Cylinder(3.0f, 5.0f), 90.0f),
                        RotateY(Cylinder(3.0f, 5.0f), 90.0f))));

            BasicThing = MoveZ(BasicThing, 2.0f);
#else
            var BasicThing =
            MoveZ(Box(4.0f, 10.0f, 5.0f), 2.5f);
#endif

            var Ground =
            Plane(0.0f, 0.0f, 1.0f);

            Model = new TracingGrist(Union(Ground, BasicThing));
            Floor = new TracingGrist(Ground);
            Stuff = new TracingGrist(BasicThing);
        }
    }

    public void Boot(FrameInfo Frame)
    {
        AspectRatio = Frame.AspectRatio;

        double ScreenArea = Frame.Width * Frame.Height;

        double MinSquareArea = ScreenArea / (double)Settings.MaxSurfels;
        double MinSquareEdge = Math.Sqrt(MinSquareArea);
        FineDiameter = (float)(MinSquareEdge / Frame.Height * Math.Sqrt(2.0));

        double MaxSquareArea = ScreenArea / (double)Settings.TracingRate;
        double MaxSquareEdge = Math.Sqrt(MaxSquareArea);
        CoarseDiameter = (float)(MaxSquareEdge / Frame.Height * Math.Sqrt(2.0));

        FrustaCountX = (int)Math.Floor((Frame.Width / MaxSquareEdge));
        FrustaCountY = (int)Math.Floor((Frame.Height / MaxSquareEdge));
        Settings.TracingRate = FrustaCountX * FrustaCountY;

        WorldToView = Matrix4x4.CreateLookTo(
            Vector3.Zero,
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0, 0, 1));
        InfinitePerspective(out ViewToClip, Settings.FieldOfView, AspectRatio, Settings.NearPlane);

        ClipToView = Matrix4x4.Identity;
        Matrix4x4.Invert(ViewToClip, out ClipToView);

        PositionUpload = new Fixie[Settings.MaxSurfels];
        ColorUpload = new Vector3[Settings.MaxSurfels];

        {
            var parallelOptions = new ParallelOptions();
            parallelOptions.CancellationToken = CancelSource.Token;
            parallelOptions.MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 2, 1);

            Task.Run(() => {
                while(!parallelOptions.CancellationToken.IsCancellationRequested)
                {
                    if (PendingSurfels.Count < Settings.TracingRate)
                    {
                        try
                        {
                            PopulateSplats(parallelOptions);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                    }
                    Thread.Yield();
                }
            }, CancelSource.Token);
        }

        FrameRate.Reset();
        SplatCopyCount.Reset();
        SplatCopyTime.Reset();
    }

    public void Teardown()
    {
         // stuff for tearing down threadpools
    }

    public void Advance(FrameInfo Frame, PerformerStatus PlayerState, CharacterController Game)
    {
        FrameRate.LogFrame();

        if (!PlayerState.Paused)
        {
            var FindLightPosition = (double Speed, double Phase) =>
            {
                double T = Frame.RunTimeMs / 5000.0;
                double P = 2.0 * Math.PI * Phase;
                float S = (float)Math.Sin(T * Speed + P);
                float C = (float)Math.Cos(T * Speed + P);
                return Eye + new Fixie(S * 20.0f, C * 20.0f, 15.0f);
            };

            LightPoints[0] = FindLightPosition(1.0, 0.0 / 3.0);
            LightPoints[1] = FindLightPosition(2.0, 1.0 / 3.0);
            LightPoints[2] = FindLightPosition(-4.0, 2.0 / 3.0);
        }

        WorldToView = Matrix4x4.CreateLookTo(
            Vector3.Zero,
            EyeDir,
            new Vector3(0, 0, 1));

        {
            const long UpdateTimeSlice = TimeSpan.TicksPerMillisecond * 4;

            long StartTime = DateTime.UtcNow.Ticks;
            long ElapsedTicks = 0;
            int Processed = 0;

            SurfelList? SurfelBatch;
            while (ElapsedTicks < UpdateTimeSlice && PendingSurfels.TryDequeue(out SurfelBatch))
            {
                foreach (var Surfel in SurfelBatch)
                {
                    (Fixie Position, Vector3 Color) = Surfel;

                    PositionUpload[WriteCursor] = Position;
                    ColorUpload[WriteCursor] = Color;
                    WriteCursor = (WriteCursor + 1) % (uint)Settings.MaxSurfels;
                    LiveSurfels = Math.Min(LiveSurfels + 1, (uint)Settings.MaxSurfels);
                }
                Processed += SurfelBatch.Count;
                ElapsedTicks = DateTime.UtcNow.Ticks - StartTime;
            }

            double ElapsedCopyTimeMs = (double)ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;

            SplatCopyCount.LogQuantity(Processed);
            SplatCopyTime.LogQuantity(ElapsedCopyTimeMs);
        }

        SplatDiameter = Single.Lerp(FineDiameter, CoarseDiameter, GrainAlpha);

        const long PerfLogFrequency = TimeSpan.TicksPerSecond * 1;
        if (DateTime.UtcNow.Ticks - LastPerfLog >= PerfLogFrequency)
        {
            if (LastPerfLog > 0)
            {

                CadenceMs = FrameRate.Average();
                CadenceHz = 1.0 / CadenceMs * 1000.0;

                UpdatesPerFrame = SplatCopyCount.Average();
                UpdateProcessingMs = SplatCopyTime.Average();
                Efficiency = (UpdatesPerFrame / Settings.MaxSurfels) * 100.0;

                ConvergenceTimeMs = ((Settings.MaxSurfels / UpdatesPerFrame) - 1) * CadenceMs;

                if (Efficiency == 100.0)
                {
                    Analysis = null;
                }
                else if (UpdateProcessingMs < 4.0)
                {
                    Analysis = "Convergence time is bottlenecked on shading throughput.";
                }
                else if (UpdateProcessingMs >= 4.0)
                {
                    Analysis = "Convergence time is bottlenecked on Synchronization.";
                }
            }
            LastPerfLog = DateTime.UtcNow.Ticks;
        }
    }

    private void PopulateSplats(ParallelOptions parallelOptions)
    {
        Matrix4x4 ViewToWorld = Matrix4x4.Identity;
        Matrix4x4 WorldToLocal = Matrix4x4.Identity;

        Matrix4x4.Invert(WorldToView, out ViewToWorld);

        var CeilDivide = (int Numerator, int Denominator) =>
        {
            return (Numerator + Denominator - 1) / Denominator;
        };

        var TracingPartitioner = Partitioner.Create(0, Settings.TracingRate);

        Parallel.ForEach(TracingPartitioner, (SliceParams, LoopState) =>
        {
            int SliceStart = SliceParams.Item1;
            int SliceStop = SliceParams.Item2;
            int Range = SliceStop - SliceStart;

            var NewSurfels = new SurfelList(Range);

            var SplatRNG = new Random();

            Fixie ProjectedEye = Eye + MovementProjection;
            Fixie RelativeTracingOrigin = Model.RelativeTracingOrigin(ProjectedEye);
            Vector3 CachedEye = (ProjectedEye - RelativeTracingOrigin).ToVector3();

            for (int Cursor = SliceStart; Cursor < SliceStop; ++Cursor)
            {
                Vector3 RayDir;
                {
#if true
                    float JitterX = (float)SplatRNG.Next(-1000, 1000) / 1000.0f * 0.5f;
                    float JitterY = (float)SplatRNG.Next(-1000, 1000) / 1000.0f * 0.5f;
#else
                    float JitterX = 0.0f;
                    float JitterY = 0.0f;
#endif

                    float FrustumX = (float)(Cursor % FrustaCountX) + 0.5f + JitterX;
                    float FrustumY = (float)(Cursor / FrustaCountX) + 0.5f + JitterY;

                    Vector4 ClipTarget;
                    ClipTarget.X = (FrustumX / (float)FrustaCountX * 2.0f - 1.0f);
                    ClipTarget.Y = -(FrustumY / (float)FrustaCountY * 2.0f - 1.0f);
                    ClipTarget.Z = -1;
                    ClipTarget.W = 1;

#if true
                    if (Tunneling > 0.0f || Math.Abs(Turning) > 0.0f)
                    {
                        Vector2 Offset;
                        Offset.X = 0.5f * Turning;
                        Offset.Y = 0.0f;

                        Vector2 Scale;
                        Scale.X = Math.Abs((ClipTarget.X >= Offset.X ? 1.0f : -1.0f) - Offset.X);
                        Scale.Y = 1.0f;

                        Vector2 Point;
                        Point.X = ClipTarget.X;
                        Point.Y = ClipTarget.Y;

                        Point = (Point - Offset) / Scale;
                        float Mag = Point.Length();
                        if (Mag > 0.0f)
                        {
                            Vector2 Norm = Point / Mag;
                            Norm /= Math.Max(Math.Abs(Norm.X), Math.Abs(Norm.Y));
                            float MagScale = Norm.Length();
                            float NewMag = Mag / MagScale;
                            NewMag = NewMag * NewMag * MagScale;
                            float Distortion = NewMag / Mag;
                            Point *= Distortion;
                        }
                        Point = Point * Scale + Offset;

                        float Alpha = Math.Max(Tunneling * 0.75f, Math.Abs(Turning) * 0.5f);
                        ClipTarget.X = Single.Lerp(ClipTarget.X, Point.X, Alpha);
                        ClipTarget.Y = Single.Lerp(ClipTarget.Y, Point.Y, Alpha);
                    }
#endif

#if false
                    ClipTarget.X *= 0.5f;
                    ClipTarget.Y *= 0.5f;
#endif

                    float Overscan = Single.Lerp(1.1f, 1.5f, Tunneling);
                    ClipTarget.X *= Overscan;
                    ClipTarget.Y *= Overscan;

                    Vector4 ViewTarget = Vector4.Transform(ClipTarget, ClipToView);
                    ViewTarget /= ViewTarget.W;

                    Vector3 ViewRayDir;
                    ViewRayDir.X = ViewTarget.X;
                    ViewRayDir.Y = ViewTarget.Y;
                    ViewRayDir.Z = ViewTarget.Z;
                    ViewRayDir = Vector3.Normalize(ViewRayDir);

                    RayDir = Vector3.TransformNormal(ViewRayDir, ViewToWorld);
                }

                // For some reason, 100_000.0f as a max draw distance works fine on my Linux laptop but not on my Windows desktop?
                var Start = CachedEye;
                var Stop = RayDir * 10_000.0f + CachedEye;

                Vector3 MissColor = Vector3.Zero;
                {
                    // This will need to be revised if the view can ever face away from the horizon.
                    float SkyAlpha = 1.0f - Single.Abs(RayDir.Z);
                    SkyAlpha = SkyAlpha * SkyAlpha;
                    SkyAlpha = SkyAlpha * SkyAlpha;
                    SkyAlpha = 1.0f - SkyAlpha;
                    Vector3[] SkyGradient = {
                        Vector3.One,
                        new Vector3(1.5f, 0.4f, 0.4f),
                        new Vector3(0.2f, 0.2f, 0.2f),
                        new Vector3(0.2f, 0.2f, 0.2f),
                        new Vector3(0.2f, 0.2f, 0.2f),
                        new Vector3(0.2f, 0.2f, 0.2f),
                        new Vector3(0.2f, 0.2f, 0.2f),
                        new Vector3(0.1f, 0.1f, 0.1f),
                    };

                    MissColor = SkyGradient[0];

                    for (int Tail = 1; Tail < SkyGradient.Count(); ++Tail)
                    {
                        MissColor = Vector3.Lerp(MissColor, SkyGradient[Tail], SkyAlpha);
                    }
                }

                bool Mirror = false;
                float FirstTravel = 0.0f;
                float MirrorTravel = 0.0f;

                var MirrorColor = new Vector3(0.0f, 0.0f, 0.0f);

                if (Start != Stop)
                {
                    (bool Hit, FirstTravel) = Model.TravelTrace(Start, RayDir, 10_000.0f, 0.0f);
                    Vector3 Position = RayDir * FirstTravel + Start;

                    if (Hit)
                    {
                        float FloorDist = Floor.Eval(Position);
                        float StuffDist = Stuff.Eval(Position);
                        if (FloorDist < StuffDist)
                        {
                            {
                                var Normal = Model.Gradient(Position);

                                for (int LightIndex = 0; LightIndex < LightPoints.Length; ++LightIndex)
                                {
                                    var LightPoint = (LightPoints[LightIndex] + MovementProjection - RelativeTracingOrigin).ToVector3();
                                    var LightColor = LightColors[LightIndex];

                                    var Offset = Normal * 0.01f + Position;
                                    float Visibility = Model.LightTrace(Normal * 0.01f + Position, LightPoint, 0.1f);

                                    if (Visibility > 0.0f)
                                    {
                                        var LightRay = Vector3.Normalize(LightPoint - Position);

                                        float Luminence = Math.Max(Vector3.Dot(LightRay, Normal), 0.0f) * Visibility;

                                        MirrorColor += LightColor * Luminence;
                                    }
                                }
                            }

                            Mirror = true;
                            Vector3 MirrorRay = Vector3.Reflect(RayDir, Vector3.UnitZ);
                            (Hit, MirrorTravel) = Stuff.TravelTrace(Position, MirrorRay, 10_000.0f, 0.0f);
                            Position = MirrorRay * MirrorTravel + Position;
                        }
                    }

                    if (Hit)
                    {
                        var Normal = Model.Gradient(Position);
                        var SplatColor = new Vector3(0.0f, 0.0f, 0.0f);

                        for (int LightIndex = 0; LightIndex < LightPoints.Length; ++LightIndex)
                        {
                            var LightPoint = (LightPoints[LightIndex] + MovementProjection - RelativeTracingOrigin).ToVector3();
                            var LightColor = LightColors[LightIndex];

                            var Offset = Normal * 0.01f + Position;
                            float Visibility = Model.LightTrace(Normal * 0.01f + Position, LightPoint, 0.1f);

                            if (Visibility > 0.0f)
                            {
                                var LightRay = Vector3.Normalize(LightPoint - Position);

                                float Luminence = Math.Max(Vector3.Dot(LightRay, Normal), 0.0f) * Visibility;

                                SplatColor += LightColor * Luminence;
                            }
                        }
#if false
                        // This places the reflection splats on the surface of the mirror.
                        Position = RayDir * (FirstTravel) + Start;
#else
                        // This places the reflection splats in world space as if the mirror were a window.
                        Position = RayDir * (FirstTravel + MirrorTravel) + Start;
#endif
                        Fixie SplatPosition = RelativeTracingOrigin + new Fixie(Position);
                        if (Mirror)
                        {
                            //SplatColor = (SplatColor * MirrorColor);
                            float Fnord = Single.Min(MirrorTravel / 20.0f, 0.5f);
                            SplatColor = Vector3.Lerp(SplatColor, MissColor, Fnord) * Vector3.Lerp(MirrorColor, MissColor, Fnord);
                        }
                        NewSurfels.Add((SplatPosition, SplatColor));
                        continue;
                    }
                }
                {
                    if (Mirror)
                    {
                        Vector3 Position = RayDir * FirstTravel + Start;
                        Fixie SplatPosition = RelativeTracingOrigin + new Fixie(Position);
                        NewSurfels.Add((SplatPosition, MirrorColor));
                    }
                    else
                    {
                        Fixie SplatPosition = RelativeTracingOrigin + new Fixie(Stop);
                        NewSurfels.Add((SplatPosition, MissColor));
                    }
                    continue;
                }
            }

            if (NewSurfels.Count > 0)
            {
                PendingSurfels.Enqueue(NewSurfels);
            }
        });
    }
}
