
using System.Text;
using System.Threading;
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

    [System.Runtime.InteropServices.FieldOffset(184)]
    public float PupilOffset;
}


class LowLevelRenderer
{
    public IntPtr Window = IntPtr.Zero;
    public IntPtr Device = IntPtr.Zero;
    private IntPtr SurfelPipeline = IntPtr.Zero;
    private IntPtr RevealPipeline = IntPtr.Zero;
    private IntPtr StereoRevealPipeline = IntPtr.Zero;
    private IntPtr OverlayPipeline = IntPtr.Zero;
    private IntPtr[] ColorTexture = {};
    private IntPtr DepthTexture = IntPtr.Zero;
    private IntPtr SplatVertexBuffer = IntPtr.Zero;
    private IntPtr SplatIndexBuffer = IntPtr.Zero;
    private IntPtr SplatWorldPositionBuffer_L = IntPtr.Zero;
    private IntPtr SplatWorldPositionBuffer_H = IntPtr.Zero;
    private IntPtr SplatColorBuffer = IntPtr.Zero;
    private IntPtr SplatPortalBuffer = IntPtr.Zero;

    private bool StereoRendering = false;
    private float[] PupilOffsets = {};

    private UInt32 CurrentWidth = 0;
    private UInt32 CurrentHeight = 0;

    public RootWidget? Overlay = null;

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

