
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Numerics;
using SDL3;
using static SDL3.SDL;
using static System.Buffer;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Matrix4x4 = System.Numerics.Matrix4x4;
using System.Diagnostics.Tracing;

namespace StarMachine
{
    internal struct ViewInfoUpload
    {
        public Matrix4x4 LocalToWorld;
        public Matrix4x4 WorldToView;
        public Matrix4x4 ViewToClip;
        public float SplatDiameter;
        public float SplatDepth;
        public float AspectRatio;
    }

    internal class ConstantBuffer
    {

    }

    internal class Program
    {
        private static IntPtr LoadShader(
            IntPtr Device,
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

        static unsafe void UploadBytes(IntPtr Device, IntPtr DestinationBuffer, byte* UploadData, uint UploadSize, bool Cycling = false)
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

        static void UploadVector3s(IntPtr Device, IntPtr DestinationBuffer, Vector3[] UploadData, bool Cycling = false)
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

                UploadBytes(Device, DestinationBuffer, (byte*)ScratchSpace, UploadSize, Cycling);
            }
        }

        static void UploadUInt16s(IntPtr Device, IntPtr DestinationBuffer, UInt16[] UploadData, bool Cycling = false)
        {
            uint UploadSize = sizeof(UInt16) * (uint)UploadData.Length;
            unsafe
            {
                fixed (UInt16* UploadDataPtr = UploadData)
                {
                    UploadBytes(Device, DestinationBuffer, (byte*)UploadDataPtr, UploadSize, Cycling);
                }
            }
        }

        static void Main(string[] args)
        {
#if false
            Console.WriteLine("Embedded resources:");
            string[] ResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (string ResourceName in ResourceNames)
            {
                Console.WriteLine($" - {ResourceName}");
            }
#endif

            if (SDL_Init(SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("SDL3 initialization failed.");
                return;
            }

            ulong WindowFlags = SDL.SDL_WINDOW_HIGH_PIXEL_DENSITY;
            var Window = SDL_CreateWindow("Star Machine"u8, 900, 900, WindowFlags);
            if (Window == IntPtr.Zero)
            {
                Console.WriteLine("SDL3 failed to create a window.");
                return;
            }
            Console.WriteLine($"Window pixel density: {SDL_GetWindowPixelDensity(Window)}");
            Console.WriteLine($"Window display scale: {SDL_GetWindowDisplayScale(Window)}");

            var Device = SDL_GpuCreateDevice((ulong)SDL.SDL_GpuBackendBits.SDL_GPU_BACKEND_VULKAN, 1, 0);
            if (Device == IntPtr.Zero)
            {
                SDL_DestroyWindow(Window);
                Console.WriteLine("SDL3 failed to create GPU device.");
                return;
            }

            if (SDL_GpuClaimWindow(Device, Window, SDL.SDL_GpuSwapchainComposition.SDL_GPU_SWAPCHAINCOMPOSITION_SDR, SDL.SDL_GpuPresentMode.SDL_GPU_PRESENTMODE_VSYNC) == 0)
            {
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("SDL3 failed to attach Window to GPU device.");
                return;
            }

            Console.WriteLine("Ignition successful.");

            IntPtr SimplePipeline = IntPtr.Zero;
            {
                var VertexShader = LoadShader(Device, "draw_splat.vs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_VERTEX, 0, 1, 0, 0);
                if (VertexShader == IntPtr.Zero)
                {
                    SDL_GpuUnclaimWindow(Device, Window);
                    SDL_GpuDestroyDevice(Device);
                    SDL_DestroyWindow(Window);
                    Console.WriteLine("Failed to create vertex shader.");
                    return;
                }

                var FragmentShader = LoadShader(Device, "draw_splat.fs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT, 0, 0, 0, 0);
                if (VertexShader == IntPtr.Zero)
                {
                    SDL_GpuReleaseShader(Device, VertexShader);
                    SDL_GpuUnclaimWindow(Device, Window);
                    SDL_GpuDestroyDevice(Device);
                    SDL_DestroyWindow(Window);
                    Console.WriteLine("Failed to create fragment shader.");
                    return;
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
                return;
            }
            else
            {
                Console.WriteLine("Raster pipeline created???");
            }


            var VertexData = new Vector3[]
            {
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3( 1.0f, -1.0f, 0.0f),
                new Vector3( 0.0f,  1.0f, 0.0f)
            };

            var IndexData = new UInt16[] { 0, 1, 2 };


            IntPtr SplatVertexBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(float) * 3 * (uint)VertexData.Length);

            UploadVector3s(Device, SplatVertexBuffer, VertexData);


            IntPtr SplatIndexBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_INDEX_BIT,
                sizeof(UInt16) * (uint)IndexData.Length);

            UploadUInt16s(Device, SplatIndexBuffer, IndexData);



            IntPtr SplatWorldPositionBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(float) * 3 * (uint)VertexData.Length);


            IntPtr SplatColorBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(float) * 3 * (uint)VertexData.Length);

            var WorldPositionUpload = new Vector3[]
            {
                new Vector3(-0.25f, -0.25f, 0.0f),
                new Vector3(0.0f, 0.25f, 0.0f),
                new Vector3(0.25f, -0.25f, 0.0f)
            };

            var ColorUpload = new Vector3[]
            {
                new Vector3(0.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 0.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 0.0f)
            };

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

            UInt32 LastWidth = 0;
            UInt32 LastHeight = 0;

            while (true)
            {
                SDL_Event Event;
                if (SDL_PollEvent(out Event) != 0)
                {
                    if (Event.type == SDL.SDL_EventType.SDL_EVENT_QUIT)
                    {
                        break;
                    }
                }

                UploadVector3s(Device, SplatWorldPositionBuffer, WorldPositionUpload, true);
                UploadVector3s(Device, SplatColorBuffer, ColorUpload, true);

                IntPtr CommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
                if (CommandBuffer == IntPtr.Zero)
                {
                    Console.WriteLine("GpuAcquireCommandBuffer failed.");
                    break;
                }

                (IntPtr BackBuffer, UInt32 Width, UInt32 Height) = SDL_GpuAcquireSwapchainTexture(CommandBuffer, Window);
                if (BackBuffer != IntPtr.Zero)
                {
                    if (Width != LastWidth || Height != LastHeight)
                    {
                        LastWidth = Width;
                        LastHeight = Height;
                        Console.WriteLine($"Width: {Width}, Height: {Height}");
                    }

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
                        ViewInfo.LocalToWorld = Matrix4x4.Identity;
                        ViewInfo.WorldToView = Matrix4x4.Identity;
                        ViewInfo.ViewToClip = Matrix4x4.Identity;
                        ViewInfo.SplatDiameter = 0.25f;
                        ViewInfo.SplatDepth = 0.25f;
                        ViewInfo.AspectRatio = 1.0f;
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
                            SDL_GpuDrawIndexedPrimitives(RenderPass, 0, 0, 1, 3);
                        }
                        SDL_GpuEndRenderPass(RenderPass);
                    }
                }
                SDL_GpuSubmit(CommandBuffer);
            }

            SDL_GpuReleaseBuffer(Device, SplatColorBuffer);
            SDL_GpuReleaseBuffer(Device, SplatWorldPositionBuffer);
            SDL_GpuReleaseBuffer(Device, SplatIndexBuffer);
            SDL_GpuReleaseBuffer(Device, SplatVertexBuffer);
            SDL_GpuReleaseGraphicsPipeline(Device, SimplePipeline);
            SDL_GpuUnclaimWindow(Device, Window);
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
        }
    }
}
