#ifndef ECLIPSE_PBR_INCLUDED
#define ECLIPSE_PBR_INCLUDED

#include "EclipseCommon.hlsl"
#include "LightingPass.hlsl"

TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);

float3 GetLightSpecular(SurfaceData surface, float3 lightDir)
{
    float roughness = surface.roughness;//PerceptualRoughnessToRoughness(surface.roughness);

    float3 h = SafeNormalize(lightDir + surface.viewDir);
	float nh2 = Square(saturate(dot(surface.normal, h)));
	float lh2 = Square(saturate(dot(lightDir, h)));
	float r2 = Square(clamp(roughness,0.002,1));
	float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
	float normalization = roughness * 4.0 + 2.0;
    float dirDot = saturate(dot(surface.normal,lightDir));

    float3 brdfSpecular = lerp(0.04, surface.diffuse, surface.metalness);
    float3 result = r2 * dirDot / (d2 * max(0.1, lh2) * normalization);
    result *= brdfSpecular;
	return result;
}

float3 GetPointSpecular(SurfaceData surface, int id)
{
    float3 lightPos = _OtherLightPositions[id];
    float3 color = _OtherLightColors[id];

    float3 dist = lightPos-surface.position;
    float3 lightDir = normalize(dist);
    float distanceSqr = max(dot(dist, dist), 0.00001);
    
    float rangeAttenuation = Square(
		saturate(1.0 - Square(distanceSqr * _OtherLightPositions[id].w))
	);


    return GetLightSpecular(surface,lightDir) * color * rangeAttenuation;
}

float3 GetDirSpecular(SurfaceData surface, int id)
{
    float3 lightDir = _DirectionalLightDirections[id];
    float3 color = _DirectionalLightColors[id];

    return GetLightSpecular(surface,lightDir) * color;
}


float3 GetSpecular(SurfaceData surface)
{
    float3 specularReflection = 0;
    
    
    for(int k=0; k< _OtherLightCount; k++)
    {
        specularReflection += GetPointSpecular(surface,k);
    }
    for(int k2=0; k2< _DirectionalLightCount; k2++)
    {
        specularReflection += GetDirSpecular(surface,k2);
    }

    //specularReflection = lerp(specularReflection, specularReflection * surface.diffuse, surface.metalness);
    specularReflection *= surface.specularity;
    

    return specularReflection;
}

float3 GetMetalness(SurfaceData surface)
{
    float invMetal = clamp(1 - surface.metalness,0.04,1);
    float3 metallicColor = surface.diffuse * invMetal;
    return metallicColor;
}

float3 GetSpecularReflection(SurfaceData surface, float3 viewDir = (0,0,0))
{
    float roughness = PerceptualRoughnessToRoughness(surface.roughness);
    float roughnessB = PerceptualRoughnessToMipmapLevel(clamp(surface.roughness,0,1));

    if(length(viewDir) == 0)
        viewDir = surface.viewDir;
    float3 reflectionDir = reflect(-viewDir,surface.normal);
    float3 reflection = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectionDir, roughnessB);
    float3 brdfSpecular = lerp(0.04, surface.diffuse, surface.metalness);

    float fresnel = Pow4(1.0 - saturate(dot(surface.normal, surface.viewDir)));
    float fresnelStrength = saturate((1.0 -surface.roughness) + 1.0 - clamp(1 - surface.metalness,0.04,1));

    //brdfSpecular = lerp(brdfSpecular, fresnelStrength,fresnel);

    reflection *= brdfSpecular * surface.specularity;
    reflection /= surface.roughness * surface.roughness + 1;
    return reflection;
}

float3 GetReflection(float3 normal, float3 viewDir,float roughness) //Debug
{
    float3 dir = reflect(-viewDir,normal);
    float3 reflection = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, dir,roughness*8);
    return reflection;
}

#endif