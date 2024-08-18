
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Buffer;
using Vector2 = System.Numerics.Vector2;
using Matrix3x2 = System.Numerics.Matrix3x2;

using SDL3;
using static SDL3.SDL;

using PlutoVG;
using static PlutoVG.PlutoVG;

using PlutoSVG;
using static PlutoSVG.PlutoSVG;

using Moloch;
using static Moloch.BlobHelper;

namespace StarMachine;


public readonly record struct GridParameters
{
    // The grid is set up such that 1 grid unit is 5% of the vertical resolution, which is the recommended
    // line height for text to ensure excellent readability for players sitting at a reasonable distance away from the screen.
    public const float Grid = 20.0f;
    public const float GridDivisor = Grid / 2.0f;

    // The current conversion from grid space to NDC space.
    public readonly float DivisorX;
    public readonly float DivisorY;

    // Pixels per grid unit.
    public readonly float PixelDensity;

    public GridParameters()
    {
        DivisorX = 0.0f;
        DivisorY = 0.0f;
        PixelDensity = -1.0f;
    }

    public GridParameters(float ScreenWidth, float ScreenHeight)
    {
        float Aspect = (ScreenWidth / ScreenHeight);
        DivisorX = GridDivisor * Aspect;
        DivisorY = GridDivisor;
        PixelDensity = ScreenHeight / Grid;
    }
}


public class WidgetAnchor
{
    // Anchors for translating coordinate systems.
    // The coordinates are relative to the center of the parent widget.
    public Matrix3x2 LocalTransform = Matrix3x2.Identity;
    public List<BaseWidget> Attachments = new();

    public void ResetTransform(float OffsetX = 0.0f, float OffsetY = 0.0f)
    {
        LocalTransform = Matrix3x2.CreateTranslation(OffsetX, OffsetY);
    }

    public void Reflow(Matrix3x2 ParentTransform, GridParameters NewGrid)
    {
        Matrix3x2 RootTransform = LocalTransform * ParentTransform;
        foreach (var Widget in Attachments)
        {
            Widget.Reflow(RootTransform, NewGrid);
        }
    }

    public void PropagateRelease()
    {
        foreach (var Widget in Attachments)
        {
            Widget.PropagateRelease();
        }
    }
}


public abstract class BaseWidget : IComparable<BaseWidget>
{
    // Size in grid units.
    public float GridWidth = 1.0f;
    public float GridHeight = 1.0f;

    // Widget placement relative to its anchor.  This describes a point within the bounding box from -1.0 to 1.0.
    public float AlignX = 0.0f;
    public float AlignY = 0.0f;

    // Set false to skip rendering and updates for this widget and all of it's attachments.
    public bool Visible = true;

    // Widgets are drawn depth-first.  This hint is used to determine the drawing order of widgets in the same generation.
    public int OrderHint = 0;

    // Indicates that data changed and may need to be reuploaded to the GPU.
    protected bool RootTransformChanged = true;
    protected bool GridParametersChanged = true;

    // Current Grid Parameters
    protected GridParameters Grid = new();

    public virtual void Advance(FrameInfo Frame)
    {
        RootTransformChanged = false;
        GridParametersChanged = false;
    }

    public virtual void Draw(IntPtr RenderPass)
    {
    }

    public virtual void Release()
    {
    }

    public void ResetTransform()
    {
        LocalTransform = Matrix3x2.Identity;
        RootTransform = LocalTransform * ParentTransform;
        RootTransformChanged = true;
        ReflowAttachments();
    }

    public void Move(float X, float Y)
    {
        LocalTransform = Matrix3x2.CreateTranslation(X, Y) * LocalTransform;
        RootTransform = LocalTransform * ParentTransform;
        RootTransformChanged = true;
        ReflowAttachments();
    }

    public void Rotate(float Degrees)
    {
        LocalTransform = Matrix3x2.CreateRotation(Single.DegreesToRadians(Degrees)) * LocalTransform;
        RootTransform = LocalTransform * ParentTransform;
        RootTransformChanged = true;
        ReflowAttachments();
    }

    // For IComparable
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

    // Magic list that contains all of the WidgetAnchor fields on this widget.
    private List<WidgetAnchor>? _Anchors = null;
    public List<WidgetAnchor> Anchors
    {
        get
        {
            if (_Anchors == null)
            {
                _Anchors = GatherAnchors();
            }
            return _Anchors;
        }
    }

