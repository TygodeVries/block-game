#version 330 core

in vec2 Uv;
in vec3 Normal;
in vec3 FragPos;

out vec4 FragColor;

uniform sampler2D u_Color;
uniform vec3 u_CameraPos;

uniform vec3 u_FogColor = vec3(0.6, 0.7, 0.8);
uniform float u_FogNear = 0;
uniform float u_FogFar = 0;

in float AO;

vec3 u_LightDir = normalize(vec3(0.5, 1.0, 0.3));

void main()
{
    vec3 norm = normalize(Normal);
    float light = max(dot(norm, normalize(u_LightDir)), 0.5);

    vec4 texColor = texture(u_Color, Uv);

    if(texColor.a < 0.1)
        discard;

    vec3 baseColor = texColor.rgb * light;

    float dist = length(u_CameraPos - FragPos);

    float fogFactor = clamp((u_FogFar - dist) / (u_FogFar - u_FogNear), 0.0, 1.0);

    vec3 finalColor = mix(u_FogColor, baseColor, fogFactor) * AO;

    FragColor = vec4(finalColor, texColor.a);
}