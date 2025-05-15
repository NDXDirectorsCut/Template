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

int GetCascade(float3 positionWS)
{
	int cascade = 0;
	for(int cascade=0; cascade<_CascadeCount; cascade++)
	{
		float4 sphere = _CascadeCullingSpheres[cascade];
		float distanceSqr = DistanceSquared(positionWS, sphere.xyz);
		if( distanceSqr < sphere.w)
		{
			break;
		}
	}
	return cascade;
}

DirectionalShadowData GetDirShdData (int id, float3 positionWS) {
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData[id].x;
	int cascade = GetCascade(positionWS);
	if(cascade == _CascadeCount)
		data.strength = 0;
	data.tileIndex = _DirectionalLightShadowData[id].y + GetCascade(positionWS);
	return data;
}


float GetDirShadow(int id,float3 positionWS)
{
	DirectionalShadowData dirShadow = GetDirShdData(id,positionWS);
    if(dirShadow.strength <= 0.0)
	{
		return 1;
	}
	float3 positionSTS = mul(
		_DirectionalShadowMatrices[dirShadow.tileIndex],
		float4(positionWS, 1.0)
	).xyz;
    float shadow = SAMPLE_TEXTURE2D(_DirectionalShadowAtlas,sampler_DirectionalShadowAtlas,positionSTS) < positionSTS.z;
	shadow = lerp(1.0, shadow, dirShadow.strength);
	return shadow;
}

#endif