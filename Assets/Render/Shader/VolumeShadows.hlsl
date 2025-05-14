#ifndef ECLIPSE_VOLUMESHADOWS_INCLUDED
#define ECLIPSE_VOLUMESHADOWS_INCLUDED

#include "EclipseCommon.hlsl"

TEXTURE2D(_DirectionalShadowAtlas);
SAMPLER(sampler_DirectionalShadowAtlas);

struct DirectionalShadowData 
{
	float strength;
	int tileIndex;
};

DirectionalShadowData GetDirShdData (int id) {
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData[id].x;
	data.tileIndex = _DirectionalLightShadowData[id].y;
	return data;
}


float GetDirShadow(DirectionalShadowData dirShadow,float3 positionWS)
{
    float3 positionSTS = mul(
		_DirectionalShadowMatrices[dirShadow.tileIndex],
		float4(positionWS, 1.0)
	).xyz;
    float shadow = SAMPLE_TEXTURE2D(_DirectionalShadowAtlas,sampler_DirectionalShadowAtlas,positionSTS) < positionSTS.z;
    return shadow;
}

#endif