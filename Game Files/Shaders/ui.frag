#version 330 core

in vec2 Uv;
in vec3 Normal;

out vec4 FragColor;
uniform sampler2D u_Color;

void main()
{
    vec4 texColor = texture(u_Color, Uv);
    if(texColor.a < 0.1)
        discard;
    FragColor = texColor;
}