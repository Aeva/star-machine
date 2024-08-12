
using System.Runtime.InteropServices;

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

    public SVGResource(string Name)
    {
        MallocResource(Name);
        unsafe
        {
            void* VoidStar = Ptr.ToPointer();
            Handle = plutosvg_document_load_from_data(VoidStar, Size, 640, 480, FreeCallback, VoidStar);
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
