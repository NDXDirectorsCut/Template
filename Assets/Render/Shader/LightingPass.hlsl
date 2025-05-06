#ifndef ECLIPSE_LIGHTING_INCLUDED
#define ECLIPSE_LIGHTING_INCLUDED

#include "EclipseCommon.hlsl"

#define MAX_DIRECTIONAL_LIGHT_COUNT 32

int _DirectionalLightCount;
float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
// Shader.PropertyToID("_DirectionalLightCount"),
//         dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
//         dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection"),

//         otherLightCountId = Shader.PropertyToID("_OtherLightCount"),
//         otherLightColorId = Shader.PropertyToID("_OtherLightColor"),
//         otherLightDirectionId = Shader.PropertyToID("_OtherLightPosition");

float3 GetDirLight(float3 normal, float3 lightDir)
{
    float3 dirLighting = dot(normal,lightDir);
    return dirLighting;
}

float3 GetLighting(float3 normal)
{
    // float3 dir = float3(0,1,0);
    // float3 lighting = dot(normal,dir);
    float3 lighting = 0;
    for(int i=0; i< _DirectionalLightCount; i++)
    {
        float3 dir = _DirectionalLightDirections[i];
        lighting += GetDirLight(normal,dir);
    }

    return lighting; 
}

#endif