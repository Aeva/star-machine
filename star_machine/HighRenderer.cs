
using System.Collections.Concurrent;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Matrix4x4 = System.Numerics.Matrix4x4;

using static StarMachine.MoreMath;
using static Evaluator.ProgramBuffer;
using PerfCounter = Perf.PerfCounter;


namespace StarMachine;

using SurfelList = List<(Vector3 Position, Vector3 Color)>;
using ConcurrentSurfelQueue = ConcurrentQueue<List<(Vector3 Position, Vector3 Color)>>;


class HighLevelRenderer
{
    public TracingGrist Model;

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

    // These are ring buffers for splat rendering.
    public Vector3[] PositionUpload = Array.Empty<Vector3>();
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

    public Vector3[] LightPoints = new Vector3[3];
    public Vector3[] LightColors = new Vector3[3];
    public Vector3 Eye = new Vector3(0.0f, -8.0f, 2.0f); // May be offset by constructor.
    public Vector3 EyeDir = new Vector3(0.0f, 1.0f, 0.0f);

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
            Eye,
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0, 0, 1));
        InfinitePerspective(out ViewToClip, Settings.FieldOfView, AspectRatio, Settings.NearPlane);

        ClipToView = Matrix4x4.Identity;
        Matrix4x4.Invert(ViewToClip, out ClipToView);

        PositionUpload = new Vector3[Settings.MaxSurfels];
        ColorUpload = new Vector3[Settings.MaxSurfels];

        {
            var parallelOptions = new ParallelOptions();
            parallelOptions.CancellationToken = CancelSource.Token;
            parallelOptions.MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 2, 1);

            Task.Run(() => {
                while(!parallelOptions.CancellationToken.IsCancellationRequested)
                {
                    if (PendingSurfels.Count == 0)
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
                    else
                    {
                        Thread.Yield();
                    }
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
                return Eye + new Vector3(S * 20.0f, C * 20.0f, 15.0f);
            };

            LightPoints[0] = FindLightPosition(1.0, 0.0 / 3.0);
            LightPoints[1] = FindLightPosition(2.0, 1.0 / 3.0);
            LightPoints[2] = FindLightPosition(-4.0, 2.0 / 3.0);
        }

        WorldToView = Matrix4x4.CreateLookTo(
            Eye,
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
                    (Vector3 Position, Vector3 Color) = Surfel;

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

        double CadenceMs = FrameRate.Average();

        double Hz = 1.0 / CadenceMs * 1000.0;

        const long PerfLogFrequency = TimeSpan.TicksPerSecond * 5;
        if (DateTime.UtcNow.Ticks - LastPerfLog >= PerfLogFrequency)
        {
            if (LastPerfLog > 0)
            {
                double UpdatesPerFrame = SplatCopyCount.Average();
                double UpdateProcessingMs = SplatCopyTime.Average();
                double Efficiency = (UpdatesPerFrame / Settings.MaxSurfels) * 100.0;

                double ConvergenceTimeMs = ((Settings.MaxSurfels / UpdatesPerFrame) - 1) * CadenceMs;

                //TracingRate = Math.Max(MinTracingRate, (int)UpdatesPerFrame);

                Console.Write(
                    "\n\n" +
                    " +- Cadence ------------------------------------------------------------------+\n" +
                    " |\n" +
                    $" |           Frequency : {Math.Round(Hz, 1)} hz\n" +
                    $" |            Interval : {Math.Round(CadenceMs, 1)} ms\n" +
                    " |\n" +
                    " +- Shading ------------------------------------------------------------------+\n" +
                    " |\n" +
                    $" |          Throughput : {Math.Round(UpdatesPerFrame, 0)} ({Math.Round(Efficiency, 2)}%)\n" +
                    $" |           Sync Time : {Math.Round(UpdateProcessingMs, 2)} ms\n" +
                    $" |    Convergence Time : {Math.Round(ConvergenceTimeMs, 2)} ms\n" +
                    " |\n" +
                    " +- Analysis -----------------------------------------------------------------+\n" +
                    " |\n"
                );

                if (Efficiency == 100.0)
                {
                    Console.WriteLine(" |    Convergence time is perfect.");
                }
                else if (UpdateProcessingMs < 4.0)
                {
                    Console.WriteLine(" |    Convergence time is bottlenecked on shading throughput.");
                }
                else if (UpdateProcessingMs >= 4.0)
                {
                    Console.WriteLine(" |    Convergence time is bottlenecked on Synchronization.");
                }
                Console.WriteLine(" |\n +\n");
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

        int CurrentTracingRate = Settings.TracingRate;

        int SliceSize = CurrentTracingRate;
        int SliceCount = 1;

        if (CurrentTracingRate > parallelOptions.MaxDegreeOfParallelism)
        {
            int MaximumBatchSize = 16;
            SliceSize = Math.Min(CeilDivide(CurrentTracingRate, parallelOptions.MaxDegreeOfParallelism), MaximumBatchSize);
            SliceCount = CeilDivide(CurrentTracingRate, SliceSize);
            SliceSize = CeilDivide(CurrentTracingRate, SliceCount);
        }

        Vector3 MissColor = new Vector3(0.2f, 0.2f, 0.2f);

        Parallel.For(0, SliceCount, parallelOptions, (SliceIndex) =>
        {
            int SliceStart = SliceIndex * SliceSize;
            int SliceStop = Math.Min(CurrentTracingRate, SliceStart + SliceSize);
            int Range = SliceStop - SliceStart;

            var NewSurfels = new SurfelList(Range);

            var SplatRNG = new Random();

            for (int BatchIndex = 0; BatchIndex < Range; ++BatchIndex)
            {
                int Cursor = SliceStart + BatchIndex;

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
                    ClipTarget.Y = (FrustumY / (float)FrustaCountY * 2.0f - 1.0f);
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

                var Start = Eye;
                var Stop = RayDir * 100_000.0f + Eye;

                if (Start != Stop)
                {
                    (bool Hit, Vector3 Position) = Model.Trace(Start, Stop);
                    if (Hit)
                    {
                        var Normal = Model.Gradient(Position);

                        var SplatColor = new Vector3(0.0f, 0.0f, 0.0f);
                        for (int LightIndex = 0; LightIndex < LightPoints.Length; ++LightIndex)
                        {
                            var LightPoint = LightPoints[LightIndex];//Vector3.Transform(LightPoints[LightIndex], WorldToLocal);
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
                        NewSurfels.Add((Position, SplatColor));
                        continue;
                    }
                }
                {
                    //NewSurfels.Add((new Vector4(Stop, 1.0f), new Vector4(-RayDir, 1.0f), Color.CornflowerBlue));
                    NewSurfels.Add((Stop, MissColor));
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
