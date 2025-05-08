#ifndef ECLIPSE_PBR_INCLUDED
#define ECLIPSE_PBR_INCLUDED

#include "EclipseCommon.hlsl"
#include "LightingPass.hlsl"

float3 GetSpecular(SurfaceData surface, float3 lightDir)
{
    float3 h = SafeNormalize(lightDir + surface.viewDir);
	float nh2 = Square(saturate(dot(surface.normal, h)));
	float lh2 = Square(saturate(dot(lightDir, h)));
	float r2 = Square(clamp(surface.roughness,0.002,1));
	float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
	float normalization = surface.roughness * 4.0 + 2.0;
    float dirDot = saturate(dot(surface.normal,lightDir));

    float3 result = r2 * dirDot / (d2 * max(0.1, lh2) * normalization);

	return result;
}

float3 GetPointSpecular(SurfaceData surface, int id)
{
    float3 lightPos = _OtherLightPositions[id];
    float3 color = _OtherLightColors[id];

    float3 dist = lightPos-surface.position;
    float3 lightDir = normalize(dist);

    return GetSpecular(surface,lightDir) * color;
}

float3 GetDirSpecular(SurfaceData surface, int id)
{
    float3 lightDir = _DirectionalLightDirections[id];
    float3 color = _DirectionalLightColors[id];

    return GetSpecular(surface,lightDir) * color;
}


float3 GetPBR(SurfaceData surface)
{
    float3 specularReflection = 0;
    float invMetal = clamp(1 - surface.metalness,0.04,1);
    
    for(int k=0; k< _OtherLightCount; k++)
    {
        specularReflection += GetPointSpecular(surface,k);
    }
    for(int k2=0; k2< _DirectionalLightCount; k2++)
    {
        specularReflection += GetDirSpecular(surface,k2);
    }

    specularReflection = lerp(specularReflection, specularReflection * surface.diffuse, surface.metalness);
    specularReflection *= surface.specularity;
    

    return specularReflection;
}

#endif