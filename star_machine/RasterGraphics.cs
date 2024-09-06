
using System.Diagnostics;
using System.Runtime.CompilerServices;

using SDL3;
using static SDL3.SDL;

using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

using Moloch;
using static Moloch.BlobHelper;


namespace StarMachine;


public class ImageResource
{
    public string Name = "";
    public IntPtr Surface = IntPtr.Zero;
    public readonly int Width;
    public readonly int Height;

    private ImageResource(string InName, IntPtr InSurface)
    {
        Name = InName;
        Surface = InSurface;
        SDL_Rect ClipRect;
        unsafe
        {
            SDL_GetSurfaceClipRect(Surface, &ClipRect);
        }
        Trace.Assert(ClipRect.x == 0);
        Trace.Assert(ClipRect.y == 0);
        Width = ClipRect.w;
        Height = ClipRect.h;
    }

    public static ImageResource CreateFromData(
        string Name, int Width, int Height, byte[] Data, SDL.SDL_PixelFormat PixelFormat)
    {
        IntPtr Surface = IntPtr.Zero;
        unsafe
        {
            int Stride = Data.Length / Height;
            fixed (void* Pixels = Data)
            {
                Surface = SDL_CreateSurfaceFrom(Width, Height, PixelFormat, Pixels, Stride);
            }
        }
        return new ImageResource(Name, Surface);
    }

    public static ImageResource LoadBMP(string Name)
    {
        byte[] ResourceData = Resources.Read(Name);
        unsafe
        {
            fixed(byte* ResourceDataPtr = ResourceData)
            {
                IntPtr MemoryStream = SDL_IOFromMem(ResourceDataPtr, ResourceData.Length);
                IntPtr Surface = SDL_LoadBMP_IO(MemoryStream, 1);
                return new ImageResource(Name, Surface);
            }
        }
    }

    public void Free()
    {
        if (Surface != IntPtr.Zero)
        {
            SDL_DestroySurface(Surface);
            Surface = IntPtr.Zero;
        }
    }

    ~ImageResource()
    {
        Free();
    }

    public Vector4 Read(int X, int Y)
    {
        unsafe
        {
            X = Int32.Min(Int32.Max(X, 0), Width - 1);
            Y = Int32.Min(Int32.Max(Y, 0), Height - 1);
            float R = 0.0f;
            float G = 0.0f;
            float B = 0.0f;
            float A = 0.0f;
            int Status = SDL_ReadSurfacePixelFloat(Surface, X, Y, &R, &G, &B, &A);
            Vector4 Texel;
            Texel.X = R;
            Texel.Y = G;
            Texel.Z = B;
            Texel.W = A;
            return Texel;
        }
    }

    public Vector4 Read(float X, float Y)
    {
        return Read((int)X, (int)Y);
    }

    public Vector4 SampleLinear(Vector2 UV)
    {
        Vector2 WH = new((float)Width, (float)Height);
        Vector2 XY = WH * UV;
        Vector2 FloorXY = new(Single.Floor(XY.X), Single.Floor(XY.Y));
        Vector2 MaxXY = WH - Vector2.One;

        Vector2 Low = Vector2.Min(Vector2.Max(FloorXY, Vector2.Zero), MaxXY);
        Vector2 High = Vector2.Min(Vector2.Max(FloorXY + Vector2.One, Vector2.Zero), MaxXY);
        Vector2 Alpha = Low - XY;

        return Vector4.Lerp(
            Vector4.Lerp(Read(Low.X, Low.Y), Read(High.X, Low.Y), Alpha.X),
            Vector4.Lerp(Read(Low.X, High.Y), Read(High.X, High.Y), Alpha.X),
            Alpha.Y);
    }

    public Vector4 SampleLinear(float U, float V)
    {
        Vector2 UV = new(U, V);
        return SampleLinear(UV);
    }

    public delegate float DesaturateMixin(Vector4 Color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Print(DesaturateMixin Thunk)
    {
        const float TextAspect = 32.0f / 14.0f;
        const float StepU = 1.0f / 100.0f;
        const float StepV = StepU * TextAspect;

        string[] TextGradient = {"▁▂▃▄▅▆▇█", "▏▎▍▌▋▊▉█"};

        float GradientSteps = Int32.Min(TextGradient[0].Length, TextGradient[1].Length);
        float MaxGradientIndex = (float)(GradientSteps - 1);

        int CheckerY = 0;
        for (float V = 0.0f; V <= 1.0f; V += StepV)
        {
            int CheckerX = CheckerY;
            string Row = "";
            for (float U = 0.0f; U <= 1.0f; U += StepU)
            {
                float Gray = Thunk(SampleLinear(U, V));
                //var Color = SampleLinear(U, V);
                //float Gray = (Color.X + Color.Y + Color.Z) / 3.0f;
                int ShadeIndex = (int)Single.Min(Single.Max(Gray * MaxGradientIndex, 0.0f), MaxGradientIndex);
                Row += TextGradient[CheckerX][ShadeIndex];

                CheckerX = (CheckerX + 1) % 2;
            }
            Console.WriteLine(Row);
            CheckerY = (CheckerY + 1) % 2;
        }
    }

    public void Print(bool PreMultipliedAlpha)
    {
        if (PreMultipliedAlpha)
        {
            DesaturateMixin Thunk = (Vector4 Color) =>
            {
                return (Color.X + Color.Y + Color.Z) / 3.0f;
            };
            Print(Thunk);
        }
        else
        {
            DesaturateMixin Thunk = (Vector4 Color) =>
            {
                return ((Color.X + Color.Y + Color.Z) / 3.0f) * Color.W;
            };
            Print(Thunk);
        }
    }
}
