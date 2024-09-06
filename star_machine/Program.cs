
using System.Reflection;
using System.Text.RegularExpressions;
using static System.Buffer;

using SDL3;
using static SDL3.SDL;

using FixedPointTests = FixedPoint.FixedPointTests;
using FixedInt = FixedPoint.FixedInt;
using System.Reflection.Emit;


namespace StarMachine;


public class RenderingConfig
{
    // This determines the maximum number of surfels that will be rendered
    // every frame, which effectively controls the point density as well as
    // Convergence time.  If set too high, frame drops can occur.  If set
    // too low, the image will look chunky.
    public int MaxSurfels  = 50_000;

    // This is the target number of new surfels to trace every generation.
    // This tracing is performed asynchronously, but the tracing rate should
    // roughly match the amount of surfels that can be traced in one vsync
    // interval to ensure a good screen space distribution and fill rate.
    // Setting this a bit higher than expected may sometimes produce better
    // results, however.
    public int TracingRate = 1_800;

    // View space splat depth.
    public float SplatDepth = 0.01f;

    // Number of vertices in each vertex ring.
    public int[] SplatRings = {1, 5, 50};

    // Field of view, degrees.
    public double FieldOfView = 60;

    // Pupilary distance.  Set to 0 to disable stereo rendering.
#if true
    public float PupilaryDistance = 0.0f;
#else
    public float PupilaryDistance = 62.0f * 0.001f * 4.0f; // 62 mm
#endif

    // View space near plane distance.
    public double NearPlane = 0.001;

    // Start in fullscreen mode.
    public bool Fullscreen = true;

    // Preferred present mode.
    public SDL_GpuPresentMode PresentMode = SDL_GpuPresentMode.SDL_GPU_PRESENTMODE_VSYNC;

    // Preferred swapchain composition.
    public SDL_GpuSwapchainComposition SwapchainComposition = SDL.SDL_GpuSwapchainComposition.SDL_GPU_SWAPCHAINCOMPOSITION_SDR;

    // When true, this enables validation layers.
    public bool DebugMode = false;

    // Whether to use paraboloids or discs.
    public bool ParaboloidSplats = true;

    public bool ShowCadenceStats = true;
    public bool ShowShadingStats = true;
    public bool ShowGameState = true;
}


public struct FrameInfo
{
    public long Number;
    public long Start;
    public double ElapsedMs;
    public double RunTimeMs;
    public int Width;
    public int Height;
    public bool Resize;
    public float AspectRatio;
}


struct JoyStick
{
    public double X;
    public double Y;

    public double Mag;
    public double Angle;
    public double DeltaAngle;

    public void Update()
    {
        double LastMag = Mag;
        double LastAngle = Angle;

        X = Double.Clamp(X, -1.0, 1.0);
        Y = Double.Clamp(Y, -1.0, 1.0);
        double Mag2 = X * X + Y * Y;
        if (Mag2 > 0.0)
        {
            Mag = Double.Sqrt(Mag2);
            double InvMag = 1.0 / Mag;
            Mag = Double.Min(Mag, 1.0);

            // The args of Atan2 are intentionally reversed here to ensure
            // positive Y is the zero angle point.
            Angle = Double.Atan2(X * InvMag, Y * InvMag) / Double.Pi;
        }
        else
        {
            Angle = 0.0;
            Mag = 0.0;
        }

        if (LastMag == 0.0)
        {
            LastAngle = Angle;
        }

        {
            double Trend = DeltaAngle < 0.0 ? -1.0 : 1.0;

            double Dist = Double.Abs(LastAngle - Angle);
            if (Dist == 0.0)
            {
                DeltaAngle = 0.0;
            }
            else if (Dist < 1.0)
            {
                Trend = (Angle < LastAngle) ? -1.0 : 1.0;
            }
            else if (Dist > 1.0)
            {
                Trend = (Angle < LastAngle) ? 1.0 : -1.0;
                Dist = 2.0 - Dist;
            }

            DeltaAngle = Dist * Trend;
        }
    }
}


struct PerformerStatus
{
    public bool Up;
    public bool Left;
    public bool Down;
    public bool Right;

    public float Turn;

    //public float Clutch;
    public float Brake;
    public float Gas;

    public bool Paused;

    // For debugging
    public bool Reset;
    public bool HardStop;
    public bool Align;
    public bool Clear;
}


internal class Program
{
    static public RenderingConfig Settings = new();

