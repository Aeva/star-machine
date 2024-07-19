
using System.Reflection;
using static System.Buffer;

using SDL3;
using static SDL3.SDL;

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
    public bool Left;
    public bool Right;
    public bool Up;
    public bool Down;
    public bool Paused;
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

    static void Main(string[] args)
    {
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
