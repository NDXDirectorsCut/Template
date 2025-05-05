#ifndef ECLIPSE_SIMPLE_INCLUDED
#define ECLIPSE_SIMPLE_INCLUDED

#include "EclipseCommon.hlsl"

TEXTURE2D(_AlbedoMap);
TEXTURE2D(_AlbedoMap_ST);
SAMPLER(sampler_AlbedoMap);
float4 _Tint;

float4x4 unity_MatrixVP;
float4x4 unity_ObjectToWorld;

struct Attributes
{
    float4 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
};

Varyings vert (Attributes IN)
{
    Varyings OUT;
    float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
    OUT.positionCS = mul(unity_MatrixVP, worldPos);
    OUT.baseUV = IN.baseUV;
    return OUT;
}

float4 frag (Varyings IN) : SV_TARGET
{
    float4 albedoMap = SAMPLE_TEXTURE2D(_AlbedoMap, sampler_AlbedoMap, IN.baseUV );
    float4 surfaceColor = albedoMap * _Tint;
    return surfaceColor;
}

#endif