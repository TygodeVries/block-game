#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aUv;
layout(location = 2) in vec3 aNormal;

uniform mat4 u_Model;
uniform mat4 u_View;
uniform mat4 u_Projection;
uniform float u_Time;

out vec3 Normal;
out vec2 Uv;
out vec3 FragPos;

void main()
{
    vec4 worldPos = u_Model * vec4(aPosition, 1.0);

    mat3 normalMatrix = transpose(inverse(mat3(u_Model)));
    vec3 worldNormal = normalMatrix * aNormal;

    Uv = aUv;
    Normal = worldNormal;
    FragPos = worldPos.xyz; 

    gl_Position = (u_Projection * u_View * worldPos);
}