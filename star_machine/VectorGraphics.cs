
using System.Text;
using System.Reflection;
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


public class WidgetAnchor
{
    // Anchors for translating coordinate systems.
    // The coordinates are relative to the center of the parent widget.
    public float X = 0.0f;
    public float Y = 0.0f;
    public List<BaseWidget> Attachments = new();

    public void SetXY(float InX, float InY)
    {
        X = InX;
        Y = InY;
    }
}


public class BaseWidget : IComparable<BaseWidget>
{
    // Set false to skip rendering and updates for this widget and all of it's attachments.
    public bool Visible = true;

    // Widgets are drawn depth-first.  This hint is used to determine the drawing order of widgets in the same generation.
    public int OrderHint = 0;

    public int CompareTo(BaseWidget? Other)
    {
        if (Other == null)
        {
            return -1;
        }
        else
        {
            return OrderHint.CompareTo(Other.OrderHint);
        }
    }

    // Returns a list containing this widget's anchors.
    public List<WidgetAnchor> GatherAnchors()
    {
        var Anchors = new List<WidgetAnchor>();
        foreach (var Field in GetType().GetFields())
        {
            if (Field.FieldType == typeof(WidgetAnchor))
            {
                object? Value = Field.GetValue(this);
                if (Value != null)
                {
                    var Anchor = (WidgetAnchor)Value;
                    Anchors.Add(Anchor);
                }
            }
        }

        return Anchors;
    }

    // Recursively build the list of all widgets sorted in drawing order.
    public List<(BaseWidget, int)> GatherWidgets()
    {
        List<WidgetAnchor> Anchors = GatherAnchors();

        var Attachments = new List<BaseWidget>();
        foreach (var Anchor in Anchors)
        {
            Attachments.AddRange(Anchor.Attachments);
        }
        Attachments.Sort();

        var Gathered = new List<(BaseWidget, int)>();
        foreach (var Widget in Attachments)
        {
            List<(BaseWidget, int)> Descents = Widget.GatherWidgets();
            Gathered.Add((Widget, Descents.Count));
            Gathered.AddRange(Descents);
        }

        return Gathered;
    }
}


public class RootWidget : BaseWidget
{
    // The root widget represents the screen.

    public WidgetAnchor TopLeft = new();
    public WidgetAnchor TopCenter = new();
    public WidgetAnchor TopRight = new();
    public WidgetAnchor CenterLeft = new();
    public WidgetAnchor Center = new();
    public WidgetAnchor CenterRight = new();
    public WidgetAnchor BottomLeft = new();
    public WidgetAnchor BottomCenter = new();
    public WidgetAnchor BottomRight = new();

    // The grid is set up such that 1 grid unit is 5% of the vertical resolution, which is the recommended
    // line height for text to ensure excellent readability for players sitting at a reasonable distance away from the screen.
    private const float Grid = 20.0f;
    private const float GridDivisor = Grid / 2.0f;

    // The last known screen dimensions are cached to help determine if we need to update the widget graph.
    private float CachedWidth = -1.0f;
    private float CachedHeight = -1.0f;

    // The current conversion from grid space to NDC space.
    private float DivisorX = 0.0f;
    private float DivisorY = 0.0f;

    // The list of all widgets attached to the screen.
    private List<(BaseWidget, int)> DrawOrder = new();

    public void Refresh(float ScreenWidth, float ScreenHeight)
    {
        if (CachedWidth != ScreenWidth || CachedHeight != ScreenHeight)
        {
            CachedWidth = ScreenWidth;
            CachedHeight = ScreenHeight;

            float Aspect = (ScreenWidth / ScreenHeight);
            DivisorX = GridDivisor * Aspect;
            DivisorY = GridDivisor;

            TopLeft.SetXY(-DivisorX, DivisorY);
            TopCenter.SetXY(0.0f, DivisorY);
            TopRight.SetXY(DivisorX, DivisorY);

            CenterLeft.SetXY(-DivisorX, 0.0f);
            Center.SetXY(0.0f, 0.0f);
            CenterRight.SetXY(DivisorX, 0.0f);

            BottomLeft.SetXY(-DivisorX, -DivisorY);
            BottomCenter.SetXY(0.0f, -DivisorY);
            BottomRight.SetXY(DivisorX, -DivisorY);
        }
    }

    public void Rebuild()
    {
        DrawOrder = GatherWidgets();
    }
}


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

    public (int W, int H, byte[] Data) Render(string Text, float Scale, float ScreenHeight)
    {
        byte[] TextBytes = Encoding.UTF8.GetBytes(Text);

        float FontSize = ScreenHeight * Scale; // Size in pixels.
        unsafe
        {
            plutovg_rect_t BoundingBox;
            plutovg_surface_t* Surface;
            plutovg_canvas_t* Canvas;
            plutovg_text_encoding_t Encoding = plutovg_text_encoding_t.PLUTOVG_TEXT_ENCODING_UTF8;

            fixed (byte* TextPtr = TextBytes)
            {
                float Ascent;
                float Descent;
                float LineGap;
                plutovg_font_face_get_metrics(Handle, FontSize, &Ascent, &Descent, &LineGap, &BoundingBox);

                float Advance = plutovg_font_face_text_extents(Handle, FontSize, TextPtr, TextBytes.Length, Encoding, &BoundingBox);
                Surface = plutovg_surface_create((int)Single.Floor(BoundingBox.w), (int)Single.Ceiling(FontSize));
                Canvas = plutovg_canvas_create(Surface);
                plutovg_canvas_set_rgba(Canvas, 1.0f, 1.0f, 1.0f, 1.0f);
                plutovg_canvas_set_font(Canvas, Handle, FontSize);
                plutovg_canvas_fill_text(Canvas, TextPtr, TextBytes.Length, Encoding, Single.Floor(-BoundingBox.x), Ascent + Descent);
            }

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

            plutovg_canvas_destroy(Canvas);
            plutovg_surface_destroy(Surface);

            return (SurfaceWidth, SurfaceHeight, Blob);
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
