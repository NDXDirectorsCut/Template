// Aaron Lanterman, July 2, 2023
// Based heavily on https://catlikecoding.com/unity/tutorials/scriptable-render-pipeline/
Shader "GPU23SRP/MyShadowLit"
{
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    
    SubShader {
        Pass {
            HLSLPROGRAM
            
// Unity defaults to target 2.5                        
#pragma target 3.5
#pragma multi_compile_instancing
                    
#pragma vertex MyLitPassVertex
#pragma fragment MyLitPassFragment

// Contains CBUFFER macros  
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
// This may redefine UNITY_MATRIX_M and UNITY_MATRIX_I_M in case of GPU instancing
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
  
// Unity will populate these variables for us
// CBUFFER definitions must be consistent throughout project
CBUFFER_START(UnityPerFrame) 
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
    float4x4 unity_WorldToObject;
CBUFFER_END

CBUFFER_START(UnityPerMaterial)
    float4 _Color;
CBUFFER_END

#define MAX_VISIBLE_LIGHTS 4

CBUFFER_START(_LightBuffer)
	float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightDirectionsOrPositions[MAX_VISIBLE_LIGHTS];
CBUFFER_END

CBUFFER_START(_ShadowBuffer)
	float4x4 _WorldToShadowMatrix;
CBUFFER_END
       
TEXTURE2D_SHADOW(_ShadowMap);
SAMPLER_CMP(sampler_ShadowMap);

struct VertexInput {
    float4 pos : POSITION;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID 
};

struct VertexOutput {
    float4 clipPos : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float3 normal : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID 
};

VertexOutput MyLitPassVertex (VertexInput input) {
    VertexOutput output;
    // UNITY_MATRIX_M is basically unity_ObjectToWorld, extended to instancing
    UNITY_SETUP_INSTANCE_ID(input);
    float4 positionWS = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
    output.clipPos = mul(unity_MatrixVP, positionWS);
    output.positionWS = positionWS.xyz;

    // To transform normals, we want to use the 
    // inverse transpose of upper left 3x3
    // Putting input.n in first argument is like doing  
    // trans((float3x3)_World2Object) * input.n 
    output.normal = normalize(mul(input.normal, (float3x3) UNITY_MATRIX_I_M));
    return output;
}

float4 MyLitPassFragment (VertexOutput input) : SV_TARGET {
    UNITY_SETUP_INSTANCE_ID(input);
    input.normal = normalize(input.normal);
    float3 albedo = _Color.rgb;

	float3 diffuseLight = 0;
	for (int i = 0; i < MAX_VISIBLE_LIGHTS; i++) {
        float3 lV = _VisibleLightDirectionsOrPositions[i].xyz 
                        - input.positionWS * _VisibleLightDirectionsOrPositions[i].w;
        lV = normalize(lV);
		diffuseLight += _VisibleLightColors[i].rgb * 
        	  saturate(dot(input.normal, lV));
	}

	float4 shadowPos = mul(_WorldToShadowMatrix, float4(input.positionWS, 1.0));
    shadowPos.xyz /= shadowPos.w;
    float shadowAttenuation =
       SAMPLE_TEXTURE2D_SHADOW(_ShadowMap, sampler_ShadowMap, shadowPos.xyz);

	float3 color = shadowAttenuation * diffuseLight * albedo;
	return float4(color, _Color.a); 
}

    ENDHLSL
}  // end of main Lit pass

// Shadow pass
        Pass {
            Tags {
				"LightMode" = "ShadowCaster"
			}
            HLSLPROGRAM
            
// Unity defaults to target 2.5                        
#pragma target 3.5
                    
#pragma vertex ShadowCasterPassVertex
#pragma fragment ShadowCasterPassFragment

// Contains CBUFFER macros  
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
// This may redefine UNITY_MATRIX_M and UNITY_MATRIX_I_M in case of GPU instancing
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
  
// Unity will populate these variables for us
// CBUFFER definitions must be consistent throughout project
CBUFFER_START(UnityPerFrame) 
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

CBUFFER_START(_ShadowCasterBuffer)
	float _ShadowBias;
CBUFFER_END

struct VertexInput {
    float4 pos : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID 
};

struct VertexOutput {
    float4 clipPos : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID 
};

VertexOutput ShadowCasterPassVertex (VertexInput input) {
    VertexOutput output;
    // UNITY_MATRIX_M is basically unity_ObjectToWorld, extended to instancing
    UNITY_SETUP_INSTANCE_ID(input);
    float4 positionWS = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
    output.clipPos = mul(unity_MatrixVP, positionWS);
	#if UNITY_REVERSED_Z
		output.clipPos.z -= _ShadowBias;
	#else
		output.clipPos.z += _ShadowBias;
   	#endif
    return output;
}

float4 ShadowCasterPassFragment (VertexOutput input) : SV_TARGET {
    return 0;
}

            ENDHLSL
        }
    }
}