#version 450

layout (location = 0) out vec4 OutColor;

layout (set = 2, binding = 0, rgba8) uniform readonly image2D LeftSplatImage;
layout (set = 2, binding = 1, rgba8) uniform readonly image2D RightSplatImage;

vec3 GrayScale(vec3 Color)
{
    float Gray = (Color.r + Color.b + Color.g) / 3.0f;
    return vec3(Gray, Gray, Gray);
}

void main()
{
    vec3 LeftColor = imageLoad(LeftSplatImage, ivec2(gl_FragCoord.xy)).rgb;
    vec3 LeftGray = GrayScale(LeftColor);

    vec3 RightColor = imageLoad(RightSplatImage, ivec2(gl_FragCoord.xy)).rgb;
    vec3 RightGray = GrayScale(RightColor);

    vec3 Color;
    Color.r = mix(LeftGray.r, LeftColor.r, 0.125f);
    Color.gb = mix(RightGray.gb, RightColor.gb, 0.125f);
    OutColor = vec4(Color, 1.0f);
}
