#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aUv;

uniform sampler2D u_Color;

out vec2 Uv;

void main()
{
    Uv = aUv;
    gl_Position = vec4(aPosition, 1.0);
}