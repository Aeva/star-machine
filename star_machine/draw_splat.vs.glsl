#version 450

layout(std140, set = 1, binding = 0)
uniform ViewInfoBlock
{
	mat4 LocalToWorld;
	mat4 WorldToView;
	mat4 ViewToClip;

	float SplatDiameter;
	float SplatDepth;
	float AspectRatio;
};

layout (location = 0) in vec3 LocalVertexOffset;
layout (location = 1) in vec3 SplatWorldPosition;
layout (location = 2) in vec3 SplatColor;

layout (location = 0) out vec3 VertexColor;

void main()
{
    vec4 ViewPosition = WorldToView * vec4(SplatWorldPosition, 1.0f);
    ViewPosition /= ViewPosition.w;
    ViewPosition.z += LocalVertexOffset.z * SplatDepth;

    vec4 ClipPosition = ViewToClip * ViewPosition;
    ClipPosition.xyz /= ClipPosition.w;

    ClipPosition.xy += LocalVertexOffset.xy * vec2(SplatDiameter * AspectRatio, SplatDiameter);
    ClipPosition.xyz *= ClipPosition.w;
    gl_Position = ClipPosition;

    VertexColor = SplatColor;
}
