﻿
using System;
using System.IO;
using System.Threading;
using SDL3;
using static SDL3.SDL;


namespace StarMachine
{
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
            try
            {
                ShaderBlob = System.IO.File.ReadAllBytes(Path);
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine($"File not found: {Path}");
                return IntPtr.Zero;
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

        static void Main(string[] args)
        {
            if (SDL_Init(SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("SDL3 initialization failed.");
                return;
            }

            var Window = SDL_CreateWindow("Star Machine"u8, 600, 600, 0);
            if (Window == IntPtr.Zero)
            {
                Console.WriteLine("SDL3 failed to create a window.");
                return;
            }

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
                var VertexShader = LoadShader(Device, "RawTriangle.vert.spv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_VERTEX, 0, 0, 0, 0);
                if (VertexShader == IntPtr.Zero)
                {
                    SDL_GpuUnclaimWindow(Device, Window);
                    SDL_GpuDestroyDevice(Device);
                    SDL_DestroyWindow(Window);
                    Console.WriteLine("Failed to create vertex shader.");
                    return;
                }

                var FragmentShader = LoadShader(Device, "SolidColor.frag.spv", SDL_GpuShaderStage.SDL_GPU_SHADERSTAGE_VERTEX, 0, 0, 0, 0);
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

                    var ColorDesc = new SDL_GpuColorAttachmentDescription[1];
                    ColorDesc[0].format = SDL_GpuGetSwapchainTextureFormat(Device, Window);
                    ColorDesc[0].blendState = BlendState;

                    fixed (SDL_GpuColorAttachmentDescription* ColorDescPtr = ColorDesc)
                    {
                        SDL_GpuGraphicsPipelineAttachmentInfo AttachmentInfo = new();
                        AttachmentInfo.colorAttachmentCount = (uint)ColorDesc.Length;
                        AttachmentInfo.colorAttachmentDescriptions = ColorDescPtr;

                        SDL_GpuGraphicsPipelineCreateInfo PipelineCreateInfo;
                        PipelineCreateInfo.attachmentInfo = AttachmentInfo;

                        PipelineCreateInfo.multisampleState.sampleMask = 0xFFFF;
                        PipelineCreateInfo.primitiveType = SDL_GpuPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST;
                        PipelineCreateInfo.vertexShader = VertexShader;
                        PipelineCreateInfo.fragmentShader = FragmentShader;
                        PipelineCreateInfo.rasterizerState.fillMode = SDL_GpuFillMode.SDL_GPU_FILLMODE_FILL;

                        SimplePipeline = SDL_GpuCreateGraphicsPipeline(Device, &PipelineCreateInfo);
                    }
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
                    ColorAttachmentInfo.textureSlice.texture = BackBuffer;
                    ColorAttachmentInfo.clearColor.r = 0.2f;
                    ColorAttachmentInfo.clearColor.g = 0.2f;
                    ColorAttachmentInfo.clearColor.b = 0.2f;
                    ColorAttachmentInfo.clearColor.a = 1.0f;
                    ColorAttachmentInfo.loadOp = SDL.SDL_GpuLoadOp.SDL_GPU_LOADOP_CLEAR;
                    ColorAttachmentInfo.storeOp = SDL.SDL_GpuStoreOp.SDL_GPU_STOREOP_STORE;


                    unsafe
                    {
                        IntPtr RenderPass = SDL_GpuBeginRenderPass(CommandBuffer, &ColorAttachmentInfo, 1, null);
                        SDL_GpuBindGraphicsPipeline(RenderPass, SimplePipeline);
                        SDL_GpuDrawPrimitives(RenderPass, 0, 1);
                        SDL_GpuEndRenderPass(RenderPass);
                    }
                }
                SDL_GpuSubmit(CommandBuffer);
            }


            SDL_GpuReleaseGraphicsPipeline(Device, SimplePipeline);
            SDL_GpuUnclaimWindow(Device, Window);
            SDL_GpuDestroyDevice(Device);
            SDL_DestroyWindow(Window);
        }
    }
}
