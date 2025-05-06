#ifndef ECLIPSE_BASIC_INCLUDED
#define ECLIPSE_BASIC_INCLUDED

#include "EclipseCommon.hlsl"

float3 GetDiffuse(float4 albedo, float3 tint, float3 emission)
{
    return albedo.xyz * tint + emission.xyz;
}

#endif