    private void UploadVector4s(IntPtr DestinationBuffer, Vector4[] UploadData, bool Cycling = false)
    {
        uint WordCount = 4 * (uint)UploadData.Length;
        uint UploadSize = sizeof(float) * WordCount;
        unsafe
        {
            float* ScratchSpace = stackalloc float[(int)WordCount];
            int Cursor = 0;
            foreach (Vector4 Vertex in UploadData)
            {
                ScratchSpace[Cursor++] = Vertex.X;
                ScratchSpace[Cursor++] = Vertex.Y;
                ScratchSpace[Cursor++] = Vertex.Z;
                ScratchSpace[Cursor++] = Vertex.W;
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
        StereoRendering = Settings.PupilaryDistance > 0.0f;

        {
            Console.WriteLine($"PlutoVG version: {plutovg_version_string()}");
            Console.WriteLine($"PlutoSVG version: {plutosvg_version_string()}");
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

        Window = SDL_CreateWindow("Star Machine"u8, 720, 480, WindowFlags);
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
            int DebugMode = Settings.DebugMode ? 1 : 0;
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

        if (SDL_GpuSupportsSwapchainComposition(Device, Window, Settings.SwapchainComposition) == 0)
        {
            Console.WriteLine($"Requested swapchain composition is unavailable: {Settings.SwapchainComposition.ToString()}");
            // SDL_GPU_SWAPCHAINCOMPOSITION_SDR is always supported.
            Settings.SwapchainComposition = SDL.SDL_GpuSwapchainComposition.SDL_GPU_SWAPCHAINCOMPOSITION_SDR;
        }

        if (SDL_GpuSupportsPresentMode(Device, Window, Settings.PresentMode) == 0)
        {
            Console.WriteLine($"Requested present mode is unavailable: {Settings.PresentMode.ToString()}");
            // SDL_GPU_PRESENTMODE_VSYNC is always supported.
            Settings.PresentMode = SDL.SDL_GpuPresentMode.SDL_GPU_PRESENTMODE_VSYNC;
        }

        if (SDL_GpuClaimWindow(Device, Window, Settings.SwapchainComposition, Settings.PresentMode) == 0)
        {
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
            Console.WriteLine("SDL3 failed to attach Window to GPU device.");
            return true;
        }
        else
        {
            Console.WriteLine($"Swapchain composition: {Settings.SwapchainComposition.ToString()}");
            Console.WriteLine($"Present mode: {Settings.PresentMode.ToString()}");
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
                CurrentWidth = Width;
                CurrentHeight = Height;
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
                if (StereoRendering)
                {
                    ColorTexture = new IntPtr[2];
                    ColorTexture[0] = SDL_GpuCreateTexture(Device, &ColorTextureDesc);
                    ColorTexture[1] = SDL_GpuCreateTexture(Device, &ColorTextureDesc);
                    SDL_GpuSetTextureName(Device, ColorTexture[0], "Left Stereo ColorTexture"u8);
                    SDL_GpuSetTextureName(Device, ColorTexture[1], "Right Stereo ColorTexture"u8);
                    PupilOffsets = new float[2];
                    PupilOffsets[0] = Settings.PupilaryDistance * 0.5f;
                    PupilOffsets[1] = Settings.PupilaryDistance * -0.5f;
                }
                else
                {
                    ColorTexture = new IntPtr[1];
                    ColorTexture[0] = SDL_GpuCreateTexture(Device, &ColorTextureDesc);
                    SDL_GpuSetTextureName(Device, ColorTexture[0], "ColorTexture"u8);
                    PupilOffsets = new float[1];
                    PupilOffsets[0] = 0.0f;
                }
            }

            // Initial Color Clear
            {
                IntPtr CommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
                foreach (IntPtr Texture in ColorTexture)
                {
                    SDL_GpuColorAttachmentInfo ColorAttachmentInfo;
                    {
                        ColorAttachmentInfo.textureSlice.texture = Texture;
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
            var VertexShader = LoadShader("DrawSplat.vs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_VERTEX, 0, 1, 0, 0);
            if (VertexShader == IntPtr.Zero)
            {
                SDL_GpuUnclaimWindow(Device, Window);
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("Failed to create vertex shader.");
                return true;
            }

            var FragmentShader = LoadShader("DrawSplat.fs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT, 0, 0, 0, 0);
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

                const int VertexBindingCount = 5;
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

                    // "Portal"
                    VertexBindings[4].binding = 4;
                    VertexBindings[4].stride = (uint)sizeof(Vector4);
                    VertexBindings[4].inputRate = SDL_GpuVertexInputRate.SDL_GPU_VERTEXINPUTRATE_INSTANCE;
                    VertexBindings[4].stepRate = 1;
                }

                const int VertexAttributeCount = 5;
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

                    // "Portal"
                    VertexAttributes[4].location = 4;
                    VertexAttributes[4].binding = 4;
                    VertexAttributes[4].format = SDL_GpuVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_VECTOR4;
                    VertexAttributes[4].offset = 0;
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
            if (FragmentShader == IntPtr.Zero)
            {
                SDL_GpuReleaseShader(Device, VertexShader);
                SDL_GpuUnclaimWindow(Device, Window);
                SDL_GpuDestroyDevice(Device);
                SDL_DestroyWindow(Window);
                Console.WriteLine("Failed to create fragment shader.");
                return true;
            }

            var StereoFragmentShader = LoadShader("Stereo.fs.spirv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT, 0, 0, 0, 2);
            if (StereoFragmentShader == IntPtr.Zero)
            {
                SDL_GpuReleaseShader(Device, FragmentShader);
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

                PipelineCreateInfo.fragmentShader = StereoFragmentShader;
                StereoRevealPipeline = SDL_GpuCreateGraphicsPipeline(Device, &PipelineCreateInfo);
            }

            SDL_GpuReleaseShader(Device, StereoFragmentShader);
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
                // premultiplied alpha
                BlendState.srcColorBlendFactor = SDL_GpuBlendFactor.SDL_GPU_BLENDFACTOR_ONE;
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

            SplatPortalBuffer = SDL_GpuCreateBuffer(
                Device,
                (uint)SDL_GpuBufferUsageFlagBits.SDL_GPU_BUFFERUSAGE_VERTEX_BIT,
                sizeof(float) * 4 * (uint)Settings.MaxSurfels);
            SDL_GpuSetBufferName(Device, SplatPortalBuffer, "SplatPortalBuffer"u8);
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
        if (Overlay != null)
        {
            Overlay.PropagateRelease();
        }

        if (Device != IntPtr.Zero)
        {
            MaybeReleaseBuffer(SplatPortalBuffer);
            MaybeReleaseBuffer(SplatColorBuffer);
            MaybeReleaseBuffer(SplatWorldPositionBuffer_L);
            MaybeReleaseBuffer(SplatWorldPositionBuffer_H);
            MaybeReleaseBuffer(SplatIndexBuffer);
            MaybeReleaseBuffer(SplatVertexBuffer);
            foreach (IntPtr Texture in ColorTexture)
            {
                MaybeReleaseTexture(Texture);
            }
            MaybeReleaseTexture(DepthTexture);
            MaybeReleaseReleaseGraphicsPipeline(SurfelPipeline);
            MaybeReleaseReleaseGraphicsPipeline(RevealPipeline);
            MaybeReleaseReleaseGraphicsPipeline(OverlayPipeline);
            MaybeReleaseReleaseGraphicsPipeline(StereoRevealPipeline);
            SDL_GpuUnclaimWindow(Device, Window);
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
        }
    }

    public bool Advance(FrameInfo Frame, RenderingConfig Settings, bool DebugClear, HighLevelRenderer HighRenderer)
    {
        UploadFixies(SplatWorldPositionBuffer_L, SplatWorldPositionBuffer_H, HighRenderer.PositionUpload, true);
        UploadVector3s(SplatColorBuffer, HighRenderer.ColorUpload, true);
        UploadVector4s(SplatPortalBuffer, HighRenderer.PortalUpload, true);

        IntPtr CommandBuffer = SDL_GpuAcquireCommandBuffer(Device);
        if (CommandBuffer == IntPtr.Zero)
        {
            Console.WriteLine("GpuAcquireCommandBuffer failed.");
            return true;
        }

        UInt32 Width = CurrentWidth;
        UInt32 Height = CurrentHeight;

        if (Overlay != null)
        {
            Overlay.Advance(Frame);
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
            ViewInfo.PupilOffset = 0.0f;
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
            Span<SDL_GpuBufferBinding> VertexBufferBindings = stackalloc SDL_GpuBufferBinding[5];

            for (int EyeIndex = 0; EyeIndex < ColorTexture.Count(); ++EyeIndex)
            {
                ViewInfo.PupilOffset = PupilOffsets[EyeIndex];
                IntPtr RenderTarget = ColorTexture[EyeIndex];

                SDL_GpuPushVertexUniformData(CommandBuffer, 0, &ViewInfo, (uint)sizeof(ViewInfoUpload));

                {
                    SDL_GpuColorAttachmentInfo ColorAttachmentInfo;
                    {
                        ColorAttachmentInfo.textureSlice.texture = RenderTarget;
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

                    {
                        VertexBufferBindings[0].buffer = SplatVertexBuffer;
                        VertexBufferBindings[0].offset = 0;
                        VertexBufferBindings[1].buffer = SplatWorldPositionBuffer_L;
                        VertexBufferBindings[1].offset = 0;
                        VertexBufferBindings[2].buffer = SplatWorldPositionBuffer_H;
                        VertexBufferBindings[2].offset = 0;
                        VertexBufferBindings[3].buffer = SplatColorBuffer;
                        VertexBufferBindings[3].offset = 0;
                        VertexBufferBindings[4].buffer = SplatPortalBuffer;
                        VertexBufferBindings[4].offset = 0;
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
                                SDL_GpuBindVertexBuffers(SurfelPass, 0, VertexBufferBindingsPtr, (uint)VertexBufferBindings.Length);
                            }
                            SDL_GpuBindIndexBuffer(
                                SurfelPass, &IndexBufferBinding, SDL_GpuIndexElementSize.SDL_GPU_INDEXELEMENTSIZE_16BIT);
                            SDL_GpuDrawIndexedPrimitives(SurfelPass, 0, 0, SplatMesh.TriangleCount * 3, HighRenderer.LiveSurfels);
                        }
                    }
                    SDL_GpuPopDebugGroup(CommandBuffer);
                    SDL_GpuEndRenderPass(SurfelPass);
                }
            }

            (IntPtr SwapchainTexture, CurrentWidth, CurrentHeight) = SDL_GpuAcquireSwapchainTexture(CommandBuffer, Window);
            while (SwapchainTexture == IntPtr.Zero)
            {
                Thread.Yield();
                (SwapchainTexture, CurrentWidth, CurrentHeight) = SDL_GpuAcquireSwapchainTexture(CommandBuffer, Window);
            }

            HighRenderer.FrameRate.LogFrame();

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
                    if (StereoRendering)
                    {
                        SDL_GpuBindGraphicsPipeline(RevealPass, StereoRevealPipeline);
                    }
                    else
                    {
                        SDL_GpuBindGraphicsPipeline(RevealPass, RevealPipeline);
                    }
                    SDL_GpuSetViewport(RevealPass, &Viewport);
                    SDL_GpuSetScissor(RevealPass, &ScissorRect);

                    int TextureCount = ColorTexture.Count();
                    Span<SDL_GpuTextureSlice> TextureBindings = stackalloc SDL_GpuTextureSlice[TextureCount];
                    for (int EyeIndex = 0; EyeIndex < TextureCount; ++EyeIndex)
                    {
                        TextureBindings[EyeIndex].texture = ColorTexture[EyeIndex];
                        TextureBindings[EyeIndex].mipLevel = 0;
                        TextureBindings[EyeIndex].layer = 0;
                    }

                    fixed (SDL_GpuTextureSlice* TextureBindingsPtr = TextureBindings)
                    {
                        SDL_GpuBindFragmentStorageTextures(RevealPass, 0, TextureBindingsPtr, (uint)TextureCount);
                    }
                    SDL_GpuDrawPrimitives(RevealPass, 0, 3);
                }
                SDL_GpuPopDebugGroup(CommandBuffer);
                SDL_GpuEndRenderPass(RevealPass);
            }

            if (Overlay != null)
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

                    Overlay.Draw(OverlayPass);
                }
                SDL_GpuPopDebugGroup(CommandBuffer);
                SDL_GpuEndRenderPass(OverlayPass);
            }
        }

        SDL_GpuSubmit(CommandBuffer);
        return false;
    }
}
