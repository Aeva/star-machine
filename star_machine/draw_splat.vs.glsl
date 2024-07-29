#version 450

// TODO: Emulate int64 when it is unavailable?  Or just do it anyway to avoid the validation error?
#extension GL_ARB_gpu_shader_int64 : require

layout(std140, set = 1, binding = 0)
uniform ViewInfoBlock
{
    mat4 WorldToView;
    mat4 ViewToClip;

    uvec3 EyeInnerWorldPosition;
    uvec3 EyeOuterWorldPosition;

    float SplatDiameter;
    float SplatDepth;
    float AspectRatio;
};

layout (location = 0) in vec3 LocalVertexOffset;
layout (location = 1) in vec3 SplatInnerWorldPosition;
layout (location = 2) in vec3 SplatOuterWorldPosition;
layout (location = 3) in vec3 SplatColor;

layout (location = 0) out vec3 VertexColor;

#if 0
vec3 DecodeFixie(uvec3 Inner, uvec3 Outer)
{
    const int UnitOffset = 16;
    const int WordOffset = 32;
    const float InnerMultiplier = 1.0 / float(1 << UnitOffset);
    const float OutterMultiplier = float(1 << (WordOffset - UnitOffset));
    const uint SignMask = (1 << 31);
    vec3 Sign = vec3(1.0, 1.0, 1.0);
    for (int Channel = 0; Channel < 3; ++Channel)
    {
        if ((Outer[Channel] & SignMask) == SignMask)
        {
            Sign[Channel] = -1.0;
            Inner[Channel] = 0xFFFFFFFF ^ (Inner[Channel] - 1);

            if (Inner[Channel] == 0)
            {
                Outer[Channel] -= 1;
            }

            Outer[Channel] = 0xFFFFFFFF ^ Outer[Channel];
        }
    }
    return (vec3(Inner) * InnerMultiplier + vec3(Outer) * OutterMultiplier) * Sign;
}
#endif

vec3 DecodeFixie(i64vec3 Fixie)
{
    const int UnitOffset = 16;
    const int64_t UnitValue = 1 << UnitOffset;
    const int64_t DecimalMask = UnitValue - 1;
    const float UnitReciprocal = 1.0 / float(UnitValue);

    vec3 Result;
    for (int Channel = 0; Channel < 3; ++Channel)
    {
        float Whole = float(Fixie[Channel] >> UnitOffset);
        float Fract = float(Fixie[Channel] & DecimalMask) * UnitReciprocal;
        Result[Channel] = Whole + Fract;
    }
    return Result;
}

i64vec3 Unpack(uvec3 Inner, uvec3 Outer)
{
    i64vec3 Result;
    for (int Channel = 0; Channel < 3; ++Channel)
    {
        Result[Channel] = int64_t(uint64_t(Inner[Channel]) | (uint64_t(Outer[Channel]) << 32));
    }
    return Result;
}

void main()
{
    i64vec3 SplatWorldPosition = Unpack(floatBitsToUint(SplatInnerWorldPosition), floatBitsToUint(SplatOuterWorldPosition));
    i64vec3 EyeWorldPosition = Unpack(EyeInnerWorldPosition, EyeOuterWorldPosition);

    vec3 SplatViewPosition = DecodeFixie(SplatWorldPosition - EyeWorldPosition);

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
