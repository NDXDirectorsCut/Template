#ifndef ECLIPSE_VOLUMESHADOWS_INCLUDED
#define ECLIPSE_VOLUMESHADOWS_INCLUDED

#include "EclipseCommon.hlsl"

TEXTURE2D(_DirectionalShadowAtlas);
SAMPLER(sampler_DirectionalShadowAtlas);

TEXTURE2D(_OtherShadowAtlas);
SAMPLER(sampler_OtherShadowAtlas);

int _VolumeShadowSamples;
float _VolumeShadowBlur;

#define pi 3.14159265359
#define e 2.71828182846

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
	float numSamples = clamp(_VolumeShadowSamples,0,64);

	float4 positionSTS = mul(mat,float4(pos, 1.0));
	float3 coords = positionSTS.xyz/positionSTS.w;
	shadow += shadowAtlas.Sample(state, coords.xy, 0) <= coords.z;
	float zCoord = coords.z;

	for(int blurCount=0; blurCount<numSamples; blurCount++)
	{
		float2 offset = poissonDisk[blurCount] * blur;
		float3 offsetPos = pos+float3(offset.x,offset.y,offset.y);

		positionSTS = mul(mat,float4(offsetPos, 1.0));
		coords = positionSTS.xyz/positionSTS.w;
		//coords = clamp(coords,0,1);

		float offsetShadow = shadowAtlas.Sample(state, coords, 0) <= coords.z;
		shadow += offsetShadow;
	}
	shadow = shadow/(numSamples+1);
	return shadow;
}

float ShadowMapGauss(Texture2D shadowAtlas,SamplerState state, float4x4 mat, float blur, float3 pos)
{
	float sum = 0;
	float4 result = 0;
	int numSamples = clamp(_VolumeShadowSamples,0,16);
	float stDev = clamp(blur*0.5f,0.01f,blur*0.5f+1);
	float stDevSquared = stDev*stDev;
	for(float i=0; i<numSamples; i++)
	{
		for(float j=0; j<numSamples; j++)
		{
			float xOffset = (i/(numSamples-1)-0.5f) * blur;
			float yOffset = (j/(numSamples-1)-0.5f) * blur;
			float2 offset = float2(xOffset,yOffset);

			float3 offsetPos = pos;

			float gauss = ( 1/sqrt(2*pi*stDevSquared)) * pow(e, -((xOffset*xOffset)/(2*stDevSquared)) );
			sum += gauss;

			float4 positionSTS = mul(mat,float4(pos, 1.0));
			float3 coords = positionSTS.xyz/positionSTS.w;

			float shadow = shadowAtlas.Sample(state,coords + offset,0)<= coords.z;

			result += shadow*gauss;
		}
	}

	result = result/sum;
	return result;
}

float DilateShadow(Texture2D t2D,SamplerState state,float4x4 mat, float size, float3 pos,bool mode = 0)
{
	float dilated = 0.0f;

	float4 positionSTS = mul(mat,float4(pos, 1.0));
	float3 coords = positionSTS.xyz/positionSTS.w;

	float ref = t2D.Sample(state, coords, 0)-coords.z;
	dilated = clamp(ref,0,1000);

	if(mode==0)
	{	
		int numSamples = clamp(_VolumeShadowSamples,0,16);
		for(int dilate=0; dilate<numSamples; dilate++)
		{
			float2 offset = poissonDisk[numSamples-dilate] * size;
			float3 offsetPos = pos+float3(offset,0);

			positionSTS = mul(mat,float4(offsetPos, 1.0));
			coords = positionSTS.xyz/positionSTS.w;
			coords = clamp(coords,0,1);

			float shadow = t2D.Sample(state, coords, 0)-coords.z;
			shadow = clamp(shadow,0,1000);
			if(shadow>dilated)
			{
				dilated = shadow;
			}
		}
		return dilated;
	}
	else
	{
		float sum = 0;
		float4 result = 0;
		int numSamples = clamp(_VolumeShadowSamples,0,16);
		float stDev = clamp(size*0.5f,0.01f,size*0.5f+1);
		float stDevSquared = stDev*stDev;
		for(float i=0; i<numSamples; i++)
		{
			for(float j=0; j<numSamples; j++)
			{
				float xOffset = (i/(numSamples-1)-0.5f) * size;
				float yOffset = (j/(numSamples-1)-0.5f) * size;
				float2 offset = float2(xOffset,yOffset);

				float gauss = ( 1/sqrt(2*pi*stDevSquared)) * pow(e, -((xOffset*xOffset)/(2*stDevSquared)) );
				sum += gauss;

				float4 positionSTS = mul(mat,float4(pos, 1.0));
				float3 coords = positionSTS.xyz/positionSTS.w;

				float shadow = t2D.Sample(state,coords+offset,0)-coords.z;

				result = shadow>result? shadow : result;
			}
		}

		//result = result/sum;
		return result;
	}
	
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

	float3 pos = surface.position + normalBias*1.5f;

	float4 positionSTS = mul(
		mat,
		float4(pos, 1.0));

	//float shadow = PCSS(_DirectionalShadowAtlas,positionSTS, texelSize);
	float strength = 0.1f;
	float dilation = DilateShadow(_DirectionalShadowAtlas, sampler_DirectionalShadowAtlas, mat,_VolumeShadowBlur*texelSize,pos,true);//
	dilation = dilation*1/texelSize;

	float blurParameter = _VolumeShadowBlur*0.1f*dilation;
    float shadow = ShadowMapGauss(_DirectionalShadowAtlas,sampler_DirectionalShadowAtlas,mat,blurParameter*texelSize,pos);
	//shadow += ShadowMapGauss(_DirectionalShadowAtlas,sampler_DirectionalShadowAtlas,mat,_VolumeShadowBlur,pos,float3(0,0,1));
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

	float texelSize = 1.0/8;

	float dilation = DilateShadow(_OtherShadowAtlas,sampler_OtherShadowAtlas,mat,_VolumeShadowBlur * texelSize,pos,true);

	float blurParameter = _VolumeShadowBlur*dilation;
	float shadow = ShadowMapBlur(_OtherShadowAtlas,sampler_OtherShadowAtlas,mat, blurParameter*4,pos);
	//shadow = GaussianBlur(_OtherShadowAtlas,sampler_OtherShadowAtlas,coord);

	//float finalShadow = 0;

	shadow = lerp(1.0, shadow, othShadow.strength);
	//shadow = smoothstep(0,1,shadow);
	return shadow;
}

#endif