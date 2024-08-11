#version 450

layout (location = 0) out vec4 OutColor;

layout (set = 2, binding = 0, rgba8) uniform readonly image2D SplatImage;

void main()
{
    vec4 Color = imageLoad(SplatImage, ivec2(gl_FragCoord.xy));
    OutColor = vec4(Color.xyz, 1.0f);
}
