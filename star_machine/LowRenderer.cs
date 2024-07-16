
using System.Reflection;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Matrix4x4 = System.Numerics.Matrix4x4;
using static System.Buffer;

using SDL3;
using static SDL3.SDL;

namespace StarMachine;


struct ViewInfoUpload
{
    public Matrix4x4 WorldToView;
    public Matrix4x4 ViewToClip;
    public float SplatDiameter;
    public float SplatDepth;
    public float AspectRatio;
}


class LowLevelRenderer
{
    public IntPtr Window = IntPtr.Zero;
    private IntPtr Device = IntPtr.Zero;
    private IntPtr SimplePipeline = IntPtr.Zero;
    private IntPtr SplatVertexBuffer = IntPtr.Zero;
    private IntPtr SplatIndexBuffer = IntPtr.Zero;
    private IntPtr SplatWorldPositionBuffer = IntPtr.Zero;
    private IntPtr SplatColorBuffer = IntPtr.Zero;

    private SplatGenerator SplatMesh;

    // These will eventually be the ring buffers for drawing
    public Vector3[] WorldPositionUpload = Array.Empty<Vector3>();
    public Vector3[] ColorUpload = Array.Empty<Vector3>();

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

    public bool Boot(RenderingConfig Settings)
    {
        if (SDL_Init(SDL_INIT_VIDEO) < 0)
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
                }

                const int VertexBindingCount = 3;
                SDL_GpuVertexBinding* VertexBindings = stackalloc SDL_GpuVertexBinding[VertexBindingCount];
                {
                    // "LocalVertexOffset"
                    VertexBindings[0].binding = 0;
                    VertexBindings[0].stride = (uint)sizeof(Vector3);
                    VertexBindings[0].inputRate = SDL_GpuVertexInputRate.SDL_GPU_VERTEXINPUTRATE_VERTEX;
                    VertexBindings[0].stepRate = 0;

                    // "SplatWorldPosition"
                    VertexBindings[1].binding = 1;
                    VertexBindings[1].stride = (uint)sizeof(Vector3);
                    VertexBindings[1].inputRate = SDL_GpuVertexInputRate.SDL_GPU_VERTEXINPUTRATE_INSTANCE;
                    VertexBindings[1].stepRate = 1;

                    // "SplatColor"
                    VertexBindings[2].binding = 2;
                    VertexBindings[2].stride = (uint)sizeof(Vector3);
                    VertexBindings[2].inputRate = SDL_GpuVertexInputRate.SDL_GPU_VERTEXINPUTRATE_INSTANCE;
                    VertexBindings[2].stepRate = 1;
                }

                const int VertexAttributeCount = 3;
                SDL_GpuVertexAttribute* VertexAttributes = stackalloc SDL_GpuVertexAttribute[VertexAttributeCount];
                {
                    // "LocalVertexOffset"
                    VertexAttributes[0].location = 0;
                    VertexAttributes[0].binding = 0;
                    VertexAttributes[0].format = SDL_GpuVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_VECTOR3;
                    VertexAttributes[0].offset = 0;

                    // "SplatWorldPosition"
                    VertexAttributes[1].location = 1;
                    VertexAttributes[1].binding = 1;
                    VertexAttributes[1].format = SDL_GpuVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_VECTOR3;
                    VertexAttributes[1].offset = 0;

                    // "SplatColor"
                    VertexAttributes[2].location = 2;
                    VertexAttributes[2].binding = 2;
                    VertexAttributes[2].format = SDL_GpuVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_VECTOR3;
                    VertexAttributes[2].offset = 0;
                }

