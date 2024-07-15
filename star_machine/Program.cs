
using System.Reflection;
using static System.Buffer;

using SDL3;
using static SDL3.SDL;

namespace StarMachine;


public struct FrameInfo
{
    public long Start;
    public double ElapsedMs;
    public int Width;
    public int Height;
    public bool Resize;
    public float AspectRatio;
}


internal class Program
{
    const bool Fullscreen = true;
    const bool ParaboloidSplats = false;
    static readonly int[] SplatRings = {1, 5, 50};

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
        var SplatMesh = new SplatGenerator(ParaboloidSplats, SplatRings);
        var LowRenderer = new LowLevelRenderer(SplatMesh);
        bool Halt = LowRenderer.Boot(Fullscreen);
        if (!Halt)
        {
            LoadIcon(LowRenderer.Window);

            var HighRenderer = new HighLevelRenderer();
            HighRenderer.Boot();

            FrameInfo LastFrame;
            FrameInfo ThisFrame;

            LastFrame.Start = DateTime.UtcNow.Ticks;
            LastFrame.Width = 0;
            LastFrame.Height = 0;

            while (!Halt)
            {
                ThisFrame.Start = DateTime.UtcNow.Ticks;
                ThisFrame.ElapsedMs = (double)(ThisFrame.Start - LastFrame.Start) / (double)TimeSpan.TicksPerMillisecond;

                SDL_Event Event;
                if (SDL_PollEvent(out Event) != 0)
                {
                    if (Event.type == SDL.SDL_EventType.SDL_EVENT_QUIT)
                    {
                        Halt = true;
                        break;
                    }
                    else if (Event.type == SDL.SDL_EventType.SDL_EVENT_KEY_DOWN &&
                        Event.key.scancode == SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE)
                    {
                        Halt = true;
                        break;
                    }
                }

                (ThisFrame.Width, ThisFrame.Height) = SDL_GetWindowSizeInPixels(LowRenderer.Window);
                ThisFrame.Resize = LastFrame.Width != ThisFrame.Width || LastFrame.Height != ThisFrame.Height;
                ThisFrame.AspectRatio = (float)ThisFrame.Height / (float)ThisFrame.Width;

                HighRenderer.Advance(ThisFrame);
                Halt = LowRenderer.Advance(ThisFrame);

                LastFrame = ThisFrame;
            }

            HighRenderer.Teardown();
        }

        LowRenderer.Teardown();
    }
}
