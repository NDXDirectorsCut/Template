// This defines a simple unlit Shader object that is compatible with a custom Scriptable Render Pipeline.
// It applies a hardcoded color, and demonstrates the use of the LightMode Pass tag.
// It is not compatible with SRP Batcher.

Shader "Eclipse/Lit"
{
    Properties
    {
        _AlbedoMap("Albedo", 2D) = "white" {}
        _AlbedoTint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        _AlphaMap("Alpha", 2D) = "white" {}
        _AlphaInv("Inversion", Float) = 1
        _NormalMap("Normal",2D) = "bump" {}
        _NormalStrength("Strength", Float) = 1
        _EmissionMap("Emission", 2D) = "black"
        _EmissionInv("Inversion", Float) = 1
        // _EmissionTint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)

        _SpecularMap("Specular", 2D) = "white" {}
        _SpecularInv("Inversion", Float) = 1
        _SpecularTint("Tint", Color) = (1.0, 1.0, 1.0, 1.0) 
        _MetalnessMap("Metalness", 2D) = "black" {}
        _MetalnessInv("Inversion", Float) = 1
        _RoughnessMap("Roughness", 2D) = "white" {}

    }
    SubShader
    {
        Pass
        {
            // The value of the LightMode Pass tag must match the ShaderTagId in ScriptableRenderContext.DrawRenderers
            Tags { "LightMode" = "EclipseLightModeTag"}

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing    

            #include "EclipseCommon.hlsl"   
            #include "BasicPass.hlsl"
            #include "LightingPass.hlsl"
            #include "PBRPass.hlsl"
            
            TEXTURE2D(_AlbedoMap);
            TEXTURE2D(_AlbedoMap_ST);
            SAMPLER(sampler_AlbedoMap);
            float4 _AlbedoTint;
            float _AlphaInv;
            TEXTURE2D(_NormalMap);
            TEXTURE2D(_NormalMap_ST);
            float _NormalStrength;
            TEXTURE2D(_EmissionMap);
            TEXTURE2D(_EmissionMap_ST);

            TEXTURE2D(_SpecularMap);
            TEXTURE2D(_Specular_ST);
            SAMPLER(sampler_SpecularMap);
            float4 _SpecularTint;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 baseUV : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : VAR_POSITION;
                float3 normalWS : VAR_NORMAL;
                float2 baseUV : VAR_BASE_UV;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionWS = worldPos;
                OUT.positionCS = mul(unity_MatrixVP, worldPos);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.baseUV = IN.baseUV;

                return OUT;
            }

            float3 frag (Varyings IN) : SV_TARGET
            {
                float3 result;

                //From properties
                float3 normal = normalize(IN.normalWS);
                float3 position = IN.positionWS;
                float3 viewDir = normalize(_WorldSpaceCameraPos - position);

                float4 albedo = SAMPLE_TEXTURE2D(_AlbedoMap, sampler_AlbedoMap, IN.baseUV);
                float alpha = albedo.w;
                

                //Basic Pass
                result = GetDiffuse(albedo,_AlbedoTint,float3(0,0,0));

                //PBR Pass



                //Lighting Pass
                float3 lighting = GetLighting(normal,position);
                result = result * lighting;

                return GetPBR(viewDir,normal,albedo,1,0,.9);//abs(length(normal) - 1.0) * 10.0;;
            }
            
            ENDHLSL
        }
    }
}