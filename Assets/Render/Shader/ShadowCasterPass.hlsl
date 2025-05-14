#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED

#include "EclipseCommon.hlsl"  

TEXTURE2D(_BaseMap);
UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST);    
SAMPLER(sampler_BaseMap);

struct Attributes {
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
};

struct Varyings {
	float4 positionCS : SV_POSITION;
	float2 baseUV : VAR_BASE_UV;
};

Varyings ShadowCasterPassVertex (Attributes IN)
{
    Varyings OUT;
    float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
    OUT.positionCS = mul(unity_MatrixVP, worldPos);
    OUT.baseUV = IN.baseUV;

    return OUT;
}

void ShadowCasterPassFragment (Varyings IN)
{
	float4 albedo = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap, IN.baseUV);
}

#endif