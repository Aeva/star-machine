#version 450

#extension GL_ARB_shading_language_include : require
#include "Fixie.glsl"


layout(std140, set = 1, binding = 0)
uniform ViewInfoBlock
{
    mat4 WorldToView;
    mat4 ViewToClip;

    uvec3 EyeWorldPosition_L;
    uvec3 EyeWorldPosition_H;

    float SplatDiameter;
    float SplatDepth;
    float AspectRatio;
};

layout (location = 0) in vec3 LocalVertexOffset;
layout (location = 1) in vec3 SplatWorldPosition_L;
layout (location = 2) in vec3 SplatWorldPosition_H;
layout (location = 3) in vec3 SplatColor;

layout (location = 0) out vec3 VertexColor;


void main()
{
    Fixie SplatWorldPosition = Unpack(floatBitsToUint(SplatWorldPosition_L), floatBitsToUint(SplatWorldPosition_H));
    Fixie EyeWorldPosition = Unpack(EyeWorldPosition_L, EyeWorldPosition_H);
    vec3 SplatViewPosition = FixedPointToFloat(Sub(SplatWorldPosition, EyeWorldPosition));

    vec4 ViewPosition = WorldToView * vec4(SplatViewPosition, 1.0f);
    ViewPosition /= ViewPosition.w;
    ViewPosition.z += LocalVertexOffset.z * SplatDepth;

    vec4 ClipPosition = ViewToClip * ViewPosition;
    ClipPosition.xyz /= ClipPosition.w;

    ClipPosition.xy += LocalVertexOffset.xy * vec2(SplatDiameter * AspectRatio, SplatDiameter);
    ClipPosition.xyz *= ClipPosition.w;
    gl_Position = ClipPosition;

    VertexColor = SplatColor;
}
