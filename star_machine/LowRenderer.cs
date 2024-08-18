
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Matrix4x4 = System.Numerics.Matrix4x4;
using static System.Buffer;

using SDL3;
using static SDL3.SDL;

using PlutoVG;
using static PlutoVG.PlutoVG;

using PlutoSVG;
using static PlutoSVG.PlutoSVG;

using FixedInt = FixedPoint.FixedInt;
using Fixie = FixedPoint.Fixie;

namespace StarMachine;


[System.Runtime.CompilerServices.InlineArray(3)]
public struct ivec3
{
    public Int32 _element0;
}

[System.Runtime.CompilerServices.InlineArray(3)]
public struct uvec3
{
    public UInt32 _element0;
}

[System.Runtime.CompilerServices.InlineArray(3)]
public struct vec3
{
    public float _element0;
}

[System.Runtime.CompilerServices.InlineArray(4)]
public struct vec4
{
    public float _element0;
}

[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
struct ViewInfoUpload
{
    [System.Runtime.InteropServices.FieldOffset(0)]
    public Matrix4x4 WorldToView;

    [System.Runtime.InteropServices.FieldOffset(64)]
    public Matrix4x4 ViewToClip;

    [System.Runtime.InteropServices.FieldOffset(128)]
    public vec4 MovementProjection;

    [System.Runtime.InteropServices.FieldOffset(144)]
    public uvec3 EyeWorldPosition_L;

    [System.Runtime.InteropServices.FieldOffset(160)]
    public uvec3 EyeWorldPosition_H;

    [System.Runtime.InteropServices.FieldOffset(172)]
    public float SplatDiameter;

    [System.Runtime.InteropServices.FieldOffset(176)]
    public float SplatDepth;

    [System.Runtime.InteropServices.FieldOffset(180)]
    public float AspectRatio;
}


class ImageOverlay
{
    private IntPtr Device = IntPtr.Zero;
    public IntPtr Texture = IntPtr.Zero;
    public IntPtr Sampler = IntPtr.Zero;
    public IntPtr VertexBuffer = IntPtr.Zero;

    public enum AlignModeH
    {
        Left,
        Right,
        Center,
    }

    public enum AlignModeV
    {
        Top,
        Bottom,
        Center,
    }

    public enum ScaleMode
    {
        Aspect, // Ignore the scale value and maintain aspect ratio.
        Screen, // Scale is the portion of the screen to cover.
    }

    private float TextureWidth = 0.0f;
    private float TextureHeight = 0.0f;

    public AlignModeH AlignModeX = AlignModeH.Center;
    public float AlignX = 0.0f;

    public AlignModeV AlignModeY = AlignModeV.Center;
    public float AlignY = 0.0f;

    public ScaleMode ScaleModeX = ScaleMode.Screen;
    public float ScaleX = 1.0f;

    public ScaleMode ScaleModeY = ScaleMode.Aspect;
    public float ScaleY = 1.0f;

    public ImageOverlay()
    {
    }

    public ImageOverlay(IntPtr InDevice, (int W, int H, byte[] Data) Image)
    {
        UploadTexture(InDevice, Image);
    }

    public ImageOverlay(IntPtr InDevice, (int W, int H, byte[] Data) Image, float ScreenWidth, float ScreenHeight)
    {
        UploadTexture(InDevice, Image);
        UploadVertices(ScreenWidth, ScreenHeight);
    }

    public void UploadTexture(IntPtr InDevice, (int W, int H, byte[] Data) Image)
    {
        ReleaseTexture();
        Device = InDevice;

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

        TextureWidth = (float)Image.W;
        TextureHeight = (float)Image.H;

        SDL_GpuTextureCreateInfo Desc;
        {
            Desc.width = (uint)Image.W;
            Desc.height = (uint)Image.H;
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
            int UploadSize = Image.Data.Length;
            Texture = SDL_GpuCreateTexture(Device, &Desc);
            SDL_GpuSetTextureName(Device, Texture, "SVG Test"u8);

            IntPtr TransferBuffer = SDL_GpuCreateTransferBuffer(
                Device, SDL.SDL_GpuTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD, (uint)UploadSize);

            byte* MappedMemory;
            SDL_GpuMapTransferBuffer(Device, TransferBuffer, 0, (void**)&MappedMemory);
            fixed (byte* DataPtr = Image.Data)
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

    public void UploadVertices(float ScreenWidth, float ScreenHeight)
    {
        Trace.Assert(Device != IntPtr.Zero);
        ReleaseVertexBuffer();

        float ScreenMinX = -1.0f;
        float ScreenMaxX = +1.0f;
        float ScreenMinY = -1.0f;
        float ScreenMaxY = +1.0f;

        {
            float TextureScaleX = 1.0f;
            float TextureScaleY = 1.0f;

            if (ScaleModeX == ScaleMode.Screen)
            {
                TextureScaleX = ScaleX;
                if (ScaleModeY == ScaleMode.Aspect)
                {
                    TextureScaleY = (TextureHeight / TextureWidth) / (ScreenHeight / ScreenWidth) * TextureScaleX;
                }
            }

            if (ScaleModeY == ScaleMode.Screen)
            {
                TextureScaleY = ScaleY;
                if (ScaleModeX == ScaleMode.Aspect)
                {
                    TextureScaleX = (TextureWidth / TextureHeight) / (ScreenWidth / ScreenHeight) * TextureScaleY;
                }
            }

            float SpanX = 2.0f * TextureScaleX;
            float SpanY = 2.0f * TextureScaleY;

            if (AlignModeX == AlignModeH.Left)
            {
                ScreenMinX = Single.Lerp(-1.0f, +1.0f, AlignX);
                ScreenMaxX = ScreenMinX + SpanX;
            }
            else if (AlignModeX == AlignModeH.Right)
            {
                ScreenMaxX = Single.Lerp(+1.0f, -1.0f, AlignX);
                ScreenMinX = ScreenMaxX - SpanX;
            }
            else if (AlignModeX == AlignModeH.Center)
            {
                ScreenMinX = SpanX * -0.5f;
                ScreenMaxX = SpanX * +0.5f;
            }

            if (AlignModeY == AlignModeV.Top)
            {
                ScreenMaxY = Single.Lerp(+1.0f, -1.0f, AlignY);
                ScreenMinY = ScreenMaxY - SpanY;
            }
            else if (AlignModeY == AlignModeV.Bottom)
            {
                ScreenMinY = Single.Lerp(-1.0f, +1.0f, AlignY);
                ScreenMaxY = ScreenMinY + SpanY;
            }
            else if (AlignModeY == AlignModeV.Center)
            {
                ScreenMinY = SpanY * -0.5f;
                ScreenMaxY = SpanY * +0.5f;
            }
        }

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

    public void Release()
    {
        ReleaseTexture();
        ReleaseSampler();
        ReleaseVertexBuffer();
    }

    ~ImageOverlay()
    {
        Trace.Assert(Texture == IntPtr.Zero);
        Trace.Assert(VertexBuffer == IntPtr.Zero);
    }
}


class LowLevelRenderer
{
    public IntPtr Window = IntPtr.Zero;
    public IntPtr Device = IntPtr.Zero;
    private IntPtr SurfelPipeline = IntPtr.Zero;
    private IntPtr RevealPipeline = IntPtr.Zero;
    private IntPtr OverlayPipeline = IntPtr.Zero;
    private IntPtr ColorTexture = IntPtr.Zero;
    private IntPtr DepthTexture = IntPtr.Zero;
    private IntPtr SplatVertexBuffer = IntPtr.Zero;
    private IntPtr SplatIndexBuffer = IntPtr.Zero;
    private IntPtr SplatWorldPositionBuffer_L = IntPtr.Zero;
    private IntPtr SplatWorldPositionBuffer_H = IntPtr.Zero;
    private IntPtr SplatColorBuffer = IntPtr.Zero;

    private FontResource Michroma = new("Michroma-Regular.ttf");
    private ImageOverlay Speedometer;

    public RootWidget? Overlay = null;

    public double MilesPerHour = 0.0; // Current speed
    public double SpeedOfLight = 0.0; // Current speed

    public List<ImageOverlay> Overlays;

    private SplatGenerator SplatMesh;

    public LowLevelRenderer(SplatGenerator InSplatMesh)
    {
        SplatMesh = InSplatMesh;
        Overlays = new List<ImageOverlay>();
        {
            Speedometer = new ImageOverlay();
            Speedometer.ScaleModeX = ImageOverlay.ScaleMode.Aspect;
            Speedometer.ScaleModeY = ImageOverlay.ScaleMode.Screen;
            Speedometer.ScaleY = 0.05f;
            Speedometer.AlignModeX = ImageOverlay.AlignModeH.Center;
            Speedometer.AlignModeY = ImageOverlay.AlignModeV.Bottom;
            Speedometer.AlignY = 0.01f;
            Overlays.Add(Speedometer);
        }
    }

    private IntPtr LoadShader(
        string Path,
        SDL_GpuShaderStage ShaderStage,
        uint SamplerCount,
        uint UniformBufferCount,
        uint StorageBufferCount,
        uint StorageTextureCount)
    {
        byte[] ShaderBlob = Resources.FindAndRead(Path);

        unsafe
        {
            fixed (byte* ShaderBlobPtr = ShaderBlob)
            {
                var EntryPoint = "main"u8;
                fixed (byte* EntryPointStr = EntryPoint)
                {
                    SDL.SDL_GpuShaderCreateInfo Desc;
                    Desc.codeSize = ShaderBlob.Length;
                    Desc.code = ShaderBlobPtr;
                    Desc.entryPointName = (char*)EntryPointStr;
                    Desc.format = SDL_GpuShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV;
                    Desc.stage = ShaderStage;
                    Desc.samplerCount = SamplerCount;
                    Desc.uniformBufferCount = UniformBufferCount;
                    Desc.storageBufferCount = StorageBufferCount;
                    Desc.storageTextureCount = StorageTextureCount;
                    return SDL_GpuCreateShader(Device, &Desc);
                }
            }
        }
    }

    private unsafe void UploadBytes(IntPtr DestinationBuffer, byte* UploadData, uint UploadSize, bool Cycling = false)
    {
        IntPtr TransferBuffer = SDL_GpuCreateTransferBuffer(Device, SDL_GpuTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD, UploadSize);
        int Cycle = Cycling ? 1 : 0;

        byte* MappedMemory = null;
        SDL_GpuMapTransferBuffer(Device, TransferBuffer, Cycle, (void**)&MappedMemory);
        MemoryCopy(UploadData, MappedMemory, UploadSize, UploadSize);
        SDL_GpuUnmapTransferBuffer(Device, TransferBuffer);

        IntPtr UploadCommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
        IntPtr CopyPass = SDL_GpuBeginCopyPass(UploadCommandBuffer);
        {
            SDL_GpuTransferBufferLocation Source;
            {
                Source.transferBuffer = TransferBuffer;
                Source.offset = 0;
            }
            SDL_GpuBufferRegion Dest;
            {
                Dest.buffer = DestinationBuffer;
                Dest.offset = 0;
                Dest.size = UploadSize;
            }
            SDL_GpuUploadToBuffer(CopyPass, &Source, &Dest, Cycle);
        }
        SDL_GpuEndCopyPass(CopyPass);
        SDL_GpuSubmit(UploadCommandBuffer);
        SDL_GpuReleaseTransferBuffer(Device, TransferBuffer);
    }

    private void UploadVector3s(IntPtr DestinationBuffer, Vector3[] UploadData, bool Cycling = false)
    {
        uint WordCount = 3 * (uint)UploadData.Length;
        uint UploadSize = sizeof(float) * WordCount;
        unsafe
        {
            float* ScratchSpace = stackalloc float[(int)WordCount];
            int Cursor = 0;
            foreach (Vector3 Vertex in UploadData)
            {
                ScratchSpace[Cursor++] = Vertex.X;
                ScratchSpace[Cursor++] = Vertex.Y;
                ScratchSpace[Cursor++] = Vertex.Z;
            }

            UploadBytes(DestinationBuffer, (byte*)ScratchSpace, UploadSize, Cycling);
        }
    }

    private void UploadUInt16s(IntPtr DestinationBuffer, UInt16[] UploadData, bool Cycling = false)
    {
        uint UploadSize = sizeof(UInt16) * (uint)UploadData.Length;
        unsafe
        {
            fixed (UInt16* UploadDataPtr = UploadData)
            {
                UploadBytes(DestinationBuffer, (byte*)UploadDataPtr, UploadSize, Cycling);
            }
        }
    }

    private void UploadFixies(IntPtr Buffer_L, IntPtr Buffer_H, Fixie[] UploadData, bool Cycling = false)
    {
        uint UploadSize = sizeof(Int32) * 3 * (uint)UploadData.Length;
        unsafe
        {
#if false
            uvec3* ScratchSpace_L = stackalloc uvec3[UploadData.Length];
            uvec3* ScratchSpace_H = stackalloc uvec3[UploadData.Length];
#else
            uvec3[] ScratchSpace_L = new uvec3[UploadData.Length];
            uvec3[] ScratchSpace_H = new uvec3[UploadData.Length];
#endif
            int Cursor = 0;
            foreach (Fixie Vertex in UploadData)
            {
                for (int Lane = 0; Lane < 3; ++Lane)
                {
                    (ScratchSpace_L[Cursor][Lane], ScratchSpace_H[Cursor][Lane]) = Vertex.Lanes[Lane].Split();
                }
                ++Cursor;
            }
            Trace.Assert(Cursor == UploadData.Length);

#if false
            UploadBytes(Buffer_L, (byte*)ScratchSpace_L, UploadSize, Cycling);
            UploadBytes(Buffer_H, (byte*)ScratchSpace_H, UploadSize, Cycling);
#else
            fixed (uvec3* UploadDataPtr = ScratchSpace_L)
            {
                UploadBytes(Buffer_L, (byte*)UploadDataPtr, UploadSize, Cycling);
            }
            fixed (uvec3* UploadDataPtr = ScratchSpace_H)
            {
                UploadBytes(Buffer_H, (byte*)UploadDataPtr, UploadSize, Cycling);
            }
#endif
        }
    }

    public bool Boot(RenderingConfig Settings)
    {
        {
            Console.WriteLine($"PlutoVG {plutovg_version_string()}");
            Console.WriteLine($"PlutoSVG {plutosvg_version_string()}");
        }

        if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_GAMEPAD) < 0)
        {
            Console.WriteLine("SDL3 initialization failed.");
            return true;
        }

        ulong WindowFlags = SDL.SDL_WINDOW_HIGH_PIXEL_DENSITY;
        if (Settings.Fullscreen)
        {
            WindowFlags |= SDL.SDL_WINDOW_FULLSCREEN;
        }

        Window = SDL_CreateWindow("Star Machine"u8, 2256, 1504, WindowFlags);
        if (Window == IntPtr.Zero)
        {
            Console.WriteLine("SDL3 failed to create a window.");
            return true;
        }
        if (Settings.Fullscreen)
        {
            IntPtr Fnord = SDL.SDL_GetWindowFullscreenMode(Window);
            Console.WriteLine($"Borderless fullscreen mode: {Fnord == 0}");
        }

        {
            const int DebugMode = 1;
            const int PreferLowPower = 0;
            var Drivers = new string[]
            {
                // usubBorrow does not seem to be available in D3D11 :(
                "vulkan",
            };
            foreach (string Driver in Drivers)
            {
                uint GlobalProperties = SDL_GetGlobalProperties();
                SDL_SetStringProperty(GlobalProperties, SDL.SDL_PROP_GPU_CREATEDEVICE_NAME_STRING, Encoding.UTF8.GetBytes(Driver));

                Device = SDL_GpuCreateDevice(DebugMode, PreferLowPower, GlobalProperties);
                if (Device != IntPtr.Zero)
                {
                    break;
                }
            }
        }
        if (Device == IntPtr.Zero)
        {
            SDL_DestroyWindow(Window);
            Console.WriteLine("SDL3 failed to create GPU device.");
            return true;
        }

        if (SDL_GpuClaimWindow(Device, Window, SDL.SDL_GpuSwapchainComposition.SDL_GPU_SWAPCHAINCOMPOSITION_SDR, SDL.SDL_GpuPresentMode.SDL_GPU_PRESENTMODE_VSYNC) == 0)
        {
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
            Console.WriteLine("SDL3 failed to attach Window to GPU device.");
            return true;
        }

        SDL_GpuTextureCreateInfo ColorTextureDesc;

        {
            IntPtr BackBuffer;
            UInt32 Width;
            UInt32 Height;
            {
                IntPtr CommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
                if (CommandBuffer == IntPtr.Zero)
                {
                    SDL_GpuUnclaimWindow(Device, Window);
                    SDL_GpuDestroyDevice(Device);
                    SDL_DestroyWindow(Window);
                    Console.WriteLine("GpuAcquireCommandBuffer failed.");
                    return true;
                }
                (BackBuffer, Width, Height) = SDL_GpuAcquireSwapchainTexture(CommandBuffer, Window);
                SDL_GpuSubmit(CommandBuffer);
            }

            if (BackBuffer != IntPtr.Zero)
            {
                Console.WriteLine($"Backbuffer size: ({Width}, {Height})");
            }
            else
            {
                SDL_GpuUnclaimWindow(Device, Window);
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("SDL_GpuAcquireSwapchainTexture failed.");
                return true;
            }

            {
                ColorTextureDesc.width = Width;
                ColorTextureDesc.height = Height;
                ColorTextureDesc.depth = 1;
                ColorTextureDesc.isCube = 0;
                ColorTextureDesc.layerCount = 1;
                ColorTextureDesc.levelCount = 1;
                ColorTextureDesc.sampleCount = SDL.SDL_GpuSampleCount.SDL_GPU_SAMPLECOUNT_1;
                ColorTextureDesc.format = SDL.SDL_GpuTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8B8A8;
                ColorTextureDesc.usageFlags =
                    (uint)SDL.SDL_GpuTextureUsageFlagBits.SDL_GPU_TEXTUREUSAGE_COLOR_TARGET_BIT |
                    (uint)SDL.SDL_GpuTextureUsageFlagBits.SDL_GPU_TEXTUREUSAGE_GRAPHICS_STORAGE_READ_BIT;
            }

            unsafe
            {
                ColorTexture = SDL_GpuCreateTexture(Device, &ColorTextureDesc);
                SDL_GpuSetTextureName(Device, ColorTexture, "ColorTexture"u8);
            }

            // Initial Color Clear
            {
                IntPtr CommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
                SDL_GpuColorAttachmentInfo ColorAttachmentInfo;
                {
                    ColorAttachmentInfo.textureSlice.texture = ColorTexture;
                    ColorAttachmentInfo.clearColor.r = 0.2f;
                    ColorAttachmentInfo.clearColor.g = 0.2f;
                    ColorAttachmentInfo.clearColor.b = 0.2f;
                    ColorAttachmentInfo.clearColor.a = 1.0f;
                    ColorAttachmentInfo.loadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_CLEAR;
                    ColorAttachmentInfo.storeOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_STORE;
                }

                unsafe
                {
                    IntPtr RenderPass = SDL_GpuBeginRenderPass(CommandBuffer, &ColorAttachmentInfo, 1, null);
                    {
                    }
                    SDL_GpuEndRenderPass(RenderPass);
                }
                SDL_GpuSubmit(CommandBuffer);
            }

            unsafe
            {
                SDL_GpuTextureCreateInfo Desc;
                Desc.width = Width;
                Desc.height = Height;
                Desc.depth = 1;
                Desc.isCube = 0;
                Desc.layerCount = 1;
                Desc.levelCount = 1;
                Desc.sampleCount = SDL.SDL_GpuSampleCount.SDL_GPU_SAMPLECOUNT_1;
                Desc.format = SDL.SDL_GpuTextureFormat.SDL_GPU_TEXTUREFORMAT_D32_SFLOAT;
                Desc.usageFlags = (uint)SDL.SDL_GpuTextureUsageFlagBits.SDL_GPU_TEXTUREUSAGE_DEPTH_STENCIL_TARGET_BIT;

                DepthTexture = SDL_GpuCreateTexture(Device, &Desc);
                SDL_GpuSetTextureName(Device, DepthTexture, "DepthTexture"u8);
            }
        }

        {
            // Note: this can only be queried in fullscreen mode after the backbuffer has been acquired!
            // Or, at least this is true on Linux w/ Vulkan.
            float PixelDensity = SDL_GetWindowPixelDensity(Window);
            float DisplayScale = SDL_GetWindowDisplayScale(Window);
            Console.WriteLine($"Window pixel density: {PixelDensity}");
            Console.WriteLine($"Window display scale: {DisplayScale}");

            (int Width, int Height) WindowSize = SDL_GetWindowSize(Window);
            (int Width, int Height) WindowSizePx = SDL_GetWindowSizeInPixels(Window);
            Console.WriteLine($"Window size: {WindowSize}");
            Console.WriteLine($"Window size, pixels: {WindowSizePx}");
        }

        {
            var VertexShader = LoadShader("draw_splat.vs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_VERTEX, 0, 1, 0, 0);
            if (VertexShader == IntPtr.Zero)
            {
                SDL_GpuUnclaimWindow(Device, Window);
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("Failed to create vertex shader.");
                return true;
            }

            var FragmentShader = LoadShader("draw_splat.fs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT, 0, 0, 0, 0);
            if (VertexShader == IntPtr.Zero)
            {
                SDL_GpuReleaseShader(Device, VertexShader);
                SDL_GpuUnclaimWindow(Device, Window);
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("Failed to create fragment shader.");
                return true;
            }

            unsafe
            {
                var BlendState = new SDL_GpuColorAttachmentBlendState();
                BlendState.blendEnable = 1;
                BlendState.alphaBlendOp = SDL_GpuBlendOp.SDL_GPU_BLENDOP_ADD;
                BlendState.colorBlendOp = SDL_GpuBlendOp.SDL_GPU_BLENDOP_ADD;
                BlendState.colorWriteMask = 0xF;
                BlendState.srcColorBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ONE;
                BlendState.srcAlphaBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ONE;
                BlendState.dstColorBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ZERO;
                BlendState.dstAlphaBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ZERO;

                const int ColorDescCount = 1;
                SDL_GpuColorAttachmentDescription* ColorDesc = stackalloc SDL_GpuColorAttachmentDescription[ColorDescCount];
                {
                    ColorDesc[0].format = ColorTextureDesc.format;
                    ColorDesc[0].blendState = BlendState;
                }

                SDL_GpuGraphicsPipelineAttachmentInfo AttachmentInfo = new();
                {
                    AttachmentInfo.colorAttachmentCount = ColorDescCount;
                    AttachmentInfo.colorAttachmentDescriptions = ColorDesc;
                    AttachmentInfo.hasDepthStencilAttachment = 1;
                    AttachmentInfo.depthStencilFormat = SDL.SDL_GpuTextureFormat.SDL_GPU_TEXTUREFORMAT_D32_SFLOAT;
                }

                const int VertexBindingCount = 4;
                SDL_GpuVertexBinding* VertexBindings = stackalloc SDL_GpuVertexBinding[VertexBindingCount];
                {
                    // "LocalVertexOffset"
                    VertexBindings[0].binding = 0;
                    VertexBindings[0].stride = (uint)sizeof(Vector3);
                    VertexBindings[0].inputRate = SDL_GpuVertexInputRate.SDL_GPU_VERTEXINPUTRATE_VERTEX;
                    VertexBindings[0].stepRate = 0;

                    // "SplatWorldPosition_L"
                    VertexBindings[1].binding = 1;
                    VertexBindings[1].stride = (uint)sizeof(Vector3);
                    VertexBindings[1].inputRate = SDL_GpuVertexInputRate.SDL_GPU_VERTEXINPUTRATE_INSTANCE;
                    VertexBindings[1].stepRate = 1;

                    // "SplatWorldPosition_H"
                    VertexBindings[2].binding = 2;
                    VertexBindings[2].stride = (uint)sizeof(Vector3);
                    VertexBindings[2].inputRate = SDL_GpuVertexInputRate.SDL_GPU_VERTEXINPUTRATE_INSTANCE;
                    VertexBindings[2].stepRate = 1;

                    // "SplatColor"
                    VertexBindings[3].binding = 3;
                    VertexBindings[3].stride = (uint)sizeof(Vector3);
                    VertexBindings[3].inputRate = SDL_GpuVertexInputRate.SDL_GPU_VERTEXINPUTRATE_INSTANCE;
                    VertexBindings[3].stepRate = 1;
                }

                const int VertexAttributeCount = 4;
                SDL_GpuVertexAttribute* VertexAttributes = stackalloc SDL_GpuVertexAttribute[VertexAttributeCount];
                {
                    // "LocalVertexOffset"
                    VertexAttributes[0].location = 0;
                    VertexAttributes[0].binding = 0;
                    VertexAttributes[0].format = SDL_GpuVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_VECTOR3;
                    VertexAttributes[0].offset = 0;

                    // "SplatWorldPosition_L"
                    VertexAttributes[1].location = 1;
                    VertexAttributes[1].binding = 1;
                    VertexAttributes[1].format = SDL_GpuVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_VECTOR3;
                    VertexAttributes[1].offset = 0;

                    // "SplatWorldPosition_H"
                    VertexAttributes[2].location = 2;
                    VertexAttributes[2].binding = 2;
                    VertexAttributes[2].format = SDL_GpuVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_VECTOR3;
                    VertexAttributes[2].offset = 0;

                    // "SplatColor"
                    VertexAttributes[3].location = 3;
                    VertexAttributes[3].binding = 3;
                    VertexAttributes[3].format = SDL_GpuVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_VECTOR3;
                    VertexAttributes[3].offset = 0;
                }

                SDL_GpuVertexInputState VertexInputState = new();
                {
                    VertexInputState.vertexBindingCount = VertexBindingCount;
                    VertexInputState.vertexBindings = VertexBindings;
                    VertexInputState.vertexAttributeCount = VertexAttributeCount;
                    VertexInputState.vertexAttributes = VertexAttributes;
                }

                SDL_GpuDepthStencilState DepthStencilState = new();
                {
                    DepthStencilState.depthTestEnable = 1;
                    DepthStencilState.depthWriteEnable = 1;
                    DepthStencilState.compareOp = SDL.SDL_GpuCompareOp.SDL_GPU_COMPAREOP_LESS;
                    DepthStencilState.stencilTestEnable = 0;
                }

                SDL_GpuGraphicsPipelineCreateInfo PipelineCreateInfo;
                {
                    PipelineCreateInfo.vertexShader = VertexShader;
                    PipelineCreateInfo.fragmentShader = FragmentShader;
                    PipelineCreateInfo.vertexInputState = VertexInputState;
                    PipelineCreateInfo.primitiveType = SDL_GpuPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST;
                    PipelineCreateInfo.rasterizerState.fillMode = SDL_GpuFillMode.SDL_GPU_FILLMODE_FILL;
                    PipelineCreateInfo.rasterizerState.cullMode = SDL_GpuCullMode.SDL_GPU_CULLMODE_BACK;
                    PipelineCreateInfo.rasterizerState.frontFace = SDL_GpuFrontFace.SDL_GPU_FRONTFACE_CLOCKWISE;
                    PipelineCreateInfo.depthStencilState = DepthStencilState;
                    PipelineCreateInfo.multisampleState.sampleMask = 0xFFFF;
                    PipelineCreateInfo.attachmentInfo = AttachmentInfo;
                }

                SurfelPipeline = SDL_GpuCreateGraphicsPipeline(Device, &PipelineCreateInfo);
            }

            SDL_GpuReleaseShader(Device, FragmentShader);
            SDL_GpuReleaseShader(Device, VertexShader);
        }

        {
            var VertexShader = LoadShader("Reveal.vs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_VERTEX, 0, 0, 0, 0);
            if (VertexShader == IntPtr.Zero)
            {
                SDL_GpuUnclaimWindow(Device, Window);
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("Failed to create vertex shader.");
                return true;
            }

            var FragmentShader = LoadShader("Reveal.fs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT, 0, 0, 0, 1);
            if (VertexShader == IntPtr.Zero)
            {
                SDL_GpuReleaseShader(Device, VertexShader);
                SDL_GpuUnclaimWindow(Device, Window);
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("Failed to create fragment shader.");
                return true;
            }

            unsafe
            {
                var BlendState = new SDL_GpuColorAttachmentBlendState();
                BlendState.blendEnable = 1;
                BlendState.alphaBlendOp = SDL_GpuBlendOp.SDL_GPU_BLENDOP_ADD;
                BlendState.colorBlendOp = SDL_GpuBlendOp.SDL_GPU_BLENDOP_ADD;
                BlendState.colorWriteMask = 0xF;
                BlendState.srcColorBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ONE;
                BlendState.srcAlphaBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ONE;
                BlendState.dstColorBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ZERO;
                BlendState.dstAlphaBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ZERO;

                const int ColorDescCount = 1;
                SDL_GpuColorAttachmentDescription* ColorDesc = stackalloc SDL_GpuColorAttachmentDescription[ColorDescCount];
                {
                    ColorDesc[0].format = SDL_GpuGetSwapchainTextureFormat(Device, Window);
                    ColorDesc[0].blendState = BlendState;
                }

                SDL_GpuGraphicsPipelineAttachmentInfo AttachmentInfo = new();
                {
                    AttachmentInfo.colorAttachmentCount = 1;
                    AttachmentInfo.colorAttachmentDescriptions = ColorDesc;
                    AttachmentInfo.hasDepthStencilAttachment = 0;
                    AttachmentInfo.depthStencilFormat = SDL.SDL_GpuTextureFormat.SDL_GPU_TEXTUREFORMAT_INVALID;
                }

                SDL_GpuVertexInputState VertexInputState = new();
                {
                    VertexInputState.vertexBindingCount = 0;
                    VertexInputState.vertexBindings = null;
                    VertexInputState.vertexAttributeCount = 0;
                    VertexInputState.vertexAttributes = null;
                }

                SDL_GpuDepthStencilState DepthStencilState = new();
                {
                    DepthStencilState.depthTestEnable = 0;
                    DepthStencilState.depthWriteEnable = 0;
                    DepthStencilState.compareOp = SDL.SDL_GpuCompareOp.SDL_GPU_COMPAREOP_NEVER;
                    DepthStencilState.stencilTestEnable = 0;
                }

                SDL_GpuGraphicsPipelineCreateInfo PipelineCreateInfo;
                {
                    PipelineCreateInfo.vertexShader = VertexShader;
                    PipelineCreateInfo.fragmentShader = FragmentShader;
                    PipelineCreateInfo.vertexInputState = VertexInputState;
                    PipelineCreateInfo.primitiveType = SDL_GpuPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST;
                    PipelineCreateInfo.rasterizerState.fillMode = SDL_GpuFillMode.SDL_GPU_FILLMODE_FILL;
                    PipelineCreateInfo.rasterizerState.cullMode = SDL_GpuCullMode.SDL_GPU_CULLMODE_NONE;
                    PipelineCreateInfo.rasterizerState.frontFace = SDL_GpuFrontFace.SDL_GPU_FRONTFACE_CLOCKWISE;
                    PipelineCreateInfo.depthStencilState = DepthStencilState;
                    PipelineCreateInfo.multisampleState.sampleMask = 0xFFFF;
                    PipelineCreateInfo.attachmentInfo = AttachmentInfo;
                }

                RevealPipeline = SDL_GpuCreateGraphicsPipeline(Device, &PipelineCreateInfo);
            }

            SDL_GpuReleaseShader(Device, FragmentShader);
            SDL_GpuReleaseShader(Device, VertexShader);
        }

        {
            var VertexShader = LoadShader("DrawOverlay.vs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_VERTEX, 0, 0, 0, 0);
            if (VertexShader == IntPtr.Zero)
            {
                SDL_GpuUnclaimWindow(Device, Window);
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("Failed to create vertex shader.");
                return true;
            }

            var FragmentShader = LoadShader("DrawOverlay.fs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT, 1, 0, 0, 0);
            if (VertexShader == IntPtr.Zero)
            {
                SDL_GpuReleaseShader(Device, VertexShader);
                SDL_GpuUnclaimWindow(Device, Window);
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("Failed to create fragment shader.");
                return true;
            }

            unsafe
            {
                var BlendState = new SDL_GpuColorAttachmentBlendState();
                BlendState.blendEnable = 1;
                BlendState.alphaBlendOp = SDL_GpuBlendOp.SDL_GPU_BLENDOP_ADD;
                BlendState.colorBlendOp = SDL_GpuBlendOp.SDL_GPU_BLENDOP_ADD;
                BlendState.colorWriteMask = 0xF;
                BlendState.srcColorBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_SRC_ALPHA;
                BlendState.dstColorBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA;
                BlendState.srcAlphaBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ONE;
                BlendState.dstAlphaBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ZERO;

                const int ColorDescCount = 1;
                SDL_GpuColorAttachmentDescription* ColorDesc = stackalloc SDL_GpuColorAttachmentDescription[ColorDescCount];
                {
                    ColorDesc[0].format = SDL_GpuGetSwapchainTextureFormat(Device, Window);
                    ColorDesc[0].blendState = BlendState;
                }

                SDL_GpuGraphicsPipelineAttachmentInfo AttachmentInfo = new();
                {
                    AttachmentInfo.colorAttachmentCount = 1;
                    AttachmentInfo.colorAttachmentDescriptions = ColorDesc;
                    AttachmentInfo.hasDepthStencilAttachment = 0;
                    AttachmentInfo.depthStencilFormat = SDL.SDL_GpuTextureFormat.SDL_GPU_TEXTUREFORMAT_INVALID;
                }

                const int VertexBindingCount = 1;
                SDL_GpuVertexBinding* VertexBindings = stackalloc SDL_GpuVertexBinding[VertexBindingCount];
                {
                    // "InVertex"
                    VertexBindings[0].binding = 0;
                    VertexBindings[0].stride = (uint)sizeof(Vector4);
                    VertexBindings[0].inputRate = SDL_GpuVertexInputRate.SDL_GPU_VERTEXINPUTRATE_VERTEX;
                    VertexBindings[0].stepRate = 0;
                }

                const int VertexAttributeCount = 1;
                SDL_GpuVertexAttribute* VertexAttributes = stackalloc SDL_GpuVertexAttribute[VertexAttributeCount];
                {
                    // "InVertex"
                    VertexAttributes[0].location = 0;
                    VertexAttributes[0].binding = 0;
                    VertexAttributes[0].format = SDL_GpuVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_VECTOR4;
                    VertexAttributes[0].offset = 0;
                }

                SDL_GpuVertexInputState VertexInputState = new();
                {
                    VertexInputState.vertexBindingCount = VertexBindingCount;
                    VertexInputState.vertexBindings = VertexBindings;
                    VertexInputState.vertexAttributeCount = VertexAttributeCount;
                    VertexInputState.vertexAttributes = VertexAttributes;
                }

                SDL_GpuDepthStencilState DepthStencilState = new();
                {
                    DepthStencilState.depthTestEnable = 0;
                    DepthStencilState.depthWriteEnable = 0;
                    DepthStencilState.compareOp = SDL.SDL_GpuCompareOp.SDL_GPU_COMPAREOP_NEVER;
                    DepthStencilState.stencilTestEnable = 0;
                }

                SDL_GpuGraphicsPipelineCreateInfo PipelineCreateInfo;
                {
                    PipelineCreateInfo.vertexShader = VertexShader;
                    PipelineCreateInfo.fragmentShader = FragmentShader;
                    PipelineCreateInfo.vertexInputState = VertexInputState;
                    PipelineCreateInfo.primitiveType = SDL_GpuPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLESTRIP;
                    PipelineCreateInfo.rasterizerState.fillMode = SDL_GpuFillMode.SDL_GPU_FILLMODE_FILL;
                    PipelineCreateInfo.rasterizerState.cullMode = SDL_GpuCullMode.SDL_GPU_CULLMODE_NONE;
                    PipelineCreateInfo.rasterizerState.frontFace = SDL_GpuFrontFace.SDL_GPU_FRONTFACE_CLOCKWISE;
                    PipelineCreateInfo.depthStencilState = DepthStencilState;
                    PipelineCreateInfo.multisampleState.sampleMask = 0xFFFF;
                    PipelineCreateInfo.attachmentInfo = AttachmentInfo;
                }

                OverlayPipeline = SDL_GpuCreateGraphicsPipeline(Device, &PipelineCreateInfo);
            }

            SDL_GpuReleaseShader(Device, FragmentShader);
            SDL_GpuReleaseShader(Device, VertexShader);
        }

        if (SurfelPipeline == IntPtr.Zero)
        {
            SDL_GpuUnclaimWindow(Device, Window);
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
            Console.WriteLine("Failed to create raster pipeline.");
            return true;
        }

        if (RevealPipeline == IntPtr.Zero)
        {
            SDL_GpuUnclaimWindow(Device, Window);
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
            Console.WriteLine("Failed to create raster pipeline.");
            return true;
        }

        if (OverlayPipeline == IntPtr.Zero)
        {
            SDL_GpuUnclaimWindow(Device, Window);
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
            Console.WriteLine("Failed to create raster pipeline.");
            return true;
        }

        {
            SplatVertexBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(float) * 3 * (uint)SplatMesh.Vertices.Length);
            SDL_GpuSetBufferName(Device, SplatVertexBuffer, "SplatVertexBuffer"u8);

            SplatIndexBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_INDEX_BIT,
                sizeof(UInt16) * (uint)SplatMesh.Indices.Length);
            SDL_GpuSetBufferName(Device, SplatIndexBuffer, "SplatIndexBuffer"u8);

            UploadVector3s(SplatVertexBuffer, SplatMesh.Vertices);
            UploadUInt16s(SplatIndexBuffer, SplatMesh.Indices);
        }

        {
            SplatWorldPositionBuffer_L = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(Int32) * 3 * (uint)Settings.MaxSurfels);
            SDL_GpuSetBufferName(Device, SplatWorldPositionBuffer_L, "SplatWorldPositionBuffer_L"u8);

            SplatWorldPositionBuffer_H = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(Int32) * 3 * (uint)Settings.MaxSurfels);
            SDL_GpuSetBufferName(Device, SplatWorldPositionBuffer_H, "SplatWorldPositionBuffer_H"u8);

            SplatColorBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(float) * 3 * (uint)Settings.MaxSurfels);
            SDL_GpuSetBufferName(Device, SplatColorBuffer, "SplatColorBuffer"u8);
        }

#if false
        {
            float ScreenWidth = (float)ColorTextureDesc.width;
            float ScreenHeight = (float)ColorTextureDesc.height;

            {
                SVGResource TigerSVG = new("tiger.svg");
                var Overlay = new ImageOverlay(Device, TigerSVG.Render((int)ScreenWidth, -1));
                Overlay.AlignModeY = ImageOverlay.AlignModeV.Top;
                Overlay.AlignY = -0.0f;
                Overlay.ScaleX = 0.5f;
                Overlay.UploadVertices(ScreenWidth, ScreenHeight);
                Overlays.Add(Overlay);
            }

            {
                SVGResource CameraSVG = new("Digital_Camera.svg");
                var Overlay = new ImageOverlay(Device, CameraSVG.Render((int)ScreenWidth, -1));
                Overlay.AlignModeY = ImageOverlay.AlignModeV.Bottom;
                Overlay.AlignY = -0.15f;
                Overlay.ScaleX = 0.75f;
                Overlay.UploadVertices(ScreenWidth, ScreenHeight);
                Overlays.Add(Overlay);
            }

            {
                var Overlay = new ImageOverlay(Device, Michroma.Render("hail eris", 0.1f, ScreenHeight));
                Overlay.ScaleModeX = ImageOverlay.ScaleMode.Aspect;
                Overlay.ScaleModeY = ImageOverlay.ScaleMode.Screen;
                Overlay.ScaleY = 0.1f;
                Overlay.UploadVertices(ScreenWidth, ScreenHeight);
                Overlays.Add(Overlay);
            }
        }
#endif

        Console.WriteLine("Ignition successful.");
        return false;
    }

    private void MaybeReleaseBuffer(IntPtr BufferPtr)
    {
        if (BufferPtr != IntPtr.Zero)
        {
            SDL_GpuReleaseBuffer(Device, BufferPtr);
        }
    }

    private void MaybeReleaseTexture(IntPtr Texture)
    {
        if (Texture != IntPtr.Zero)
        {
            SDL_GpuReleaseTexture(Device, Texture);
        }
    }

    private void MaybeReleaseReleaseGraphicsPipeline(IntPtr PipelinePtr)
    {
        if (PipelinePtr != IntPtr.Zero)
        {
            SDL_GpuReleaseGraphicsPipeline(Device, PipelinePtr);
        }
    }

    public void Teardown()
    {
        if (Overlay != null)
        {
            Overlay.PropagateRelease();
        }

        if (Device != IntPtr.Zero)
        {
            foreach (ImageOverlay Overlay in Overlays)
            {
                Overlay.Release();
            }
            Overlays.Clear();

            MaybeReleaseBuffer(SplatColorBuffer);
            MaybeReleaseBuffer(SplatWorldPositionBuffer_L);
            MaybeReleaseBuffer(SplatWorldPositionBuffer_H);
            MaybeReleaseBuffer(SplatIndexBuffer);
            MaybeReleaseBuffer(SplatVertexBuffer);
            MaybeReleaseTexture(ColorTexture);
            MaybeReleaseTexture(DepthTexture);
            MaybeReleaseReleaseGraphicsPipeline(SurfelPipeline);
            MaybeReleaseReleaseGraphicsPipeline(RevealPipeline);
            MaybeReleaseReleaseGraphicsPipeline(OverlayPipeline);
            SDL_GpuUnclaimWindow(Device, Window);
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
        }
    }

    public bool Advance(FrameInfo Frame, RenderingConfig Settings, bool DebugClear, HighLevelRenderer HighRenderer)
    {
        UploadFixies(SplatWorldPositionBuffer_L, SplatWorldPositionBuffer_H, HighRenderer.PositionUpload, true);
        UploadVector3s(SplatColorBuffer, HighRenderer.ColorUpload, true);

        IntPtr CommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
        if (CommandBuffer == IntPtr.Zero)
        {
            Console.WriteLine("GpuAcquireCommandBuffer failed.");
            return true;
        }

        (IntPtr SwapchainTexture, UInt32 Width, UInt32 Height) = SDL_GpuAcquireSwapchainTexture(CommandBuffer, Window);
        if (SwapchainTexture != IntPtr.Zero)
        {
            if (Overlay != null)
            {
                Overlay.Advance(Frame);
            }

            {
                string Text;
                if (SpeedOfLight > 0.0001)
                {
                    Text = $"{SpeedOfLight} c";
                }
                else if (MilesPerHour > 1.0)
                {
                    Text = $"{Double.Round(MilesPerHour)} mph";
                }
                else if (MilesPerHour > 0.1)
                {
                    Text = $"{Double.Round(MilesPerHour, 1)} mph";
                }
                else
                {
                    Text = $"{Double.Round(MilesPerHour, 2)} mph";
                }
                Speedometer.UploadTexture(Device, Michroma.Render(Text, Speedometer.ScaleY * 1.5f, (float)Height));
                Speedometer.UploadVertices((float)Width, (float)Height);
            }

            ViewInfoUpload ViewInfo;
            {
                ViewInfo.WorldToView = HighRenderer.WorldToView;
                ViewInfo.ViewToClip = HighRenderer.ViewToClip;

                ViewInfo.MovementProjection = new();
                {
                    (Fixie Dir, FixedInt Mag) = Fixie.Normalize(HighRenderer.MovementProjection);
                    Vector4 WorldDir = Dir.ToVector4();
                    Vector4 ViewDir = Vector4.Transform(WorldDir, HighRenderer.WorldToView);
                    ViewDir /= ViewDir.W;
                    ViewInfo.MovementProjection[0] = ViewDir[0];
                    ViewInfo.MovementProjection[1] = ViewDir[1];
                    ViewInfo.MovementProjection[2] = ViewDir[2];
                    ViewInfo.MovementProjection[3] = (float)Mag;
                }

                ViewInfo.EyeWorldPosition_L = new();
                ViewInfo.EyeWorldPosition_H = new();
                for (int Lane = 0; Lane < 3; ++Lane)
                {
                    (UInt32 Low, UInt32 High) = HighRenderer.Eye.Lanes[Lane].Split();
                    ViewInfo.EyeWorldPosition_L[Lane] = Low;
                    ViewInfo.EyeWorldPosition_H[Lane] = High;
                }

                ViewInfo.SplatDiameter = HighRenderer.SplatDiameter;
                ViewInfo.SplatDepth = Settings.SplatDepth;
                ViewInfo.AspectRatio = Frame.AspectRatio;
            }

            SDL_GpuViewport Viewport;
            {
                Viewport.x = 0;
                Viewport.y = 0;
                Viewport.w = Width;
                Viewport.h = Height;
                Viewport.minDepth = 0.1f;
                Viewport.maxDepth = 1.0f;
            }

            SDL_Rect ScissorRect;
            {
                ScissorRect.x = 0;
                ScissorRect.y = 0;
                ScissorRect.w = (int)Width;
                ScissorRect.h = (int)Height;
            }

            unsafe
            {
                SDL_GpuPushVertexUniformData(CommandBuffer, 0, &ViewInfo, (uint)sizeof(ViewInfoUpload));

                {
                    SDL_GpuColorAttachmentInfo ColorAttachmentInfo;
                    {
                        ColorAttachmentInfo.textureSlice.texture = ColorTexture;
                        ColorAttachmentInfo.clearColor.r = 0.0f;
                        ColorAttachmentInfo.clearColor.g = 0.0f;
                        ColorAttachmentInfo.clearColor.b = 0.0f;
                        ColorAttachmentInfo.clearColor.a = 1.0f;
                        ColorAttachmentInfo.loadOp = DebugClear ? SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_CLEAR : SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_LOAD;
                        ColorAttachmentInfo.storeOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_STORE;
                        ColorAttachmentInfo.cycle = 0;
                    }

                    SDL_GpuDepthStencilAttachmentInfo DepthStencilAttachmentInfo;
                    {
                        DepthStencilAttachmentInfo.textureSlice.texture = DepthTexture;
                        DepthStencilAttachmentInfo.depthStencilClearValue.depth = 1;
                        DepthStencilAttachmentInfo.depthStencilClearValue.stencil = 0;
                        DepthStencilAttachmentInfo.loadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_CLEAR;
                        DepthStencilAttachmentInfo.storeOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_DONT_CARE;
                        DepthStencilAttachmentInfo.stencilLoadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_DONT_CARE;
                        DepthStencilAttachmentInfo.stencilStoreOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_DONT_CARE;
                        DepthStencilAttachmentInfo.cycle = 0;
                    }

                    Span<SDL_GpuBufferBinding> VertexBufferBindings = stackalloc SDL_GpuBufferBinding[4];
                    {
                        VertexBufferBindings[0].buffer = SplatVertexBuffer;
                        VertexBufferBindings[0].offset = 0;
                        VertexBufferBindings[1].buffer = SplatWorldPositionBuffer_L;
                        VertexBufferBindings[1].offset = 0;
                        VertexBufferBindings[2].buffer = SplatWorldPositionBuffer_H;
                        VertexBufferBindings[2].offset = 0;
                        VertexBufferBindings[3].buffer = SplatColorBuffer;
                        VertexBufferBindings[3].offset = 0;
                    }

                    SDL_GpuBufferBinding IndexBufferBinding;
                    {
                        IndexBufferBinding.buffer = SplatIndexBuffer;
                        IndexBufferBinding.offset = 0;
                    }

                    IntPtr SurfelPass = SDL_GpuBeginRenderPass(CommandBuffer, &ColorAttachmentInfo, 1, &DepthStencilAttachmentInfo);
                    SDL_GpuPushDebugGroup(CommandBuffer, "Surfel Pass"u8);
                    {
                        SDL_GpuBindGraphicsPipeline(SurfelPass, SurfelPipeline);
                        SDL_GpuSetViewport(SurfelPass, &Viewport);
                        SDL_GpuSetScissor(SurfelPass, &ScissorRect);

                        if (HighRenderer.LiveSurfels > 0)
                        {
                            fixed (SDL_GpuBufferBinding* VertexBufferBindingsPtr = VertexBufferBindings)
                            {
                                SDL_GpuBindVertexBuffers(SurfelPass, 0, VertexBufferBindingsPtr, 4);
                            }
                            SDL_GpuBindIndexBuffer(
                                SurfelPass, &IndexBufferBinding, SDL_GpuIndexElementSize.SDL_GPU_INDEXELEMENTSIZE_16BIT);
                            SDL_GpuDrawIndexedPrimitives(SurfelPass, 0, 0, SplatMesh.TriangleCount * 3, HighRenderer.LiveSurfels);
                        }
                    }
                    SDL_GpuPopDebugGroup(CommandBuffer);
                    SDL_GpuEndRenderPass(SurfelPass);
                }

                {
                    SDL_GpuColorAttachmentInfo ColorAttachmentInfo;
                    {
                        ColorAttachmentInfo.textureSlice.texture = SwapchainTexture;
                        ColorAttachmentInfo.clearColor.r = 0.0f;
                        ColorAttachmentInfo.clearColor.g = 0.0f;
                        ColorAttachmentInfo.clearColor.b = 0.0f;
                        ColorAttachmentInfo.clearColor.a = 1.0f;

                        ColorAttachmentInfo.loadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_DONT_CARE;
                        ColorAttachmentInfo.storeOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_STORE;
                        ColorAttachmentInfo.cycle = 0;
                    }

                    IntPtr RevealPass = SDL_GpuBeginRenderPass(CommandBuffer, &ColorAttachmentInfo, 1, null);
                    SDL_GpuPushDebugGroup(CommandBuffer, "Reveal Pass"u8);
                    {
                        SDL_GpuBindGraphicsPipeline(RevealPass, RevealPipeline);
                        SDL_GpuSetViewport(RevealPass, &Viewport);
                        SDL_GpuSetScissor(RevealPass, &ScissorRect);

                        Span<SDL_GpuTextureSlice> TextureBindings = stackalloc SDL_GpuTextureSlice[1];
                        {
                            TextureBindings[0].texture = ColorTexture;
                            TextureBindings[0].mipLevel = 0;
                            TextureBindings[0].layer = 0;
                        }

                        fixed (SDL_GpuTextureSlice* TextureBindingsPtr = TextureBindings)
                        {
                            SDL_GpuBindFragmentStorageTextures(RevealPass, 0, TextureBindingsPtr, 1);
                        }
                        SDL_GpuDrawPrimitives(RevealPass, 0, 3);
                    }
                    SDL_GpuPopDebugGroup(CommandBuffer);
                    SDL_GpuEndRenderPass(RevealPass);
                }

                if (Overlays.Count > 0)
                {
                    SDL_GpuColorAttachmentInfo ColorAttachmentInfo;
                    {
                        ColorAttachmentInfo.textureSlice.texture = SwapchainTexture;
                        ColorAttachmentInfo.clearColor.r = 0.0f;
                        ColorAttachmentInfo.clearColor.g = 0.0f;
                        ColorAttachmentInfo.clearColor.b = 0.0f;
                        ColorAttachmentInfo.clearColor.a = 1.0f;

                        ColorAttachmentInfo.loadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_LOAD;
                        ColorAttachmentInfo.storeOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_STORE;
                        ColorAttachmentInfo.cycle = 0;
                    }

                    IntPtr OverlayPass = SDL_GpuBeginRenderPass(CommandBuffer, &ColorAttachmentInfo, 1, null);
                    SDL_GpuPushDebugGroup(CommandBuffer, "Overlay Pass"u8);
                    {
                        SDL_GpuBindGraphicsPipeline(OverlayPass, OverlayPipeline);
                        SDL_GpuSetViewport(OverlayPass, &Viewport);
                        SDL_GpuSetScissor(OverlayPass, &ScissorRect);

                        if (Overlay != null)
                        {
                            Overlay.Draw(OverlayPass);
                        }

                        foreach (ImageOverlay Overlay in Overlays)
                        {
                            if (Overlay.Texture != IntPtr.Zero)
                            {
                                SDL_GpuBufferBinding VertexBufferBindings;
                                {
                                    VertexBufferBindings.buffer = Overlay.VertexBuffer;
                                    VertexBufferBindings.offset = 0;
                                }

                                SDL_GpuTextureSamplerBinding SamplerBindings;
                                {
                                    SamplerBindings.texture = Overlay.Texture;
                                    SamplerBindings.sampler = Overlay.Sampler;
                                }

                                unsafe
                                {
                                    SDL_GpuBindVertexBuffers(OverlayPass, 0, &VertexBufferBindings, 1);
                                    SDL_GpuBindFragmentSamplers(OverlayPass, 0, &SamplerBindings, 1);
                                }

                                SDL_GpuDrawPrimitives(OverlayPass, 0, 4);
                            }
                        }
                    }
                    SDL_GpuPopDebugGroup(CommandBuffer);
                    SDL_GpuEndRenderPass(OverlayPass);
                }
            }
        }
        SDL_GpuSubmit(CommandBuffer);
        return false;
    }
}
