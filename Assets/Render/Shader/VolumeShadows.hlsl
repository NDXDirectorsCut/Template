#ifndef ECLIPSE_VOLUMESHADOWS_INCLUDED
#define ECLIPSE_VOLUMESHADOWS_INCLUDED

#include "EclipseCommon.hlsl"

TEXTURE2D(_DirectionalShadowAtlas);
SAMPLER(sampler_DirectionalShadowAtlas);

TEXTURE2D(_OtherShadowAtlas);
SAMPLER(sampler_OtherShadowAtlas);

struct DirectionalShadowData 
{
	float strength;
	int tileIndex;
};

struct OtherShadowData
{
	float strength;
	int tileIndex;
	bool isSpot;
	bool lightDir;
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

DirectionalShadowData GetDirShdData (int id, float3 positionWS)\
{
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData[id].x;
	int cascade = GetCascade(positionWS);
	if(cascade == _CascadeCount)
		data.strength = 0;
	data.tileIndex = _DirectionalLightShadowData[id].y + GetCascade(positionWS);
	return data;
}

OtherShadowData GetOthShdData(int id)
{
	OtherShadowData data;
	data.strength = _OtherLightShadowData[id].x;
	data.tileIndex = _OtherLightShadowData[id].y;
	data.isSpot = _OtherLightShadowData[id].z == 1.0;
	return data;
}

float GetDirShadow(int id,float3 positionWS)
{
	DirectionalShadowData dirShadow = GetDirShdData(id,positionWS);
    if(dirShadow.strength <= 0.0)
	{
		return 1;
	}
	float3 test = float3(-1,0,0);
	float4x4 mat = _DirectionalShadowMatrices[dirShadow.tileIndex];
	//mat = Move4x4(mat,test);

	float3 positionSTS = mul(
		mat,
		float4(positionWS+test, 1.0)).xyz;
	
    float shadow = SAMPLE_TEXTURE2D(_DirectionalShadowAtlas,sampler_DirectionalShadowAtlas,positionSTS);// < positionSTS.z;
	shadow = lerp(1.0, shadow, dirShadow.strength);
	return shadow;//-positionSTS.z;
}

float GetOthShadow(int id,float3 positionWS, float3 lightDir)
{
	OtherShadowData othShadow = GetOthShdData(id);
	if(othShadow.strength <= 0.0)
	{
		return 1;
	}

	float tileIndex = othShadow.tileIndex;

	if (othShadow.isSpot) 
	{
		tileIndex = othShadow.tileIndex;
	}
	else
	{
		float faceOffset = CubeMapFaceID(-lightDir);
		tileIndex += faceOffset;
	}

	float4 positionSTS = mul(
		_OtherShadowMatrices[tileIndex],
		float4(positionWS, 1.0));
	float3 coord = positionSTS.xyz / positionSTS.w;
	float shadow = SAMPLE_TEXTURE2D(_OtherShadowAtlas,sampler_OtherShadowAtlas,coord);//< coord.z;
	shadow = lerp(1.0, shadow, othShadow.strength);
	return shadow;
}

#endif