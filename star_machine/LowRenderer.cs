
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


class LowLevelRenderer
{
    public IntPtr Window = IntPtr.Zero;
    private IntPtr Device = IntPtr.Zero;
    private IntPtr SimplePipeline = IntPtr.Zero;
    private IntPtr DepthTexture = IntPtr.Zero;
    private IntPtr SplatVertexBuffer = IntPtr.Zero;
    private IntPtr SplatIndexBuffer = IntPtr.Zero;
    private IntPtr SplatWorldPositionBuffer_L = IntPtr.Zero;
    private IntPtr SplatWorldPositionBuffer_H = IntPtr.Zero;
    private IntPtr SplatColorBuffer = IntPtr.Zero;

    private SplatGenerator SplatMesh;

    public LowLevelRenderer(SplatGenerator InSplatMesh)
    {
        SplatMesh = InSplatMesh;
    }

    private IntPtr LoadShader(
        string Path,
        SDL_GpuShaderStage ShaderStage,
        uint SamplerCount,
        uint UniformBufferCount,
        uint StorageBufferCount,
        uint StorageTextureCount)
    {
        byte[] ShaderBlob;
        using (Stream? ResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("StarMachine." + Path))
        {
            if (ResourceStream != null)
            {
                ShaderBlob = new byte[ResourceStream.Length];
                ResourceStream.Read(ShaderBlob, 0, (int)ResourceStream.Length);
            }
            else
            {
                return IntPtr.Zero;
            }
        }

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
            uvec3* ScratchSpace_L = stackalloc uvec3[UploadData.Length];
            uvec3* ScratchSpace_H = stackalloc uvec3[UploadData.Length];
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

            UploadBytes(Buffer_L, (byte*)ScratchSpace_L, UploadSize, Cycling);
            UploadBytes(Buffer_H, (byte*)ScratchSpace_H, UploadSize, Cycling);
        }
    }

    public bool Boot(RenderingConfig Settings)
    {
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

        Window = SDL_CreateWindow("Star Machine"u8, 900, 900, WindowFlags);
        if (Window == IntPtr.Zero)
        {
            Console.WriteLine("SDL3 failed to create a window.");
            return true;
        }

        {
            const int DebugMode = 1;
            const int PreferLowPower = 0;
            var Backends = new ulong[]
            {
                (ulong)SDL.SDL_GpuBackendBits.SDL_GPU_BACKEND_D3D11,
                (ulong)SDL.SDL_GpuBackendBits.SDL_GPU_BACKEND_METAL,
                (ulong)SDL.SDL_GpuBackendBits.SDL_GPU_BACKEND_VULKAN,
            };
            foreach (ulong BackendFlag in Backends)
            {
                Device = SDL_GpuCreateDevice(BackendFlag, DebugMode, PreferLowPower);
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
            (IntPtr BackBuffer, UInt32 Width, UInt32 Height) = SDL_GpuAcquireSwapchainTexture(CommandBuffer, Window);
            SDL_GpuSubmit(CommandBuffer);

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
                    ColorDesc[0].format = SDL_GpuGetSwapchainTextureFormat(Device, Window);
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

                SimplePipeline = SDL_GpuCreateGraphicsPipeline(Device, &PipelineCreateInfo);
            }

            SDL_GpuReleaseShader(Device, FragmentShader);
            SDL_GpuReleaseShader(Device, VertexShader);
        }

        if (SimplePipeline == IntPtr.Zero)
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

            SplatIndexBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_INDEX_BIT,
                sizeof(UInt16) * (uint)SplatMesh.Indices.Length);

            UploadVector3s(SplatVertexBuffer, SplatMesh.Vertices);
            UploadUInt16s(SplatIndexBuffer, SplatMesh.Indices);
        }

        {
            SplatWorldPositionBuffer_L = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(Int32) * 3 * (uint)Settings.MaxSurfels);

            SplatWorldPositionBuffer_H = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(Int32) * 3 * (uint)Settings.MaxSurfels);

            SplatColorBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(float) * 3 * (uint)Settings.MaxSurfels);
        }

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
        if (Device != IntPtr.Zero)
        {
            MaybeReleaseBuffer(SplatColorBuffer);
            MaybeReleaseBuffer(SplatWorldPositionBuffer_L);
            MaybeReleaseBuffer(SplatWorldPositionBuffer_H);
            MaybeReleaseBuffer(SplatIndexBuffer);
            MaybeReleaseBuffer(SplatVertexBuffer);
            MaybeReleaseTexture(DepthTexture);
            MaybeReleaseReleaseGraphicsPipeline(SimplePipeline);
            SDL_GpuUnclaimWindow(Device, Window);
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
        }
    }

    public bool Advance(FrameInfo Frame, RenderingConfig Settings, HighLevelRenderer HighRenderer)
    {
        UploadFixies(SplatWorldPositionBuffer_L, SplatWorldPositionBuffer_H, HighRenderer.PositionUpload, true);
        UploadVector3s(SplatColorBuffer, HighRenderer.ColorUpload, true);

        IntPtr CommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
        if (CommandBuffer == IntPtr.Zero)
        {
            Console.WriteLine("GpuAcquireCommandBuffer failed.");
            return true;
        }

        (IntPtr BackBuffer, UInt32 Width, UInt32 Height) = SDL_GpuAcquireSwapchainTexture(CommandBuffer, Window);
        if (BackBuffer != IntPtr.Zero)
        {
            SDL_GpuColorAttachmentInfo ColorAttachmentInfo;
            {
                ColorAttachmentInfo.textureSlice.texture = BackBuffer;
                ColorAttachmentInfo.clearColor.r = 0.0f;
                ColorAttachmentInfo.clearColor.g = 0.0f;
                ColorAttachmentInfo.clearColor.b = 0.0f;
                ColorAttachmentInfo.clearColor.a = 1.0f;
#if false
                ColorAttachmentInfo.loadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_CLEAR;
#else
                ColorAttachmentInfo.loadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_DONT_CARE;
#endif
                ColorAttachmentInfo.storeOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_STORE;
            }

            SDL_GpuDepthStencilAttachmentInfo DepthStencilAttachmentInfo;
            {
                DepthStencilAttachmentInfo.textureSlice.texture = DepthTexture;
                DepthStencilAttachmentInfo.cycle = 1;
                DepthStencilAttachmentInfo.depthStencilClearValue.depth = 1;
                DepthStencilAttachmentInfo.depthStencilClearValue.stencil = 0;
                DepthStencilAttachmentInfo.loadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_CLEAR;
                DepthStencilAttachmentInfo.storeOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_DONT_CARE;
                DepthStencilAttachmentInfo.stencilLoadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_DONT_CARE;
                DepthStencilAttachmentInfo.stencilStoreOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_DONT_CARE;
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

            SDL_GpuRect ScissorRect;
            {
                ScissorRect.x = 0;
                ScissorRect.y = 0;
                ScissorRect.w = (int)Width;
                ScissorRect.h = (int)Height;
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

            unsafe
            {
                SDL_GpuPushVertexUniformData(CommandBuffer, 0, &ViewInfo, (uint)sizeof(ViewInfoUpload));

                IntPtr RenderPass = SDL_GpuBeginRenderPass(CommandBuffer, &ColorAttachmentInfo, 1, &DepthStencilAttachmentInfo);
                {
                    SDL_GpuBindGraphicsPipeline(RenderPass, SimplePipeline);
                    SDL_GpuSetViewport(RenderPass, &Viewport);
                    SDL_GpuSetScissor(RenderPass, &ScissorRect);

                    if (HighRenderer.LiveSurfels > 0)
                    {
                        fixed (SDL_GpuBufferBinding* VertexBufferBindingsPtr = VertexBufferBindings)
                        {
                            SDL_GpuBindVertexBuffers(RenderPass, 0, VertexBufferBindingsPtr, 4);
                        }
                        SDL_GpuBindIndexBuffer(
                            RenderPass, &IndexBufferBinding, SDL_GpuIndexElementSize.SDL_GPU_INDEXELEMENTSIZE_16BIT);
                        SDL_GpuDrawIndexedPrimitives(RenderPass, 0, 0, SplatMesh.TriangleCount, HighRenderer.LiveSurfels);
                    }
                }
                SDL_GpuEndRenderPass(RenderPass);
            }
        }
        SDL_GpuSubmit(CommandBuffer);
        return false;
    }
}
