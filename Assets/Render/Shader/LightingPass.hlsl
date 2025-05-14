#ifndef ECLIPSE_LIGHTING_INCLUDED
#define ECLIPSE_LIGHTING_INCLUDED

#include "EclipseCommon.hlsl"

float3 normalWS, positionWS;

float3 GetDirLight(int id)
{
    float3 lightDir = _DirectionalLightDirections[id];
    float3 color = _DirectionalLightColors[id];

    DirectionalShadowData dirShdData = GetDirShdData(id);
    float shadowAttenuation = GetDirShadow(dirShdData,positionWS);

    float3 dirLighting = saturate(dot(normalWS,lightDir));
    dirLighting *= shadowAttenuation;
    dirLighting = smoothstep(0,1,dirLighting);
    dirLighting *= color;
    return dirLighting;
}

float3 GetPointLight(int id)
{
    float3 lightPos = _OtherLightPositions[id];
    float3 color = _OtherLightColors[id];

    float3 dist = lightPos-positionWS;
    float3 lightDir = normalize(dist);

    float distanceSqr = max(dot(dist, dist), 0.00001);

    float rangeAttenuation = Square(
		saturate(1.0 - Square(distanceSqr * _OtherLightPositions[id].w))
	);

    float3 pointLighting = saturate(dot(normalWS,lightDir)) * (rangeAttenuation/distanceSqr);
    pointLighting = smoothstep(0,1,pointLighting);
    pointLighting = pointLighting * color;
    return pointLighting;
}

float3 GetLighting(SurfaceData surface)
{
    normalWS = surface.normal;
    positionWS = surface.position;

    float3 lighting = float3(0,0,0);
    
    float3 dirLighting = float3(0,0,0);
    
    for(int i=0; i< _DirectionalLightCount; i++)
    {
        float3 dirLight = GetDirLight(i);
        dirLighting += dirLight;
    }

    float3 pointLighting = float3(0,0,0);
    for(int j=0; j< _OtherLightCount; j++)
    {
        float3 light = GetPointLight(j);
        pointLighting += light;
    }
    lighting = dirLighting + pointLighting;

    return lighting; 
}

#endif