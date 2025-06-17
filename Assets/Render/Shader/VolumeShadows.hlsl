#ifndef ECLIPSE_VOLUMESHADOWS_INCLUDED
#define ECLIPSE_VOLUMESHADOWS_INCLUDED

#include "EclipseCommon.hlsl"

TEXTURE2D(_DirectionalShadowAtlas);
SAMPLER(sampler_DirectionalShadowAtlas);

SamplerState sampler_linear_clamp;

TEXTURE2D(_OtherShadowAtlas);
SAMPLER(sampler_OtherShadowAtlas);

int _VolumeShadowSamples;
float _VolumeShadowBlur;

struct DirectionalShadowData 
{
	float strength;
	int tileIndex;
	int cascadeId;
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

DirectionalShadowData GetDirShdData (int id, float3 positionWS)
{
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData[id].x;
	int cascade = GetCascade(positionWS);
	if(cascade == _CascadeCount)
		data.strength = 0;
	data.tileIndex = _DirectionalLightShadowData[id].y + GetCascade(positionWS);
	data.cascadeId = cascade;
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

float ShadowMapBlur(Texture2D shadowAtlas,SamplerState sampler_Shadow,float3 coords, float blur)
{
	float shadow = 0.0f;
	int numSamples = clamp(_VolumeShadowSamples,0,64);

	shadow += shadowAtlas.Sample(sampler_Shadow, coords.xy, 0) <= coords.z;

	for(int blurCount=0; blurCount<numSamples; blurCount++)
	{
		float2 offset = poissonDisk[blurCount] * blur;

		float offsetShadow = shadowAtlas.Sample(sampler_Shadow, coords.xy + offset, 0) <= coords.z+blur;
		shadow += offsetShadow;
	}
	shadow = shadow/(numSamples+1);
	return shadow;
}

float ShadowMapReproject(
	float radius, Texture2D shadowAtlas,SamplerState sampler_Shadow, float3 coords
){
	float shadow = 0.0f;
	int numSamples = clamp(_VolumeShadowSamples,0,64);
	shadow += shadowAtlas.Sample(sampler_Shadow, coords.xy, 0) <= coords.z;

	for(int count=0; count<numSamples; count++)
	{
		float2 offset = poissonDisk[count] * radius;

		float offsetShadow = shadowAtlas.Sample(sampler_Shadow, coords, 0) <= coords.z ;
		shadow += offsetShadow;
	}
	shadow = shadow/(numSamples+1);
	return shadow;
}

float3 GetDirShadow(int id, SurfaceData surface)
{
	DirectionalShadowData dirShadow = GetDirShdData(id,surface.position);
    if(dirShadow.strength <= 0.0)
	{
		return 1;
	}
	float texelSize = _CascadeData[dirShadow.cascadeId].x;

	float4x4 mat = _DirectionalShadowMatrices[dirShadow.tileIndex];
	float4x4 invMat = _DirectionalShadowInverseMatrices[dirShadow.tileIndex];
	float normalBias = surface.normal * _CascadeData[dirShadow.cascadeId].y;

	float3 axis = float3(0,1,0);
	float3x3 rotMatrix = AxisAngle3x3(axis,180*_VolumeShadowBlur);

	float3 pos = mul(rotMatrix,surface.position);

	float4 positionSTS = mul(
		mat,
		float4(pos, 1.0));

    float shadow = _DirectionalShadowAtlas.Sample(sampler_DirectionalShadowAtlas, positionSTS, 0);
	float4 shdSTS = float4(positionSTS.xy,shadow,positionSTS.w);
	float3 shadowPos = mul(invMat,shdSTS);
	float3 reconstructedPos = mul(invMat,positionSTS);

	shadow = lerp(1.0, shadow, dirShadow.strength);
	return shadowPos;
}

float3 GetOthShadow(int id, SurfaceData surface, float3 lightDir)
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
		float4(surface.position, 1.0));

	float3 coord = positionSTS.xyz / positionSTS.w;
	float shadow = _OtherShadowAtlas.Sample(sampler_OtherShadowAtlas,coord,0);//ShadowMapBlur(_OtherShadowAtlas, sampler_OtherShadowAtlas, coord, _VolumeShadowBlur*0.01f);//SAMPLE_TEXTURE2D(_OtherShadowAtlas,sampler_OtherShadowAtlas,coord)< coord.z;

	//float finalShadow = 0;

	shadow = lerp(1.0, shadow, othShadow.strength);
	shadow = smoothstep(0,1,shadow);
	return shadow;
}

#endif