#version 450

layout (location = 0) in vec2 VertexUV;
layout (location = 0) out vec4 OutColor;
layout (set = 2, binding = 0) uniform sampler2D Sampler;

void main()
{
    OutColor = texture(Sampler, VertexUV);
}
