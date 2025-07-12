#ifndef ECLIPSE_PBREXTRA_INCLUDED
#define ECLIPSE_PBREXTRA_INCLUDED

#include "EclipseCommon.hlsl"
#include "LightingPass.hlsl"
#include "AmbientCubePass.hlsl"

float sheenPow;

float3 GetOtherSheen(int id, SurfaceData surface)
{
    float3 lightPos = _OtherLightPositions[id];
    float3 color = _OtherLightColors[id];

    float3 dist = lightPos-surface.position;
    float3 lightDir = normalize(dist);

    float light = GetOtherLight(id,surface);
    
    float fresnel = pow(1-saturate(dot(surface.normal, surface.viewDir)),sheenPow);// * saturate(dot(surface.normal,lightDir));
    fresnel = clamp(fresnel,0,1);
    float3 final = fresnel*light*color;
    return final;
} 

float3 GetDirSheen(int id, SurfaceData surface)
{
    float3 light = GetDirLight(id,surface);
    float3 color = _DirectionalLightColors[id];
    float3 fresnel = pow(1-saturate(dot(surface.normal, surface.viewDir)),sheenPow);
    float3 final = fresnel*light*color;

    return final;
}

float3 GetAmbientSheen(SurfaceData surface)
{
    float fresnel = Pow4(1-saturate(dot(surface.normal, surface.viewDir)));
    float3 ambientLight = GetAmbientLight(surface);
    fresnel *= clamp(ambientLight,0,1);

    return fresnel;
}

float3 GetSheen(SurfaceData surface,float power)
{
    float3 sheen = 0;
    
    sheenPow = power;

    for(int she=0; she< _OtherLightCount; she++)
    {
        sheen += GetOtherSheen(she, surface);
    }
    for(int she2=0; she2< _DirectionalLightCount; she2++)
    {
        sheen += GetDirSheen(she2, surface);
    }

    //sheen += GetAmbientSheen(surface);

    sheen *= surface.sheen;
    return sheen;
}

#endif