#version 450

#extension GL_GOOGLE_include_directive : enable
#if !GL_GOOGLE_include_directive
#extension GL_ARB_shading_language_include : require
#endif


#include "Fixie.glsl"


layout(std140, set = 1, binding = 0)
uniform ViewInfoBlock
{
    mat4 WorldToView;
    mat4 ViewToClip;

    vec4 MovementProjection;

    uvec3 EyeWorldPosition_L;
    uvec3 EyeWorldPosition_H;

    float SplatDiameter;
    float SplatDepth;
    float AspectRatio;

    float PupilOffset;
};

layout (location = 0) in vec3 LocalVertexOffset;
layout (location = 1) in vec3 SplatWorldPosition_L;
layout (location = 2) in vec3 SplatWorldPosition_H;
layout (location = 3) in vec3 SplatColor;
layout (location = 4) in vec4 SplatPortal; // plane defined as a normal and shortest planar distance to the reflection

layout (location = 0) out vec3 VertexColor;


void main()
{
    Fixie SplatWorldPosition = Unpack(floatBitsToUint(SplatWorldPosition_L), floatBitsToUint(SplatWorldPosition_H));
    Fixie EyeWorldPosition = Unpack(EyeWorldPosition_L, EyeWorldPosition_H);
    vec3 SplatViewPosition = FixedPointToFloat(Sub(SplatWorldPosition, EyeWorldPosition));

    if (SplatPortal.w != 0.0)
    {
        // Because the view transform has not yet been applied, SplatViewPosition is
        // currently the world space offset from the view origin.
        vec3 EyeRay = normalize(-SplatViewPosition);

        // This is the view relative offset of the closest point on the reflection plane to the splat.
        vec3 PlaneOrigin = SplatPortal.xyz * -SplatPortal.w + SplatViewPosition;

        float Numerator = dot(PlaneOrigin /* - RayStart*/, SplatPortal.xyz);
        float Denominator = dot(EyeRay, SplatPortal.xyz);
        float Travel = Numerator / Denominator;

        // Amount to extend in front of the floor to prevent z fighting with mirror surface splats.
        float Push = (1.0 - dot(EyeRay, SplatPortal.xyz)) * 0.5 + 0.1;

        SplatViewPosition = EyeRay * (Travel + Push);
    }

    vec4 ViewPosition = WorldToView * vec4(SplatViewPosition, 1.0f);
    ViewPosition /= ViewPosition.w;
    ViewPosition.x += PupilOffset;
    ViewPosition.z += LocalVertexOffset.z * SplatDepth;

    float WarpTail = min(MovementProjection.w, 1000.0f);
    vec4 WarpOffset = vec4(ViewPosition.xyz + (MovementProjection.xyz * WarpTail), 1.0f);

    vec4 WarpClip = ViewToClip * WarpOffset;

    vec4 ClipPosition = ViewToClip * ViewPosition;

    vec2 WarpDir = (WarpClip.xy / WarpClip.w) - (ClipPosition.xy / ClipPosition.w);
    float WarpMag = dot(WarpDir, WarpDir);
    if (WarpMag > 0.0)
    {
        WarpDir /= sqrt(WarpMag);

        ClipPosition.xyzw = mix(ClipPosition.xyzw, WarpClip.xyzw, min(dot(LocalVertexOffset.xy, WarpDir), 0.0f));
    }
    ClipPosition.xyz /= ClipPosition.w;

    ClipPosition.xy += LocalVertexOffset.xy * vec2(SplatDiameter * AspectRatio, SplatDiameter);
    ClipPosition.xyz *= ClipPosition.w;
    gl_Position = ClipPosition;

    VertexColor = SplatColor;
}
