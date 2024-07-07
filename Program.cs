
using System.Threading;
using SDL3;
using static SDL3.SDL;


namespace StarMachine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (SDL_Init(SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("SDL3 initialization failed.");
                return;
            }

            var Window = SDL_CreateWindow("Star Machine"u8, 1024, 1024, 0);
            if (Window == IntPtr.Zero)
            {
                Console.WriteLine("SDL3 failed to create a window.");
                return;
            }

            Console.WriteLine("Ignition successful.");

            while (true)
            {
                SDL_Event Event;
                if (SDL_PollEvent(out Event) != 0)
                {
                    if (Event.type == SDL.SDL_EventType.SDL_EVENT_QUIT)
                    {
                        break;
                    }
                }

                Thread.Yield();
            }

            SDL_DestroyWindow(Window);
        }
    }
}
