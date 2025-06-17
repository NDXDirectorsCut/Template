#ifndef ECLIPSE_LIGHTING_INCLUDED
#define ECLIPSE_LIGHTING_INCLUDED

#include "EclipseCommon.hlsl"

float3 normalWS, positionWS;

float3 GetDirLight(int id, SurfaceData surface)
{
    float3 lightDir = _DirectionalLightDirections[id];
    float3 color = _DirectionalLightColors[id];

    //DirectionalShadowData dirShdData = GetDirShdData(id);
    float3 shadowAttenuation = GetDirShadow(id,surface);

    float3 dirLighting = saturate(dot(surface.normal,lightDir));
    dirLighting *= shadowAttenuation;
    //dirLighting = smoothstep(0,1,dirLighting);
    dirLighting *= color;
    return shadowAttenuation;
}

float3 GetOtherLight(int id,SurfaceData surface)
{
    float3 lightPos = _OtherLightPositions[id];
    float3 color = _OtherLightColors[id];

    float3 dist = lightPos-surface.position;
    float3 lightDir = normalize(dist);

    float distanceSqr = max(dot(dist, dist), 0.00001);

    float3 shadowAttenuation = GetOthShadow(id,surface,lightDir);

    float rangeAttenuation = Square(
		saturate(1.0 - Square(distanceSqr * _OtherLightPositions[id].w))
	);

    float4 spotAngles = _OtherLightSpotAngles[id];
    float spotAttenuation = Square(
		saturate(dot(_OtherLightDirections[id].xyz, lightDir) *
		spotAngles.x + spotAngles.y)
	);

    float3 pointLighting = rangeAttenuation * saturate(dot(surface.normal,lightDir)) * spotAttenuation * shadowAttenuation;
    //pointLighting = smoothstep(0,1,pointLighting);
    pointLighting /= distanceSqr;
    
    pointLighting = pointLighting * color;
    return shadowAttenuation;
}

float3 GetLighting(SurfaceData surface)
{
    normalWS = surface.normal;
    positionWS = surface.position;

    float3 lighting = float3(0,0,0);
    
    float3 dirLighting = float3(0,0,0);
    
    for(int i=0; i< _DirectionalLightCount; i++)
    {
        float3 dirLight = GetDirLight(i,surface);
        dirLighting += dirLight;
    }

    float3 pointLighting = float3(0,0,0);
    for(int j=0; j< _OtherLightCount; j++)
    {
        float3 light = GetOtherLight(j,surface);
        pointLighting += light;
    }
    lighting = dirLighting + pointLighting;

    return lighting; 
}

#endif