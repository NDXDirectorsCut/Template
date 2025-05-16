#ifndef ECLIPSE_COMMON_INCLUDED
#define ECLIPSE_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
float4 unity_LODFade;
real4 unity_WorldTransformParams;

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_prev_MatrixM;
float4x4 unity_prev_MatrixIM;
float4x4 glstate_matrix_projection;
float3 _WorldSpaceCameraPos;

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_I_V unity_MatrixInvV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_PREV_MATRIX_M unity_prev_MatrixM
#define UNITY_PREV_MATRIX_I_M unity_prev_MatrixIM
#define UNITY_MATRIX_P glstate_matrix_projection

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
//#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

float _EnvironmentLighting;
float _EnvironmentReflection;

#define MAX_DIRECTIONAL_LIGHTS 32
#define MAX_DIRECTIONAL_SHADOWS 4
#define MAX_CASCADES 4
#define MAX_OTHER_LIGHTS 128
#define MAX_OTHER_SHADOWS 32

int _DirectionalLightCount;
float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHTS];
float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHTS];
float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHTS];
int _CascadeCount;
float4 _CascadeCullingSpheres[MAX_CASCADES];
float4x4 _DirectionalShadowMatrices[MAX_DIRECTIONAL_SHADOWS * MAX_CASCADES];

int _OtherLightCount;
float4 _OtherLightColors[MAX_OTHER_LIGHTS];
float4 _OtherLightPositions[MAX_OTHER_LIGHTS];
float4 _OtherLightDirections[MAX_OTHER_LIGHTS];
float4 _OtherLightSpotAngles[MAX_OTHER_LIGHTS];
float4 _OtherLightShadowData[MAX_OTHER_LIGHTS];
float4x4 _OtherShadowMatrices[MAX_OTHER_SHADOWS];

static const float3 pointShadowPlanes[6] = {
	float3(-1.0, 0.0, 0.0),
	float3(1.0, 0.0, 0.0),
	float3(0.0, -1.0, 0.0),
	float3(0.0, 1.0, 0.0),
	float3(0.0, 0.0, -1.0),
	float3(0.0, 0.0, 1.0)
};

TextureCubeArray _ReflectionProbeArray;
SAMPLER(sampler_ReflectionProbeArray);

struct SurfaceData
{
    float3 diffuse;
    float alpha;
    float3 normal;
    float3 emission;
    float3 specularity;
    float metalness;
    float roughness;

    float3 viewDir;
    float3 position;
};

float DistanceSquared(float3 pA, float3 pB) {
	return dot(pA - pB, pA - pB);
}

float3 DecodeNormal (float4 sample, float strength) {
	#if defined(UNITY_NO_DXT5nm)
	    return normalize(UnpackNormalRGB(sample, strength));
	#else
	    return normalize(UnpackNormalmapRGorAG(sample, strength));
	#endif
}

float3 NormalTangentToWorld (float3 normalTS, float3 normalWS, float4 tangentWS) {
	float3x3 tangentToWorld = CreateTangentToWorld(normalWS, tangentWS.xyz, tangentWS.w);
	return TransformTangentToWorld(normalTS, tangentToWorld);
}

float Square (float x) 
{
	return x * x;
}

float3 Inversion(float3 value, float inversion)
{
    float3 activeValue = value;
    if(inversion < 0)
    {
        activeValue = 1 - value;
    }

    return activeValue * abs(inversion);
}

#endif