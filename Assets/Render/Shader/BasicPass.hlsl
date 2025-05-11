#ifndef ECLIPSE_BASIC_INCLUDED
#define ECLIPSE_BASIC_INCLUDED

#include "EclipseCommon.hlsl"

SurfaceData GetSurface(
    float4 albedo,
    float3 tint,
    float alpha,
    float3 normal,
    float emission,
    float3 emissionTint,
    float specularity,
    float3 specularityTint,
    float metalness,
    float roughness,

    float3 viewDir,
    float3 position
    )
{
    SurfaceData surface;
    surface.diffuse = albedo.xyz * tint;
    surface.alpha = albedo.w * alpha;
    surface.normal = normalize(normal);
    surface.emission = emission * emissionTint;
    surface.specularity = specularity * specularityTint;
    surface.metalness = metalness;
    surface.roughness = roughness;

    surface.viewDir = viewDir;
    surface.position = position;

    return surface;
}

#endif