                SDL_GpuVertexInputState VertexInputState = new();
                {
                    VertexInputState.vertexBindingCount = VertexBindingCount;
                    VertexInputState.vertexBindings = VertexBindings;
                    VertexInputState.vertexAttributeCount = VertexAttributeCount;
                    VertexInputState.vertexAttributes = VertexAttributes;
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

        WorldPositionUpload = new Vector3[]
        {
            new Vector3(-0.5f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.5f, 0.0f, 0.0f)
        };

        ColorUpload = new Vector3[]
        {
            new Vector3(0.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 0.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 0.0f)
        };

        SplatVertexBuffer = SDL_GpuCreateBuffer(
            Device,
            (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
            sizeof(float) * 3 * (uint)SplatMesh.Vertices.Length);

        UploadVector3s(SplatVertexBuffer, SplatMesh.Vertices);


        SplatIndexBuffer = SDL_GpuCreateBuffer(
            Device,
            (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_INDEX_BIT,
            sizeof(UInt16) * (uint)SplatMesh.Indices.Length);

        UploadUInt16s(SplatIndexBuffer, SplatMesh.Indices);


        SplatWorldPositionBuffer = SDL_GpuCreateBuffer(
            Device,
            (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
            sizeof(float) * 3 * (uint)WorldPositionUpload.Length);


        SplatColorBuffer = SDL_GpuCreateBuffer(
            Device,
            (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
            sizeof(float) * 3 * (uint)ColorUpload.Length);


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
            MaybeReleaseBuffer(SplatWorldPositionBuffer);
            MaybeReleaseBuffer(SplatIndexBuffer);
            MaybeReleaseBuffer(SplatVertexBuffer);
            MaybeReleaseReleaseGraphicsPipeline(SimplePipeline);
            SDL_GpuUnclaimWindow(Device, Window);
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
        }
    }

    public bool Advance(FrameInfo Frame)
    {
        UploadVector3s(SplatWorldPositionBuffer, WorldPositionUpload, true);
        UploadVector3s(SplatColorBuffer, ColorUpload, true);

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
                ColorAttachmentInfo.clearColor.r = 0.2f;
                ColorAttachmentInfo.clearColor.g = 0.2f;
                ColorAttachmentInfo.clearColor.b = 0.2f;
                ColorAttachmentInfo.clearColor.a = 1.0f;
                ColorAttachmentInfo.loadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_CLEAR;
                ColorAttachmentInfo.storeOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_STORE;
            }

            ViewInfoUpload ViewInfo;
            {
                ViewInfo.WorldToView = Matrix4x4.Identity;
                ViewInfo.ViewToClip = Matrix4x4.Identity;
                ViewInfo.SplatDiameter = 0.25f;
                ViewInfo.SplatDepth = 0.25f;
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

            Span<SDL_GpuBufferBinding> VertexBufferBindings = stackalloc SDL_GpuBufferBinding[3];
            {
                VertexBufferBindings[0].buffer = SplatVertexBuffer;
                VertexBufferBindings[0].offset = 0;
                VertexBufferBindings[1].buffer = SplatWorldPositionBuffer;
                VertexBufferBindings[1].offset = 0;
                VertexBufferBindings[2].buffer = SplatColorBuffer;
                VertexBufferBindings[2].offset = 0;
            }

            SDL_GpuBufferBinding IndexBufferBinding;
            {
                IndexBufferBinding.buffer = SplatIndexBuffer;
                IndexBufferBinding.offset = 0;
            }

            unsafe
            {
                SDL_GpuPushVertexUniformData(CommandBuffer, 0, &ViewInfo, (uint)sizeof(ViewInfoUpload));

                IntPtr RenderPass = SDL_GpuBeginRenderPass(CommandBuffer, &ColorAttachmentInfo, 1, null);
                {
                    SDL_GpuBindGraphicsPipeline(RenderPass, SimplePipeline);
                    SDL_GpuSetViewport(RenderPass, &Viewport);
                    SDL_GpuSetScissor(RenderPass, &ScissorRect);
                    fixed (SDL_GpuBufferBinding* VertexBufferBindingsPtr = VertexBufferBindings)
                    {
                        SDL_GpuBindVertexBuffers(RenderPass, 0, VertexBufferBindingsPtr, 3);
                    }
                    SDL_GpuBindIndexBuffer(RenderPass, &IndexBufferBinding, SDL_GpuIndexElementSize.SDL_GPU_INDEXELEMENTSIZE_16BIT);
                    SDL_GpuDrawIndexedPrimitives(RenderPass, 0, 0, SplatMesh.TriangleCount, 3);
                }
                SDL_GpuEndRenderPass(RenderPass);
            }
        }
        SDL_GpuSubmit(CommandBuffer);
        return false;
    }
}