    static void LoadIcon(IntPtr Window)
    {
        var Icon = ImageResource.LoadBMP("StarMachine.star_machine.bmp");
        if (SDL_SetWindowIcon(Window, Icon.Surface) < 0)
        {
            // Seems SDL3 can't set the window icon this way on wayland???
            //Console.WriteLine(SDL_GetError());
        }
    }

    static float ReadAxisValue(Int16 RawValue, float Threshold = 0.2f)
    {
        float Scale = 1.0f / (1.0f - Threshold);
        float Sign = 1.0f;
        float Mag = 0.0f;
        if (RawValue >= 0)
        {
            Mag = (float)RawValue / 32767.0f;
        }
        else
        {
            Sign = -1.0f;
            Mag = (float)RawValue / -32768.0f;
        }
        if (Mag < Threshold)
        {
            return 0.0f;
        }
        else
        {
            return (Mag - Threshold) * Scale * Sign;
        }
    }

    static void Main(string[] Args)
    {
        for (int ArgIndex = 0; ArgIndex < Args.Length; ++ArgIndex)
        {
            string Arg = Args[ArgIndex];
            int Remaining = (Args.Length - 1) - ArgIndex;
            if (Arg == "--vsync")
            {
                Settings.PresentMode = SDL_GpuPresentMode.SDL_GPU_PRESENTMODE_VSYNC;
            }
            else if (Arg == "--immediate" || Arg == "--novsync")
            {
                Settings.PresentMode = SDL_GpuPresentMode.SDL_GPU_PRESENTMODE_IMMEDIATE;
            }
            else if (Arg == "--mailbox")
            {
                Settings.PresentMode = SDL_GpuPresentMode.SDL_GPU_PRESENTMODE_MAILBOX;
            }
            else if (Arg == "--fullscreen")
            {
                Settings.Fullscreen = true;
            }
            else if (Arg == "--windowed")
            {
                Settings.Fullscreen = false;
            }
            else if (Arg == "--resources")
            {
                Console.WriteLine("Embedded resources:");
                string[] ResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                foreach (string ResourceName in ResourceNames)
        {
                    Console.WriteLine($" - {ResourceName}");
                }
                return;
            }
            else if (Arg == "--test-pattern")
            {
                int Span = 8;
                float Tail = Single.Sqrt(2.0f) * (float)Span;
                var Image = ImageResource.CreateEmpty("Test Pattern", Span, Span);
                for (int Y = 0; Y < Image.Height; ++Y)
                {
                    for (int X = 0; X < Image.Width; ++X)
                    {
                        float N = 1.0f - Single.Min(Single.Max(Single.Abs((float)X - (float)Y) / Tail, 0.0f), 1.0f);
                        Image.Write(X, Y, N, 0.0f, 0.0f, 1.0f);
                    }
                }
                Image.PrintRed();
                return;
            }
            else if (Arg == "--print" && Remaining > 0)
            {
                string QueryName = Args[++ArgIndex];
                string[] ResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                foreach (string ResourceName in ResourceNames)
                {
                    if (QueryName == ResourceName)
                    {
                        Console.WriteLine(ResourceName);
                        if (ResourceName.EndsWith(".bmp"))
                        {
                            var Image = ImageResource.LoadBMP(ResourceName);
                            Image.Print(false);
                        }
                        else if (ResourceName.EndsWith(".svg"))
                        {
                            // PlutoSVG calls crash if PlutoVG was not invoked first.
                            PlutoVG.PlutoVG.plutovg_version_string();
                            var SVG = new SVGResource(ResourceName);
                            (int W, int H, byte[] Data) = SVG.Render(100, -1);
                            var Format = SDL.SDL_PixelFormat.SDL_PIXELFORMAT_BGRA8888;
                            var Image = ImageResource.CreateFromData(ResourceName, W, H, Data, Format);
                            Image.Print(true);
                        }
                        else if (ResourceName.EndsWith(".ttf"))
                        {
                            var Font = new FontResource(ResourceName);
                            (int W, int H, byte[] Data) = Font.Render("mph", 64);
                            var Format = SDL.SDL_PixelFormat.SDL_PIXELFORMAT_BGRA8888;
                            var Image = ImageResource.CreateFromData(ResourceName, W, H, Data, Format);
                            Image.Print(true);
                        }
                        return;
                    }
                }
                Console.WriteLine($"No resource named {QueryName}");
                return;
            }
            else if (Arg == "--pd" && Remaining > 0)
            {
                float PupilaryDistance = Single.Max(Single.Parse(Args[++ArgIndex]), 0.0f);
                Console.WriteLine($"Pupilary distance: {PupilaryDistance} mm");
                Settings.PupilaryDistance = PupilaryDistance * 0.001f * 4.0f;
            }
        }

#if true
        {
            FixedPointTests.PreflightCheck();
            UnitsTests.PreflightCheck();
        }
#endif

        var SplatMesh = new SplatGenerator(Settings);
        var LowRenderer = new LowLevelRenderer(SplatMesh);
        bool Halt = LowRenderer.Boot(Settings);
        if (!Halt)
        {
            LoadIcon(LowRenderer.Window);

            FrameInfo LastFrame;
            FrameInfo ThisFrame;

            LastFrame.Number = -1;
            ThisFrame.Number = 0;

            LastFrame.Start = DateTime.UtcNow.Ticks;
            LastFrame.ElapsedMs = 0.0;
            LastFrame.RunTimeMs = 0.0;
            (LastFrame.Width, LastFrame.Height) = SDL_GetWindowSizeInPixels(LowRenderer.Window);
            LastFrame.Resize = true;
            LastFrame.AspectRatio = (float)LastFrame.Width / (float)LastFrame.Height;

            var HighRenderer = new HighLevelRenderer(Settings);
            HighRenderer.Boot(LastFrame);

            var Game = new CharacterController(HighRenderer, LowRenderer);

            PerformerStatus PlayerState = new();
            IntPtr GamePad = 0;

            foreach (UInt32 Joystick in SDL.SDL_GetJoysticks())
            {
                GamePad = SDL_OpenGamepad(Joystick);
                break;
            }

            JoyStick LeftStick = new JoyStick();
            JoyStick RightStick = new JoyStick();

            //double IdleRumbleLow = 0.0;
            //double IdleRumbleHigh = 100.0;

            SDL.SDL_HideCursor();

            var Screen = new RootWidget();
            LowRenderer.Overlay = Screen;

            TextWidget? PerfFrequency = null;
            TextWidget? PerfInterval = null;
            TextWidget? PerfThroughput = null;
            TextWidget? PerfConvergence = null;
            TextWidget? PerfSyncTime = null;
            TextWidget? DebugX = null;
            TextWidget? DebugY = null;
            TextWidget? Heading = null;

            TextWidget? DebugSpeedValue = null;
            TextWidget? DebugSpeedLabel = null;

            {
                float FontSize = 0.5f;
                float Margin = 0.1f;
                float LineSpacing = -FontSize * 1.25f;
                float Indent = FontSize * 17.0f + Margin;
                float Line = -Margin + LineSpacing;

                var AddStat = (string Stat) =>
                {
                    var Label = new TextWidget(LowRenderer.Device, Stat, FontSize, "Michroma-Regular.ttf");
                    Label.AlignX = 1.0f;
                    Label.AlignY = 1.0f;
                    Label.Move(Indent, Line);
                    Screen.TopLeft.Attachments.Add(Label);

                    var Value = new TextWidget(LowRenderer.Device, " 0", FontSize, "Michroma-Regular.ttf");
                    Value.AlignX = -1.0f;
                    Value.AlignY = 1.0f;
                    Value.Move(Indent, Line);
                    Screen.TopLeft.Attachments.Add(Value);
                    Line += LineSpacing;
                    return (Label, Value);
                };

                if (Settings.ShowCadenceStats)
                {
                    (_, PerfFrequency) = AddStat("frequency (hz): ");
                    (_, PerfInterval) = AddStat("interval (ms): ");
                    Line += LineSpacing;
                }
                if (Settings.ShowShadingStats)
                {
                    (_, PerfThroughput) = AddStat("splats per frame: ");
                    (_, PerfConvergence) = AddStat("convergence time (ms): ");
                    (_, PerfSyncTime) = AddStat("sync time (ms): ");
                    Line += LineSpacing;
                }
            }
            {
                float FontSize = 0.5f;
                float Margin = 0.1f;
                float LineSpacing = -FontSize * 1.25f;
                float Indent = FontSize * 14.0f + Margin;
                float Line = -Margin + LineSpacing;

                var AddStat = (string Stat) =>
                {
                    var Label = new TextWidget(LowRenderer.Device, Stat, FontSize, "Michroma-Regular.ttf");
                    Label.AlignX = 1.0f;
                    Label.AlignY = 1.0f;
                    Label.Move(Indent, Line);
                    Screen.TopCenter.Attachments.Add(Label);

                    var Value = new TextWidget(LowRenderer.Device, " 0", FontSize, "Michroma-Regular.ttf");
                    Value.AlignX = -1.0f;
                    Value.AlignY = 1.0f;
                    Value.Move(Indent, Line);
                    Screen.TopCenter.Attachments.Add(Value);
                    Line += LineSpacing;
                    return (Label, Value);
                };
                if (Settings.ShowGameState)
                {
                    (_, DebugX) = AddStat("x (miles): ");
                    (_, DebugY) = AddStat("y (miles): ");
                    (_, Heading) = AddStat("heading: ");
                    Line += LineSpacing;

                    (DebugSpeedLabel, DebugSpeedValue) = AddStat("speed (mph): ");
                    Line += LineSpacing;
                }
            }

            const float SpeedometerSize = 7.0f;
            const float SpeedometerOffset = SpeedometerSize / 2.4f;

            /*
            var SpeedometerShadow = new SvgWidget(LowRenderer.Device, "speedometer_shadow.svg", SpeedometerSize, SpeedometerSize);
            SpeedometerShadow.OrderHint = -3;
            SpeedometerShadow.Move(0.0f, SpeedometerOffset);
            Screen.BottomCenter.Attachments.Add(SpeedometerShadow);
            */

            var SpeedometerDial = new SvgWidget(LowRenderer.Device, "speedometer_dial.svg", SpeedometerSize, SpeedometerSize);
            SpeedometerDial.OrderHint = -2;
            SpeedometerDial.Move(0.0f, SpeedometerOffset);
            Screen.BottomCenter.Attachments.Add(SpeedometerDial);

            var SpeedometerNeedle = new SvgWidget(LowRenderer.Device, "speedometer_needle.svg", SpeedometerSize, SpeedometerSize);
            SpeedometerNeedle.OrderHint = -1;
            SpeedometerNeedle.Move(0.0f, SpeedometerOffset);
            Screen.BottomCenter.Attachments.Add(SpeedometerNeedle);

            Screen.Rebuild();

            while (!Halt)
            {
                ThisFrame.Start = DateTime.UtcNow.Ticks;
                ThisFrame.ElapsedMs = (double)(ThisFrame.Start - LastFrame.Start) / (double)TimeSpan.TicksPerMillisecond;
                ThisFrame.RunTimeMs = LastFrame.RunTimeMs + ThisFrame.ElapsedMs;

                SDL_Event Event;
                while (SDL_PollEvent(out Event) != 0 && !Halt)
                {
                    if (Event.type == SDL.SDL_EventType.SDL_EVENT_QUIT)
                    {
                        Halt = true;
                        break;
                    }
                    else if (Event.type == SDL.SDL_EventType.SDL_EVENT_KEY_DOWN)
                    {
                        switch(Event.key.scancode)
                        {
                            case SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE:
                                Halt = true;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_UP:
                                PlayerState.Up = true;
                                PlayerState.Gas = 1.0f;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_DOWN:
                                PlayerState.Down = true;
                                PlayerState.Brake = 1.0f;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_LEFT:
                                PlayerState.Left = true;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_RIGHT:
                                PlayerState.Right = true;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_P:
                                PlayerState.Paused = !PlayerState.Paused;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_R:
                                PlayerState.Reset = true;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_H:
                                PlayerState.HardStop = true;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_A:
                                PlayerState.Align = true;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_C:
                                PlayerState.Clear = true;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_W:
                                Console.WriteLine($"Current position: {HighRenderer.Eye}");
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_Z:
                                ThisFrame.Number = 0;
                                Game.WarpMode = true;
                                break;
                        }
                    }
                    else if (Event.type == SDL.SDL_EventType.SDL_EVENT_KEY_UP)
                    {
                        switch (Event.key.scancode)
                        {
                            case SDL.SDL_Scancode.SDL_SCANCODE_UP:
                                PlayerState.Up = false;
                                PlayerState.Gas = 0.0f;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_DOWN:
                                PlayerState.Down = false;
                                PlayerState.Brake = 0.0f;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_LEFT:
                                PlayerState.Left = false;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_RIGHT:
                                PlayerState.Right = false;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_R:
                                PlayerState.Reset = false;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_H:
                                PlayerState.HardStop = false;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_A:
                                PlayerState.Align = false;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_C:
                                PlayerState.Clear = false;
                                break;
                        }
                    }
#if false
                    else if (Event.type == SDL.SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN)
                    {
                        switch(Event.gbutton.button)
                        {
                            case SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_UP:
                                PlayerState.Up = true;
                                break;

                            case SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_DOWN:
                                PlayerState.Down = true;
                                break;

                            case SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_LEFT:
                                PlayerState.Left = true;
                                break;

                            case SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_RIGHT:
                                PlayerState.Right = true;
                                break;
                        }
                    }
                    else if (Event.type == SDL.SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP)
                    {
                        switch(Event.gbutton.button)
                        {
                            case SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_UP:
                                PlayerState.Up = false;
                                break;

                            case SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_DOWN:
                                PlayerState.Down = false;
                                break;

                            case SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_LEFT:
                                PlayerState.Left = false;
                                break;

                            case SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_RIGHT:
                                PlayerState.Right = false;
                                break;
                        }
                    }
#endif
                    else if (Event.type == SDL.SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION)
                    {
                        switch(Event.gaxis.axis)
                        {
                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFT_TRIGGER:
                                PlayerState.Brake = ReadAxisValue(Event.gaxis.value);
                                break;

                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHT_TRIGGER:
                                PlayerState.Gas = ReadAxisValue(Event.gaxis.value);
                                break;

                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTX:
                                // left to right is -1 to 1
                                LeftStick.X = ReadAxisValue(Event.gaxis.value);
                                break;

                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTY:
                                // up to down is normally -1 to 1
                                LeftStick.Y = -ReadAxisValue(Event.gaxis.value);
                                break;

                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTX:
                                // left to right is -1 to 1
                                RightStick.X = ReadAxisValue(Event.gaxis.value);
                                break;

                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTY:
                                // up to down is normally -1 to 1
                                RightStick.Y = -ReadAxisValue(Event.gaxis.value);
                                break;
                        }
                    }
                }

                LeftStick.Update();
                RightStick.Update();

                if (GamePad != 0)
                {
                    PlayerState.Turn = (float)(LeftStick.Angle * LeftStick.Mag);
                }

#if false
                if (GamePad != 0)
                {
                    ushort LowVibe = 0;
                    ushort HighVibe = 0;
                    uint Pulse = 0;

#if true
                    {
                        double TurnFrequencyLow = 100.0;
                        double TurnFrequencyHigh = 75.0;

                        IdleRumbleLow += ThisFrame.ElapsedMs;
                        if (IdleRumbleLow > TurnFrequencyLow)
                        {
                            IdleRumbleLow -= TurnFrequencyLow;
                            LowVibe = 0xFFFF;
                            Pulse = 1;
                        }

                        IdleRumbleHigh += ThisFrame.ElapsedMs;
                        if (IdleRumbleHigh > TurnFrequencyHigh)
                        {
                            IdleRumbleHigh -= TurnFrequencyHigh;
                            if (PlayerState.Gas > 0.0f)
                            {
                                LowVibe = 0x100;
                                HighVibe = 0x100;
                                Pulse = 1;
                            }
                        }

                        // IdleRumbleHigh += ThisFrame.ElapsedMs;
                        // if (IdleRumbleHigh > TurnFrequency * 0.5)
                        // {
                        //     IdleRumbleHigh   -= TurnFrequency * 0.5;
                        //     HighVibe = (ushort)((PlayerState.Gas == 0.0f) ? 0x500 : 0x600);
                        //     Pulse = 1;
                        // }
                    }
#else
                    {
                        double TurnFrequency = Double.Lerp(200.0, 150.0, PlayerState.Gas);

                        IdleRumbleLow += ThisFrame.ElapsedMs;
                        if (IdleRumbleLow > TurnFrequency)
                        {
                            IdleRumbleLow -= TurnFrequency;
                            LowVibe = (ushort)((PlayerState.Gas == 0.0f) ? 0x100 : 0x150);
                            Pulse = 1;
                        }

                        IdleRumbleHigh += ThisFrame.ElapsedMs;
                        if (IdleRumbleHigh > TurnFrequency * 0.5)
                        {
                            IdleRumbleHigh   -= TurnFrequency * 0.5;
                            HighVibe = (ushort)((PlayerState.Gas == 0.0f) ? 0x500 : 0x600);
                            Pulse = 4;
                        }
                    }
#endif

                    if (Pulse > 0)
                    {
                        SDL.SDL_RumbleGamepad(GamePad, LowVibe, HighVibe, Pulse);
                    }
                }
#endif

                if (!Halt)
                {
                    (ThisFrame.Width, ThisFrame.Height) = SDL_GetWindowSizeInPixels(LowRenderer.Window);
                    ThisFrame.Resize = LastFrame.Width != ThisFrame.Width || LastFrame.Height != ThisFrame.Height;
                    ThisFrame.AspectRatio = (float)ThisFrame.Height / (float)ThisFrame.Width;

                    Game.Advance(ThisFrame, PlayerState);

                    PerfFrequency?.SetText($" {Double.Round(HighRenderer.CadenceHz, 1):N1}");
                    PerfInterval?.SetText($" {Double.Round(HighRenderer.CadenceMs, 1):N1}");
                    if (!PlayerState.Paused)
                    {
                        PerfThroughput?.SetText($" {Double.Round(HighRenderer.UpdatesPerFrame):N0} ({Double.Round(HighRenderer.Efficiency)}%)");
                        PerfConvergence?.SetText($" {Double.Round(HighRenderer.ConvergenceTimeMs):N1}");
                        PerfSyncTime?.SetText($" {Double.Round(HighRenderer.UpdateProcessingMs, 1):N1}");
                    }
                    DebugX?.SetText($" {(HighRenderer.Eye.X / (4.0f * 1609.344f))}");
                    DebugY?.SetText($" {(HighRenderer.Eye.Y / (4.0f * 1609.344f))}");
                    if (Heading != null)
                    {
                        int Dir = ((int)Single.Round(Game.CurrentHeading / 45.0f) + 8) % 8;
                        switch(Dir)
                        {
                            case 0:
                                Heading.SetText($" North");
                                break;
                            case 1:
                                Heading.SetText($" Northeast");
                                break;
                            case 2:
                                Heading.SetText($" East");
                                break;
                            case 3:
                                Heading.SetText($" Southeast");
                                break;
                            case 4:
                                Heading.SetText($" South");
                                break;
                            case 5:
                                Heading.SetText($" Southwest");
                                break;
                            case 6:
                                Heading.SetText($" West");
                                break;
                            case 7:
                                Heading.SetText($" Northwest");
                                break;
                            default:
                                Heading.SetText($" {Game.CurrentHeading}");
                                break;
                        }
                    }

                    if (Game.MilesPerHour > 7.0)
                    {
                        double RotationMultiplier = (Game.MilesPerHour - 10.0) / -10.0;
                        double DialRotation = 20.0 * RotationMultiplier + 21.0;
                        SpeedometerNeedle.ResetTransform();
                        SpeedometerNeedle.Move(0.0f, SpeedometerOffset);
                        SpeedometerNeedle.Rotate((float)DialRotation);
                    }
                    else
                    {
                        double Alpha = Game.MilesPerHour / 7.0;
                        double RotationMultiplier = (Double.Lerp(6.5, 7.0, Alpha * Alpha * Alpha) - 10.0) / -10.0;
                        double DialRotation = 20.0 * RotationMultiplier + 21.0;
                        SpeedometerNeedle.ResetTransform();
                        SpeedometerNeedle.Move(0.0f, SpeedometerOffset);
                        SpeedometerNeedle.Rotate((float)DialRotation);
                    }

                    if (DebugSpeedLabel != null && DebugSpeedValue != null)
                    {
                        if (Game.SpeedOfLight > 0.0001)
                        {
                            DebugSpeedValue.SetText($" {Game.SpeedOfLight}");
                            DebugSpeedLabel.SetText("speed (c): ");
                        }
                        else if (Game.MilesPerHour > 1.0)
                        {
                            DebugSpeedValue.SetText($" {Double.Round(Game.MilesPerHour)}");
                            DebugSpeedLabel.SetText("speed (mph): ");
                        }
                        else if (Game.MilesPerHour > 0.1)
                        {
                            DebugSpeedValue.SetText($" {Double.Round(Game.MilesPerHour, 1)}");
                            DebugSpeedLabel.SetText("speed (mph): ");
                        }
                        else
                        {
                            DebugSpeedValue.SetText($" {Double.Round(Game.MilesPerHour, 2)}");
                            DebugSpeedLabel.SetText("speed (mph): ");
                        }
                    }

                    HighRenderer.Advance(ThisFrame, PlayerState, Game);
                    Halt = LowRenderer.Advance(ThisFrame, Settings, PlayerState.Clear, HighRenderer);

                    LastFrame = ThisFrame;
                    ThisFrame.Number++;
                }
            }

            HighRenderer.Teardown();
        }

        LowRenderer.Teardown();
    }
}