    //
    // Local transform for the widget.
    protected Matrix3x2 LocalTransform = Matrix3x2.Identity;
    protected Matrix3x2 ParentTransform = Matrix3x2.Identity;
    protected Matrix3x2 RootTransform = Matrix3x2.Identity;

    //
    public void Reflow(Matrix3x2 InParentTransform, GridParameters NewGrid)
    {
        if (NewGrid != Grid)
        {
            GridParametersChanged = true;
            Grid = NewGrid;
        }

        ParentTransform = InParentTransform;
        RootTransform = LocalTransform * ParentTransform;
        foreach (var Anchor in Anchors)
        {
            Anchor.Reflow(RootTransform, NewGrid);
        }
    }

    //
    public void ReflowAttachments()
    {
        foreach (var Anchor in Anchors)
        {
            Anchor.Reflow(RootTransform, Grid);
        }
    }

    public void PropagateRelease()
    {
        Release();
        foreach (var Anchor in Anchors)
        {
            Anchor.PropagateRelease();
        }
    }

    // TODO: GetType().GetFields() isn't workable in GatherAnchors due to the trimming required by NativeAOT.
    // What does work is typeof(...).GetFields(), which is what this hack is for.  However, maybe we can do
    // something clever with generics instead?
    public abstract FieldInfo[] GetFields();

