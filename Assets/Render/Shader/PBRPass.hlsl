#ifndef ECLIPSE_PBR_INCLUDED
#define ECLIPSE_PBR_INCLUDED

#include "EclipseCommon.hlsl"

float3 viewDirWS;
float roughnessFS;
float3 normalFS;

float GetSpecular(int id)
{
    float3 lightPos = _OtherLightPositions[id];
    float3 color = _OtherLightColors[id];

    float3 dist = lightPos-positionWS;
    float3 lightDir = normalize(dist);

    float3 h = SafeNormalize(lightDir + viewDirWS);
	float nh2 = Square(saturate(dot(normalWS, h)));
	float lh2 = Square(saturate(dot(lightDir, h)));
	float r2 = Square(roughnessFS);
	float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
	float normalization = roughnessFS * 4.0 + 2.0;
	return r2 / (d2 * max(0.1, lh2) * normalization);
}


float3 GetPBR(float3 viewDir, float3 normal, float3 albedo, float specularity, float metalness, float roughness)
{
    float3 specular;
    float invMetal = clamp(1 - metalness,0.04,1);
    roughnessFS = roughness;
    viewDirWS = viewDir;
    normalWS = normal;
    
    for(int k=0; k< _OtherLightCount; k++)
    {
        specular = GetSpecular(k);
    }
    return specular;
}

#endif