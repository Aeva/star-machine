
using System.Runtime.InteropServices;
using static System.Buffer;

using SDL3;
using static SDL3.SDL;

using PlutoVG;
using static PlutoVG.PlutoVG;

using PlutoSVG;
using static PlutoSVG.PlutoSVG;

using Moloch;
using static Moloch.BlobHelper;

namespace StarMachine;


public class FontResource : ResourceBlob
{
    private unsafe plutovg_font_face_t* Handle = null;

    public FontResource(string Name, int FontIndex = 0)
    {
        MallocResource(Name);
        unsafe
        {
            void* VoidStar = Ptr.ToPointer();
            Handle = plutovg_font_face_load_from_data(VoidStar, (uint)Size, FontIndex, FreeCallback, VoidStar);
        }
    }

    protected override void Free()
    {
        unsafe
        {
            if (Handle != null)
            {
                plutovg_font_face_destroy(Handle);
            }
        }
    }
}


public class SVGResource : ResourceBlob
{
    private unsafe plutosvg_document_t* Handle = null;
    private bool Rendered = false;
    private (int W, int H) RenderedSize = (-1, -1);
    private (int W, int H, byte[] Data) Cached;

    public (int W, int H, byte[] Data) Render(int RequestWidth=-1, int RequestHeight=-1)
    {
        if (!Rendered || RenderedSize.W != RequestWidth || RenderedSize.H != RequestHeight)
        {
            Rendered = true;
            RenderedSize = (RequestWidth, RequestHeight);
            unsafe
            {
                plutovg_surface_t* Surface = plutosvg_document_render_to_surface(
                    Handle, null, RequestWidth, RequestHeight, null, null, null);

                byte* Data = plutovg_surface_get_data(Surface);
                int Stride = plutovg_surface_get_stride(Surface);

                int SurfaceWidth = plutovg_surface_get_width(Surface);
                int SurfaceHeight = plutovg_surface_get_height(Surface);
                int Size = Stride * SurfaceHeight;
                var Blob = new byte[Size];
                fixed (byte* BlobPtr = Blob)
                {
                    MemoryCopy(Data, BlobPtr, Size, Size);
                }
                Cached = (SurfaceWidth, SurfaceHeight, Blob);

                plutovg_surface_destroy(Surface);
            }
        }

        return (Cached.W, Cached.H, Cached.Data);
    }

    public SVGResource(string Name)
    {
        MallocResource(Name);
        unsafe
        {
            void* VoidStar = Ptr.ToPointer();
            Handle = plutosvg_document_load_from_data(VoidStar, Size, -1, -1, FreeCallback, VoidStar);
        }
    }

    protected override void Free()
    {
        unsafe
        {
            if (Handle != null)
            {
                plutosvg_document_destroy(Handle);
            }
        }
    }
}
