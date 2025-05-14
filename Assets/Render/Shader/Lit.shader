Shader "Eclipse/Lit"
{
    Properties
    {
        [NoScaleOffset] _AlbedoMap("Albedo", 2D) = "white" {}
        _AlbedoTint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        _ScaleOffset("ScaleOffset", Vector) = (1.0, 1.0,0,0)
        [NoScaleOffset] _AlphaMap("Alpha", 2D) = "white" {}
        _AlphaInv("Inversion", Float) = 1
        [NoScaleOffset] [Normal] _NormalMap("Normal",2D) = "bump" {}
        _NormalStrength("Strength", Float) = 1
        [NoScaleOffset] _EmissionMap("Emission", 2D) = "black" {}
        _EmissionInv("Inversion", Float) = 1
        [HDR] _EmissionTint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)

        [NoScaleOffset] _SpecularMap("Specular", 2D) = "white" {}
        _SpecularInv("Inversion", Float) = 1
        _SpecularTint("Tint", Color) = (1.0, 1.0, 1.0, 1.0) 
        [NoScaleOffset] _MetalnessMap("Metalness", 2D) = "black" {}
        _MetalnessInv("Inversion", Float) = 1
        [NoScaleOffset]     _RoughnessMap("Roughness", 2D) = "white" {}
        _RoughnessInv("Inversion", Float) = 1

        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1

    }
    SubShader
    {
        Pass
        {
            // The value of the LightMode Pass tag must match the ShaderTagId in ScriptableRenderContext.DrawRenderers
            Tags { "LightMode" = "Eclipse"}
            ZWrite [_ZWrite]
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing    

            #include "EclipseCommon.hlsl"   
            #include "BasicPass.hlsl"
            #include "LightingPass.hlsl"
            #include "PBRPass.hlsl"
            #include "AmbientCubePass.hlsl"
            
            TEXTURE2D(_AlbedoMap);
            UNITY_DEFINE_INSTANCED_PROP(float4, _AlbedoMap_ST);    
            SAMPLER(sampler_AlbedoMap);
            float4 _AlbedoTint;
            float4 _ScaleOffset;
            float _AlphaInv;
            TEXTURE2D(_NormalMap);
            UNITY_DEFINE_INSTANCED_PROP(float4, _NormalMap_ST);  
            float _NormalStrength;
            TEXTURE2D(_EmissionMap);
            UNITY_DEFINE_INSTANCED_PROP(float4, _EmissionMap_ST);
            float _EmissionInv;
            float3 _EmissionTint;

            TEXTURE2D(_SpecularMap);
            UNITY_DEFINE_INSTANCED_PROP(float4, _SpecularMap_ST);
            SAMPLER(sampler_SpecularMap);
            float _SpecularInv;
            float4 _SpecularTint;
            TEXTURE2D(_MetalnessMap);
            UNITY_DEFINE_INSTANCED_PROP(float4, _MetalnessMap_ST);
            float _MetalnessInv;
            TEXTURE2D(_RoughnessMap);
            UNITY_DEFINE_INSTANCED_PROP(float4, _RoughnessMap_ST);
            SAMPLER(sampler_RoughnessMap);
            float _RoughnessInv;

            SurfaceData surface;

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
                float4 tangentWS : VAR_TANGENT;
                float2 baseUV : VAR_BASE_UV;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.positionWS = worldPos;
                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);
                OUT.baseUV = IN.baseUV;

                return OUT;
            }

            float3 frag (Varyings IN) : SV_TARGET
            {
                float3 result;

                //Basic Pass
                float4 albedo = SAMPLE_TEXTURE2D(_AlbedoMap,sampler_AlbedoMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw); 
                float4 normalMap = SAMPLE_TEXTURE2D(_NormalMap,sampler_AlbedoMap,IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float3 nrmMapWS = NormalTangentToWorld(DecodeNormal(normalMap,_NormalStrength),normalize(IN.normalWS),IN.tangentWS);

                float3 emission = SAMPLE_TEXTURE2D(_EmissionMap,sampler_AlbedoMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float specularity = SAMPLE_TEXTURE2D(_SpecularMap,sampler_SpecularMap,IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float metalness = SAMPLE_TEXTURE2D(_MetalnessMap,sampler_AlbedoMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float roughness = SAMPLE_TEXTURE2D(_RoughnessMap,sampler_RoughnessMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);

                surface =
                GetSurface(
                    albedo, //Albedo
                    _AlbedoTint, //Albedo Tint
                    1, // Alpha
                    nrmMapWS,//normalize(IN.normalWS), //Normal    
                    Inversion(emission,_EmissionInv), //Emission
                    _EmissionTint, //Emission Tint
                    clamp(Inversion(specularity,_SpecularInv),0,1), //Specular
                    _SpecularTint, //Specular Tint
                    clamp(Inversion(metalness,_MetalnessInv),0,1), //Metalness
                    clamp(Inversion(roughness,_RoughnessInv),0,1), // Roughness

                    normalize(_WorldSpaceCameraPos - IN.positionWS), // View Direction
                    IN.positionWS // Position
                );
                
                result = surface.diffuse;

                //PBR Pass
                float3 metallicColor = GetMetalness(surface);
                result = metallicColor;
                float3 specular = GetSpecular(surface);
                float3 reflection = GetSpecularReflection(surface,surface.viewDir);
                result += specular;
    
                //Lighting Pass
                float3 lighting = GetLighting(surface);
                result *= lighting;
                result += reflection * _EnvironmentReflection;// * (lighting+0.5);
                result += surface.emission;

                //Ambient Cube Pass
                float3 ambientLight = GetAmbientLight(surface);
                result += ambientLight * _EnvironmentLighting;

                return result;//abs(length(normal) - 1.0) * 10.0;;
            }
            
            ENDHLSL
        }
        Pass {
			Tags { "LightMode" = "ShadowCaster"}

			ColorMask 0

			HLSLPROGRAM
			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment 
			#include "EclipseCommon.hlsl"  

            TEXTURE2D(_AlbedoMap);
            UNITY_DEFINE_INSTANCED_PROP(float4, _AlbedoMap_ST);    
            SAMPLER(sampler_AlbedoMap);

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
                float3 worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.baseUV = IN.baseUV;

                return OUT;
            }

            void ShadowCasterPassFragment (Varyings IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                float4 albedo = SAMPLE_TEXTURE2D(_AlbedoMap,sampler_AlbedoMap, IN.baseUV);
                #if defined(_CLIPPING)
                    clip(albedo.a - 0.5);
                #endif
            }
			ENDHLSL
		}
    }
}