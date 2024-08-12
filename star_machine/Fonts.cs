
using System.Runtime.InteropServices;

using SDL3;
using static SDL3.SDL;

using PlutoVG;
using static PlutoVG.PlutoVG;

using PlutoSVG;
using static PlutoSVG.PlutoSVG;

namespace StarMachine;


public class Font
{
    private unsafe plutovg_font_face_t* Handle = null;
    public string ResourceName;

    public Font(string Name, int FontIndex = 0)
    {
        ResourceName = Resources.Find(Name);
        byte[] FontBytes = Resources.Read(ResourceName);

        IntPtr UnmannagedPtr = Marshal.AllocHGlobal(FontBytes.Length);
        Marshal.Copy(FontBytes, FontIndex, UnmannagedPtr, FontBytes.Length);

        unsafe
        {
            void* Fnord = UnmannagedPtr.ToPointer();
            Handle = plutovg_font_face_load_from_data(Fnord, (uint)FontBytes.Length, 0, DestroyCallback, Fnord);
        }
    }

    ~Font()
    {
        unsafe
        {
            if (Handle != null)
            {
                plutovg_font_face_destroy(Handle);
            }
        }
    }

    private static void DestroyCallback(IntPtr UnmannagedPtr)
    {
        Marshal.FreeHGlobal(UnmannagedPtr);
    }
}
