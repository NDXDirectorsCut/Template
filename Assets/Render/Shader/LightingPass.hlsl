#ifndef ECLIPSE_LIGHTING_INCLUDED
#define ECLIPSE_LIGHTING_INCLUDED

#include "EclipseCommon.hlsl"

float3 GetLighting(float3 normal)
{
    float3 dir = float3(0,1,0);
    float3 lighting = dot(normal,dir);

    return lighting; 
}

#endif