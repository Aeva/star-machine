#version 450

layout (location = 0) in vec4 InVertex;
layout (location = 0) out vec2 OutVertexUV;

void main()
{
    OutVertexUV = InVertex.xy;
    gl_Position = vec4(InVertex.zw, 0.0f, 1.0f);
}