    private List<WidgetAnchor> GatherAnchors()
    {
        var Anchors = new List<WidgetAnchor>();
        foreach (var Field in GetFields())
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

    // The last known screen dimensions are cached to help determine if we need to update the widget graph.
    private float CachedWidth = -1.0f;
    private float CachedHeight = -1.0f;

    // The list of all widgets attached to the screen.
    private List<(BaseWidget, int)> DrawOrder = new();

    public override FieldInfo[] GetFields()
    {
        return typeof(RootWidget).GetFields();
    }

    public void Rebuild()
    {
        DrawOrder = GatherWidgets();
        CachedWidth = -1.0f;
        CachedHeight = -1.0f;
    }

    public override void Advance(FrameInfo Frame)
    {
        float ScreenWidth = (float)Frame.Width;
        float ScreenHeight = (float)Frame.Height;
        if (CachedWidth != ScreenWidth || CachedHeight != ScreenHeight)
        {
            CachedWidth = ScreenWidth;
            CachedHeight = ScreenHeight;

            Grid = new GridParameters(ScreenWidth, ScreenHeight);

            TopLeft.ResetTransform(-Grid.DivisorX, Grid.DivisorY);
            TopCenter.ResetTransform(0.0f, Grid.DivisorY);
            TopRight.ResetTransform(Grid.DivisorX, Grid.DivisorY);

            CenterLeft.ResetTransform(-Grid.DivisorX, 0.0f);
            Center.ResetTransform(0.0f, 0.0f);
            CenterRight.ResetTransform(Grid.DivisorX, 0.0f);

            BottomLeft.ResetTransform(-Grid.DivisorX, -Grid.DivisorY);
            BottomCenter.ResetTransform(0.0f, -Grid.DivisorY);
            BottomRight.ResetTransform(Grid.DivisorX, -Grid.DivisorY);

            ReflowAttachments();
        }

        for (int Index = 0; Index < DrawOrder.Count; ++Index)
        {
            (BaseWidget Widget, int SkipCount) = DrawOrder[Index];
            if (Widget.Visible)
            {
                Widget.Advance(Frame);
            }
            else
            {
                Index += SkipCount;
            }
        }
    }

    public override void Draw(IntPtr RenderPass)
    {
        for (int Index = 0; Index < DrawOrder.Count; ++Index)
        {
            (BaseWidget Widget, int SkipCount) = DrawOrder[Index];
            if (Widget.Visible)
            {
                Widget.Draw(RenderPass);
            }
            else
            {
                Index += SkipCount;
            }
        }
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

    public (int W, int H, byte[] Data) Render(string Text, float TexelHeight)
    {
        byte[] TextBytes = Encoding.UTF8.GetBytes(Text);

        float FontSize = TexelHeight;

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
    public float DocumentWidth = 0.0f;
    public float DocumentHeight = 0.0f;

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
            DocumentWidth = plutosvg_document_get_width(Handle);
            DocumentHeight = plutosvg_document_get_height(Handle);
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


public abstract class TextureWidget : BaseWidget
{
    protected IntPtr Device = IntPtr.Zero;
    private IntPtr Texture = IntPtr.Zero;
    private IntPtr Sampler = IntPtr.Zero;
    private IntPtr VertexBuffer = IntPtr.Zero;

    protected int PixelWidth = 0;
    protected int PixelHeight = 0;

    public override FieldInfo[] GetFields()
    {
        return typeof(SvgWidget).GetFields();
    }

    public override void Advance(FrameInfo Frame)
    {
        if (GridParametersChanged)
        {
            PixelWidth = (int)(GridWidth * Grid.PixelDensity);
            PixelHeight = (int)(GridHeight * Grid.PixelDensity);
            GridParametersChanged = false;
            UploadTexture();
        }
        if (RootTransformChanged)
        {
            RootTransformChanged = false;
            UploadVertexBuffer();
        }
    }

    public override void Draw(IntPtr RenderPass)
    {
        Trace.Assert(Texture != IntPtr.Zero);
        Trace.Assert(Sampler != IntPtr.Zero);
        Trace.Assert(VertexBuffer != IntPtr.Zero);
        
        SDL_GpuBufferBinding VertexBufferBindings;
        {
            VertexBufferBindings.buffer = VertexBuffer;
            VertexBufferBindings.offset = 0;
        }

        SDL_GpuTextureSamplerBinding SamplerBindings;
        {
            SamplerBindings.texture = Texture;
            SamplerBindings.sampler = Sampler;
        }

        unsafe
        {
            SDL_GpuBindVertexBuffers(RenderPass, 0, &VertexBufferBindings, 1);
            SDL_GpuBindFragmentSamplers(RenderPass, 0, &SamplerBindings, 1);
        }

        SDL_GpuDrawPrimitives(RenderPass, 0, 4);
    }

    public abstract (int TexelWidth, int TexelHeight, byte[] SurfaceData) Rasterize();

    public void UploadTexture()
    {
        ReleaseTexture();

        if (Sampler == IntPtr.Zero)
        {
            SDL_GpuSamplerCreateInfo CreateInfo;
            {
                CreateInfo.minFilter = SDL.SDL_GpuFilter.SDL_GPU_FILTER_LINEAR;
                CreateInfo.magFilter = SDL.SDL_GpuFilter.SDL_GPU_FILTER_LINEAR;
                CreateInfo.mipmapMode = SDL.SDL_GpuSamplerMipmapMode.SDL_GPU_SAMPLERMIPMAPMODE_NEAREST;
                CreateInfo.addressModeU = SDL.SDL_GpuSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE;
                CreateInfo.addressModeV = SDL.SDL_GpuSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE;
                CreateInfo.addressModeW = SDL.SDL_GpuSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE;
                CreateInfo.mipLodBias = 0.0f;
                CreateInfo.anisotropyEnable = 0;
                CreateInfo.maxAnisotropy = 0.0f;
                CreateInfo.compareEnable = 0;
                CreateInfo.compareOp = SDL.SDL_GpuCompareOp.SDL_GPU_COMPAREOP_NEVER;
                CreateInfo.minLod = 0.0f;
                CreateInfo.maxLod = 0.0f;
            }
            unsafe
            {
                Sampler = SDL_GpuCreateSampler(Device, &CreateInfo);
            }
        }

        (int SurfaceWidth, int SurfaceHeight, byte[] SurfaceData) = Rasterize();

        SDL_GpuTextureCreateInfo Desc;
        {
            Desc.width = (uint)SurfaceWidth;
            Desc.height = (uint)SurfaceHeight;
            Desc.depth = 1;
            Desc.isCube = 0;
            Desc.layerCount = 1;
            Desc.levelCount = 1;
            Desc.sampleCount = SDL.SDL_GpuSampleCount.SDL_GPU_SAMPLECOUNT_1;
            Desc.format = SDL.SDL_GpuTextureFormat.SDL_GPU_TEXTUREFORMAT_B8G8R8A8;
            Desc.usageFlags = (uint)SDL.SDL_GpuTextureUsageFlagBits.SDL_GPU_TEXTUREUSAGE_SAMPLER_BIT;
        }

        unsafe
        {
            int UploadSize = SurfaceData.Length;
            Texture = SDL_GpuCreateTexture(Device, &Desc);
            SDL_GpuSetTextureName(Device, Texture, "SVG Widget"u8);

            IntPtr TransferBuffer = SDL_GpuCreateTransferBuffer(
                Device, SDL.SDL_GpuTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD, (uint)UploadSize);

            byte* MappedMemory;
            SDL_GpuMapTransferBuffer(Device, TransferBuffer, 0, (void**)&MappedMemory);
            fixed (byte* DataPtr = SurfaceData)
            {
                MemoryCopy(DataPtr, MappedMemory, UploadSize, UploadSize);
            }
            SDL_GpuUnmapTransferBuffer(Device, TransferBuffer);

            IntPtr CommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
            IntPtr CopyPass = SDL_GpuBeginCopyPass(CommandBuffer);

            SDL_GpuTextureTransferInfo TransferInfo;
            {
                TransferInfo.transferBuffer = TransferBuffer;
                TransferInfo.offset = 0;
                TransferInfo.imagePitch = Desc.width;
                TransferInfo.imageHeight = Desc.height;
            }
            SDL_GpuTextureRegion Region;
            {
                Region.textureSlice.texture = Texture;
                Region.textureSlice.mipLevel = 0;
                Region.textureSlice.layer = 0;
                Region.x = 0;
                Region.y = 0;
                Region.z = 0;
                Region.w = Desc.width;
                Region.h = Desc.height;
                Region.d = 1;
            }
            SDL_GpuUploadToTexture(CopyPass, &TransferInfo, &Region, 0);
            SDL_GpuEndCopyPass(CopyPass);
            SDL_GpuSubmit(CommandBuffer);
            SDL_GpuReleaseTransferBuffer(Device, TransferBuffer);
        }
    }

    public void UploadVertexBuffer()
    {
        ReleaseVertexBuffer();

        Vector2 GridOrigin = Vector2.Transform(Vector2.Zero, RootTransform);
        Vector2 GridDivisor = new Vector2(1.0f / Grid.DivisorX, 1.0f / Grid.DivisorY);

        float LocalOriginX = Single.Lerp(0.0f, GridWidth, AlignX);
        float LocalOriginY = Single.Lerp(0.0f, GridHeight, AlignY);

        float GridX1 = (-GridWidth - LocalOriginX) * 0.5f;
        float GridX2 = (+GridWidth - LocalOriginX) * 0.5f;
        float GridY1 = (+GridHeight - LocalOriginY) * 0.5f;
        float GridY2 = (-GridHeight - LocalOriginY) * 0.5f;

        Vector2 ScreenNW = Vector2.Transform(new Vector2(GridX1, GridY1), RootTransform) * GridDivisor;
        Vector2 ScreenNE = Vector2.Transform(new Vector2(GridX2, GridY1), RootTransform) * GridDivisor;
        Vector2 ScreenSW = Vector2.Transform(new Vector2(GridX1, GridY2), RootTransform) * GridDivisor;
        Vector2 ScreenSE = Vector2.Transform(new Vector2(GridX2, GridY2), RootTransform) * GridDivisor;

        Span<float> Data = stackalloc float[16];

        Data[0x0] = 0.0f; Data[0x1] = 0.0f;
        Data[0x4] = 0.0f; Data[0x5] = 1.0f;
        Data[0x8] = 1.0f; Data[0x9] = 0.0f;
        Data[0xC] = 1.0f; Data[0xD] = 1.0f;

        Data[0x2] = ScreenNW.X; Data[0x3] = ScreenNW.Y;
        Data[0x6] = ScreenSW.X; Data[0x7] = ScreenSW.Y;
        Data[0xA] = ScreenNE.X; Data[0xB] = ScreenNE.Y;
        Data[0xE] = ScreenSE.X; Data[0xF] = ScreenSE.Y;

        uint UploadSize = sizeof(float) * (uint)Data.Length;

        VertexBuffer = SDL_GpuCreateBuffer(
            Device,
            (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
            UploadSize);
        SDL_GpuSetBufferName(Device, VertexBuffer, "Fnord"u8);

        IntPtr TransferBuffer = SDL_GpuCreateTransferBuffer(
            Device, SDL_GpuTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD, UploadSize);

        unsafe
        {
            byte* MappedMemory;
            SDL_GpuMapTransferBuffer(Device, TransferBuffer, 0, (void**)&MappedMemory);
            fixed (void* UploadData = Data)
            {
                MemoryCopy(UploadData, MappedMemory, UploadSize, UploadSize);
            }
            SDL_GpuUnmapTransferBuffer(Device, TransferBuffer);
        }

        IntPtr CommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
        IntPtr CopyPass = SDL_GpuBeginCopyPass(CommandBuffer);
        unsafe
        {
            SDL_GpuTransferBufferLocation Source;
            {
                Source.transferBuffer = TransferBuffer;
                Source.offset = 0;
            }
            SDL_GpuBufferRegion Dest;
            {
                Dest.buffer = VertexBuffer;
                Dest.offset = 0;
                Dest.size = UploadSize;
            }
            SDL_GpuUploadToBuffer(CopyPass, &Source, &Dest, 0);
        }
        SDL_GpuEndCopyPass(CopyPass);
        SDL_GpuSubmit(CommandBuffer);
        SDL_GpuReleaseTransferBuffer(Device, TransferBuffer);
    }

    public void ReleaseTexture()
    {
        if (Texture != IntPtr.Zero)
        {
            SDL_GpuReleaseTexture(Device, Texture);
            Texture = IntPtr.Zero;
        }
    }

    public void ReleaseSampler()
    {
        if (Sampler != IntPtr.Zero)
        {
            SDL_GpuReleaseSampler(Device, Sampler);
            Sampler = IntPtr.Zero;
        }
    }

    public void ReleaseVertexBuffer()
    {
        if (VertexBuffer != IntPtr.Zero)
        {
            SDL_GpuReleaseBuffer(Device, VertexBuffer);
            VertexBuffer = IntPtr.Zero;
        }
    }

    public override void Release()
    {
        ReleaseTexture();
        ReleaseSampler();
        ReleaseVertexBuffer();
    }

    ~TextureWidget()
    {
        Trace.Assert(Texture == IntPtr.Zero);
        Trace.Assert(Sampler == IntPtr.Zero);
        Trace.Assert(VertexBuffer == IntPtr.Zero);
    }
}


public class SvgWidget : TextureWidget
{
    public SVGResource Resource;

    public SvgWidget(IntPtr InDevice, string ResourceName, float InGridWidth = -1.0f, float InGridHeight=-1.0f)
    {
        Device = InDevice;
        Resource = new SVGResource(ResourceName);

        GridWidth = InGridWidth;
        GridHeight = InGridHeight;

        if (GridHeight < 0.0f && GridWidth < 0.0f)
        {
            GridHeight = 1.0f;
        }

        if (GridWidth < 0.0f)
        {
            GridWidth = (Resource.DocumentWidth / Resource.DocumentHeight) * GridHeight;
        }

        if (GridHeight < 0.0f)
        {
            GridHeight = (Resource.DocumentHeight / Resource.DocumentWidth) * GridWidth;
        }
    }

    public override (int TexelWidth, int TexelHeight, byte[] SurfaceData) Rasterize()
    {
        return Resource.Render(PixelWidth, PixelHeight);
    }
}


public class TextWidget : TextureWidget
{
    private FontResource Resource;
    private float FontSize;
    private string Text;

    public TextWidget(IntPtr InDevice, string InText, float Size, string FontName, int FontIndex = 0)
    {
        Device = InDevice;
        FontSize = Size;
        Text = InText;
        Resource = new FontResource(FontName, FontIndex);
    }

    public void SetText(string NewText)
    {
        if (NewText != Text)
        {
            Text = NewText;
            GridParametersChanged = true;
            RootTransformChanged = true;
        }
    }

    public override (int TexelWidth, int TexelHeight, byte[] SurfaceData) Rasterize()
    {
        byte[] Blob;
        (PixelWidth, PixelHeight, Blob) = Resource.Render(Text, FontSize * Grid.PixelDensity);

        GridWidth = (float)PixelWidth / Grid.PixelDensity;
        GridHeight = (float)PixelHeight / Grid.PixelDensity;

        return (PixelWidth, PixelHeight, Blob);
    }
}
