
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using static System.Buffer;
using Matrix3x2 = System.Numerics.Matrix3x2;

using SDL3;
using static SDL3.SDL;

using PlutoVG;
using static PlutoVG.PlutoVG;

using PlutoSVG;
using static PlutoSVG.PlutoSVG;

using Moloch;
using static Moloch.BlobHelper;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using static StarMachine.ImageOverlay;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace StarMachine;


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

    public void Reflow(Matrix3x2 ParentTransform, float PixelDensity)
    {
        Matrix3x2 RootTransform = LocalTransform * ParentTransform;
        foreach (var Widget in Attachments)
        {
            Widget.Reflow(RootTransform, PixelDensity);
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
    // Local transform for the widget.
    public Matrix3x2 LocalTransform = Matrix3x2.Identity;

    // Set false to skip rendering and updates for this widget and all of it's attachments.
    public bool Visible = true;

    // Widgets are drawn depth-first.  This hint is used to determine the drawing order of widgets in the same generation.
    public int OrderHint = 0;

    // Indicates that the root transform updated.
    public bool RootTransformChanged = true;
    public bool PixelDensityChanged = true;

    public virtual void Advance(FrameInfo Frame)
    {
        RootTransformChanged = false;
        PixelDensityChanged = false;
    }

    public virtual void Draw(IntPtr RenderPass)
    {
    }

    public virtual void Release()
    {
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

    // Pixels per grid unit.
    public float PixelDensity = 0.0f;

    //
    public Matrix3x2 RootTransform = Matrix3x2.Identity;

    //
    public void Reflow(Matrix3x2 ParentTransform, float NewPixelDensity = -1.0f)
    {
        if (NewPixelDensity > 0.0f && NewPixelDensity != PixelDensity)
        {
            PixelDensityChanged = true;
            PixelDensity = NewPixelDensity;
        }

        RootTransform = LocalTransform * ParentTransform;
        foreach (var Anchor in Anchors)
        {
            Anchor.Reflow(RootTransform, NewPixelDensity);
        }
    }

    //
    public void ReflowAttachments()
    {
        foreach (var Anchor in Anchors)
        {
            Anchor.Reflow(RootTransform, PixelDensity);
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

            float Aspect = (ScreenWidth / ScreenHeight);
            DivisorX = GridDivisor * Aspect;
            DivisorY = GridDivisor;

            PixelDensity = ScreenHeight / Grid;

            TopLeft.ResetTransform(-DivisorX, DivisorY);
            TopCenter.ResetTransform(0.0f, DivisorY);
            TopRight.ResetTransform(DivisorX, DivisorY);

            CenterLeft.ResetTransform(-DivisorX, 0.0f);
            Center.ResetTransform(0.0f, 0.0f);
            CenterRight.ResetTransform(DivisorX, 0.0f);

            BottomLeft.ResetTransform(-DivisorX, -DivisorY);
            BottomCenter.ResetTransform(0.0f, -DivisorY);
            BottomRight.ResetTransform(DivisorX, -DivisorY);

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


public class SvgWidget : BaseWidget
{
    private IntPtr Device = IntPtr.Zero;
    public IntPtr Texture = IntPtr.Zero;
    public IntPtr Sampler = IntPtr.Zero;
    public IntPtr VertexBuffer = IntPtr.Zero;
    public SVGResource Resource;

    public float GridWidth = 1.0f;
    public float GridHeight = 1.0f;

    public int PixelWidth = 0;
    public int PixelHeight = 0;

    public override FieldInfo[] GetFields()
    {
        return typeof(SvgWidget).GetFields();
    }

    public SvgWidget(IntPtr InDevice, string ResourceName)
    {
        Device = InDevice;
        Resource = new SVGResource(ResourceName);
    }

    public override void Advance(FrameInfo Frame)
    {
        if (PixelDensityChanged)
        {
            Console.WriteLine($"{GridWidth}, {GridHeight}, {PixelDensity}");
            PixelWidth = (int)(GridWidth * PixelDensity);
            PixelHeight = (int)(GridHeight * PixelDensity);
            PixelDensityChanged = false;
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

        (int SurfaceWidth, int SurfaceHeight, byte[] SurfaceData) = Resource.Render(PixelWidth, PixelHeight);

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

        float ScreenMinX = -1.0f;
        float ScreenMaxX = +1.0f;
        float ScreenMinY = -1.0f;
        float ScreenMaxY = +1.0f;

        Span<float> Data = stackalloc float[16];

        Data[0x0] = 0.0f; Data[0x1] = 0.0f;
        Data[0x4] = 0.0f; Data[0x5] = 1.0f;
        Data[0x8] = 1.0f; Data[0x9] = 0.0f;
        Data[0xC] = 1.0f; Data[0xD] = 1.0f;

        Data[0x2] = ScreenMinX; Data[0x3] = ScreenMaxY;
        Data[0x6] = ScreenMinX; Data[0x7] = ScreenMinY;
        Data[0xA] = ScreenMaxX; Data[0xB] = ScreenMaxY;
        Data[0xE] = ScreenMaxX; Data[0xF] = ScreenMinY;

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

    ~SvgWidget()
    {
        Trace.Assert(Texture == IntPtr.Zero);
        Trace.Assert(Sampler == IntPtr.Zero);
        Trace.Assert(VertexBuffer == IntPtr.Zero);
    }
}