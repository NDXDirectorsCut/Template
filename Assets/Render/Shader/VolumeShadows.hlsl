#ifndef ECLIPSE_VOLUMESHADOWS_INCLUDED
#define ECLIPSE_VOLUMESHADOWS_INCLUDED

#include "EclipseCommon.hlsl"

TEXTURE2D(_DirectionalShadowAtlas);
SAMPLER(sampler_DirectionalShadowAtlas);
SAMPLER_CMP(sampler_linear_clamp);

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

float ShadowMapBlur(Texture2D shadowAtlas,SamplerState state,float4x4 mat,float blur, float3 pos)
{
	float shadow = 0.0f;
	int numSamples = clamp(_VolumeShadowSamples,0,64);

	float4 positionSTS = mul(mat,float4(pos, 1.0));
	float3 coords = positionSTS.xyz/positionSTS.w;
	//shadow += shadowAtlas.Sample(state, coords.xy, 0) <= coords.z;

	for(int blurCount=0; blurCount<numSamples; ++blurCount)
	{
		float2 offset = poissonDisk[blurCount] * blur;
		float3 offsetPos = pos+float3(offset,0);

		positionSTS = mul(mat,float4(pos, 1.0));
		coords = positionSTS.xyz/positionSTS.w;

		float offsetShadow = shadowAtlas.Sample(state, coords, 0) < coords.z+blur;
		shadow += offsetShadow;
	}
	shadow = shadow/(numSamples+1);
	return shadow;
}

float DilateShadow(Texture2D t2D,SamplerState state,float4x4 mat, float size, float3 pos,bool mode = 0)
{
	float dilated = 0.0f;
	int numSamples = clamp(_VolumeShadowSamples,0,64);

	float4 positionSTS = mul(mat,float4(pos, 1.0));
	float3 coords = positionSTS.xyz/positionSTS.w;

	float ref = t2D.Sample(state, coords, 0)-coords.z;
	dilated = clamp(ref,0,1000);

	for(int dilate=0; dilate<numSamples; dilate++)
	{
	 	float2 offset = poissonDisk[numSamples-dilate] * size;
		float3 offsetPos = pos+float3(offset,0);

	 	positionSTS = mul(mat,float4(offsetPos, 1.0));
		coords = positionSTS.xyz/positionSTS.w;

	 	float shadow = t2D.Sample(state, coords, 0)-coords.z;
		if(coords.x>1 || coords.x<0 || coords.y>1 || coords.y<0)
			shadow = 0;
		
	 	shadow = clamp(shadow,0,1000);
		if(mode == 0)
		{
			if(shadow>dilated)
			{
				dilated = shadow;
			}
		}
		else
		{
			dilated += shadow;
		}
	}
	if(mode ==0)
	{
		return dilated;
	}
	return dilated/(numSamples+1);
}

float3 GetDirShadow(int id, SurfaceData surface)
{
	DirectionalShadowData dirShadow = GetDirShdData(id,surface.position);
    if(dirShadow.strength <= 0.0)
	{
		return 1;
	}
	float texelSize = _CascadeData[dirShadow.cascadeId].x;
	float4 sphere = _CascadeCullingSpheres[dirShadow.cascadeId];

	float4x4 mat = _DirectionalShadowMatrices[dirShadow.tileIndex];
	float4x4 invMat = _DirectionalShadowInverseMatrices[dirShadow.tileIndex];
	float normalBias = surface.normal * _CascadeData[dirShadow.cascadeId].y;

	float3 axis = float3(0,1,0);
	float3x3 rotMatrix = AxisAngle3x3(axis,180*_VolumeShadowBlur);

	float3 pos = surface.position + normalBias;

	float4 positionSTS = mul(
		mat,
		float4(pos, 1.0));

	//float shadow = PCSS(_DirectionalShadowAtlas,positionSTS, texelSize);
	float strength = 0.1f;
	float dilation = DilateShadow(_DirectionalShadowAtlas, sampler_DirectionalShadowAtlas, mat,_VolumeShadowBlur,pos,false);//
	float test = rsqrt(sphere.w);
	dilation = dilation*(dirShadow.cascadeId*2+1);
	float blurParameter = _VolumeShadowBlur*0.25f*dilation*strength;
    float shadow = ShadowMapBlur(_DirectionalShadowAtlas,sampler_DirectionalShadowAtlas,mat,_VolumeShadowBlur,pos);
	//shadow = shadow*(1+_VolumeShadowBlur*0.5f);
	shadow = clamp(shadow,0,1);
	shadow = lerp(1.0, shadow, dirShadow.strength);
	return shadow;
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

	float4x4 mat = _OtherShadowMatrices[tileIndex];

	float3 pos = surface.position+surface.normal*0.01;

	float4 positionSTS = mul(
		mat,
		float4(pos, 1.0));

	float3 coord = positionSTS.xyz / positionSTS.w;

	float dilation = DilateShadow(_OtherShadowAtlas,sampler_OtherShadowAtlas,mat,_VolumeShadowBlur*0.5f,pos,false);
	float shadow = ShadowMapBlur(_OtherShadowAtlas,sampler_OtherShadowAtlas,mat, _VolumeShadowBlur*0.25f*dilation,pos);
	//shadow = GaussianBlur(_OtherShadowAtlas,sampler_OtherShadowAtlas,coord);

	//float finalShadow = 0;

	shadow = lerp(1.0, shadow, othShadow.strength);
	//shadow = smoothstep(0,1,shadow);
	return shadow;
}

#endif