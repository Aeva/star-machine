
using System.Reflection;
using static System.Buffer;

using SDL3;
using static SDL3.SDL;

using Fixie = FixedPoint.Fixie;


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

    // View space near plane distance.
    public double NearPlane = 0.001;

    // Start in fullscreen mode.
    public bool Fullscreen = true;

    // Whether to use paraboloids or discs.
    public bool ParaboloidSplats = true;
}


public struct FrameInfo
{
    public long Start;
    public double ElapsedMs;
    public double RunTimeMs;
    public int Width;
    public int Height;
    public bool Resize;
    public float AspectRatio;
}


struct PerformerStatus
{
    public bool Up;
    public bool Left;
    public bool Down;
    public bool Right;

    public float Turn;
    public float TurnL;
    public float TurnR;

    public float Clutch;
    public float Brake;
    public float Gas;

    // Left and right stick contributions to the break.
    public float BrakeL;
    public float BrakeR;

    public bool Paused;
    public bool Reset;

    public void UpdateTurn()
    {
        if (TurnL > TurnR)
        {
            Turn = TurnL * TurnL * TurnL * -1.0f;
        }
        else if (TurnL < TurnR)
        {
            Turn = TurnR * TurnR * TurnR;
        }
        else
        {
            Turn = 0.0f;
        }
    }
}


internal class Program
{
    static public RenderingConfig Settings = new();

    static void LoadIcon(IntPtr Window)
    {
        const string ResourceName = "StarMachine.star_machine.bmp";
        using (Stream? ResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
        {
            if (ResourceStream != null)
            {
                byte[] ResourceData = new byte[ResourceStream.Length];
                ResourceStream.Read(ResourceData, 0, (int)ResourceStream.Length);

                unsafe
                {
                    fixed(byte* ResourceDataPtr = ResourceData)
                    {
                        IntPtr MemoryStream = SDL_IOFromMem(ResourceDataPtr, ResourceData.Length);
                        IntPtr IconSurface = SDL_LoadBMP_IO(MemoryStream, 1);
                        if (SDL_SetWindowIcon(Window, IconSurface) < 0)
                        {
                            // Seems SDL3 can't set the window icon this way on wayland???
                            //Console.WriteLine(SDL_GetError());
                        }
                        SDL_DestroySurface(IconSurface);
                    }
                }
            }
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

    static (float, float) ReadPedalValues(Int16 RawValue)
    {
        float Value = ReadAxisValue(RawValue) * -1.0f;
        if (Value > 0.0f)
        {
            return (Value, 0.0f);
        }
        else if (Value < 0.0f)
        {
            return (0.0f, Value);
        }
        else
        {
            return (0.0f, 0.0f);
        }
    }

    static void Main(string[] args)
    {
#if true
        {
            Fixie.PreflightCheck();
        }
#endif

#if true
        Console.WriteLine("Embedded resources:");
        string[] ResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        foreach (string ResourceName in ResourceNames)
        {
            Console.WriteLine($" - {ResourceName}");
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

            LastFrame.Start = DateTime.UtcNow.Ticks;
            LastFrame.ElapsedMs = 0.0;
            LastFrame.RunTimeMs = 0.0;
            (LastFrame.Width, LastFrame.Height) = SDL_GetWindowSizeInPixels(LowRenderer.Window);
            LastFrame.Resize = true;
            LastFrame.AspectRatio = (float)LastFrame.Width / (float)LastFrame.Height;

            var HighRenderer = new HighLevelRenderer(Settings);
            HighRenderer.Boot(LastFrame);

            var Game = new CharacterController(HighRenderer);

            PerformerStatus PlayerState = new();
            IntPtr GamePad = 0;

            foreach (UInt32 Joystick in SDL.SDL_GetJoysticks())
            {
                GamePad = SDL_OpenGamepad(Joystick);
                break;
            }

            double IdleRumbleLow = 0.0;
            double IdleRumbleHigh = 100.0;

            while (!Halt)
            {
                ThisFrame.Start = DateTime.UtcNow.Ticks;
                ThisFrame.ElapsedMs = (double)(ThisFrame.Start - LastFrame.Start) / (double)TimeSpan.TicksPerMillisecond;
                ThisFrame.RunTimeMs = LastFrame.RunTimeMs + ThisFrame.ElapsedMs;
                PlayerState.Reset = false;

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
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_DOWN:
                                PlayerState.Down = true;
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
                        }
                    }
                    else if (Event.type == SDL.SDL_EventType.SDL_EVENT_KEY_UP)
                    {
                        switch(Event.key.scancode)
                        {
                            case SDL.SDL_Scancode.SDL_SCANCODE_UP:
                                PlayerState.Up = false;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_DOWN:
                                PlayerState.Down = false;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_LEFT:
                                PlayerState.Left = false;
                                break;

                            case SDL.SDL_Scancode.SDL_SCANCODE_RIGHT:
                                PlayerState.Right = false;
                                break;
                        }
                    }
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
                    if (Event.type == SDL.SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION)
                    {
                        switch(Event.gaxis.axis)
                        {
                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTY:
                                (PlayerState.Clutch, PlayerState.BrakeL) = ReadPedalValues(Event.gaxis.value);
                                PlayerState.Brake = Math.Abs(Math.Min(PlayerState.BrakeL, PlayerState.BrakeR));
                                break;

                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTY:
                                (PlayerState.Gas, PlayerState.BrakeR) = ReadPedalValues(Event.gaxis.value);
                                PlayerState.Brake = Math.Abs(Math.Min(PlayerState.BrakeL, PlayerState.BrakeR));
                                break;

                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFT_TRIGGER:
                                PlayerState.TurnL = ReadAxisValue(Event.gaxis.value);
                                PlayerState.UpdateTurn();
                                break;

                            case SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHT_TRIGGER:
                                PlayerState.TurnR = ReadAxisValue(Event.gaxis.value);
                                PlayerState.UpdateTurn();
                                break;

                        }
                    }
                }

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

                if (!Halt)
                {
                    (ThisFrame.Width, ThisFrame.Height) = SDL_GetWindowSizeInPixels(LowRenderer.Window);
                    ThisFrame.Resize = LastFrame.Width != ThisFrame.Width || LastFrame.Height != ThisFrame.Height;
                    ThisFrame.AspectRatio = (float)ThisFrame.Height / (float)ThisFrame.Width;

                    Game.Advance(ThisFrame, PlayerState);
                    HighRenderer.Advance(ThisFrame, PlayerState, Game);
                    Halt = LowRenderer.Advance(ThisFrame, Settings, HighRenderer);

                    LastFrame = ThisFrame;
                }
            }

            HighRenderer.Teardown();
        }

        LowRenderer.Teardown();
    }
}
