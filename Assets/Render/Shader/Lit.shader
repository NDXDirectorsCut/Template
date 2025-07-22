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
        [NoScaleOffset] _EmissionMap("Emission", 2D) = "white" {}
        _EmissionInv("Inversion", Float) = 0
        [HDR] _EmissionTint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)

        //PBR
        [NoScaleOffset] _SpecularMap("Specular", 2D) = "white" {}
        _SpecularInv("Inversion", Float) = 1
        _SpecularTint("Tint", Color) = (1.0, 1.0, 1.0, 1.0) 
        [NoScaleOffset] _MetalnessMap("Metalness", 2D) = "white" {}
        _MetalnessInv("Inversion", Float) = 0
        [NoScaleOffset] _RoughnessMap("Roughness", 2D) = "white" {}
        _RoughnessInv("Inversion", Float) = 1

        //Extra PBR
        [NoScaleOffset] _ClearcoatMap("Clearcoat", 2D) = "white" {}
        _ClearcoatInv("Inversion", Float) = 0
        _ClearcoatRgh("Roughness", Float) = 0.5
        [NoScaleOffset] _SheenMap("Sheen", 2D) = "white" {}
        _SheenPow("Power", Float) = 4
        _SheenInv("Inversion", Float) = 0
        _SheenTint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0 

    }
    SubShader
    {
        Cull Off
        Pass
        {
            // The value of the LightMode Pass tag must match the ShaderTagId in ScriptableRenderContext.DrawRenderers
            Tags { "LightMode" = "Eclipse"}
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing    
            #pragma target 5.0

            #include "EclipseCommon.hlsl"   
            #include "BasicPass.hlsl"
            #include "VolumeShadows.hlsl"
            #include "PBRPass.hlsl"
            #include "PBRExtraPass.hlsl"
            #include "LightingPass.hlsl"
            #include "AmbientCubePass.hlsl"
            
            TEXTURE2D(_AlbedoMap);
            UNITY_DEFINE_INSTANCED_PROP(float4, _AlbedoMap_ST);    
            SAMPLER(sampler_AlbedoMap);
            float4 _AlbedoTint;
            float4 _ScaleOffset;
            float _Cutoff;
            TEXTURE2D(_AlphaMap);
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
            TEXTURE2D(_ClearcoatMap);
            float _ClearcoatInv;
            float _ClearcoatRgh;
            TEXTURE2D(_SheenMap);
            float _SheenPow;
            float _SheenInv;
            float3 _SheenTint;

            

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

            float4 frag (Varyings IN) : SV_TARGET
            {
                float3 result;

                //Basic Pass
                float4 albedo = SAMPLE_TEXTURE2D(_AlbedoMap,sampler_AlbedoMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw); 
                float4 alpha = SAMPLE_TEXTURE2D(_AlphaMap,sampler_AlbedoMap,IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float4 normalMap = SAMPLE_TEXTURE2D(_NormalMap,sampler_AlbedoMap,IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float3 nrmMapWS = NormalTangentToWorld(DecodeNormal(normalMap,_NormalStrength),normalize(IN.normalWS),IN.tangentWS);

                float3 emission = SAMPLE_TEXTURE2D(_EmissionMap,sampler_AlbedoMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float specularity = SAMPLE_TEXTURE2D(_SpecularMap,sampler_SpecularMap,IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float metalness = SAMPLE_TEXTURE2D(_MetalnessMap,sampler_AlbedoMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float roughness = SAMPLE_TEXTURE2D(_RoughnessMap,sampler_RoughnessMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float sheenTex = SAMPLE_TEXTURE2D(_SheenMap, sampler_RoughnessMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float clearcoatness = SAMPLE_TEXTURE2D(_ClearcoatMap,sampler_SpecularMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);

                surface =
                GetSurface(
                    float4(albedo.rgb,1), // Albedo
                    _AlbedoTint, // Albedo Tint
                    saturate(Inversion(alpha,_AlphaInv)), // Alpha
                    nrmMapWS, // Normal    
                    Inversion(emission,_EmissionInv), // Emission
                    _EmissionTint, // Emission Tint
                    clamp(Inversion(specularity,_SpecularInv),0,1), //Specular
                    _SpecularTint, // Specular Tint
                    clamp(Inversion(metalness,_MetalnessInv),0,1), // Metalness
                    clamp(Inversion(roughness,_RoughnessInv),0,1), // Roughness
                    Inversion(sheenTex,_SheenInv),// Sheen
                    _SheenTint,

                    normalize(_WorldSpaceCameraPos - IN.positionWS), // View Direction
                    IN.positionWS // Position
                );
                
                float3 metallicColor = GetMetalness(surface);
                float3 reflection = GetEnvironmentReflection(surface,surface.viewDir);
                reflection = reflection * _EnvironmentReflection;

                //Lighting
                float3 ambientLight = GetAmbientLight(surface);
                ambientLight = ambientLight * lerp(1,surface.diffuse,.9) * _EnvironmentLighting;
                float3 lighting = GetLighting(surface);
                float3 specular = GetSpecular(surface);
                float3 sheen = GetSheen(surface,_SheenPow);

                surface.roughness = clamp(_ClearcoatRgh,0,1);
                surface.specularity = clearcoatness;
                float3 clearcoat = Inversion(GetSpecular(surface),_ClearcoatInv);

                //float avgLum = 0.2126*ambientLight.r + 0.7152*ambientLight.g + 0.0722*ambientLight.b;
                //reflection *= avgLum*10;
                sheen *= normalize(ambientLight+0.01f);
                //sheen *= (lighting+.5f)/2;

                result = surface.diffuse;
                result = metallicColor;
                result *= lighting;
                result += specular;
                result += clearcoat;
                result += sheen;
                result += surface.emission;
                result += ambientLight;
                result += reflection;

                #ifdef _CLIPPING
                    clip(surface.alpha - _Cutoff);
                    surface.alpha = surface.alpha > _Cutoff;
                #endif
                return float4(result,surface.alpha);//result;//abs(length(normal) - 1.0) * 10.0;;
            }
            
            ENDHLSL
        }

        Cull Off
        Pass {
			Tags { "LightMode" = "ShadowCaster"}

			ColorMask 0

			HLSLPROGRAM
			#pragma target 5.0
            #pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment 
			#include "EclipseCommon.hlsl"  

            TEXTURE2D(_AlbedoMap);
            UNITY_DEFINE_INSTANCED_PROP(float4, _AlbedoMap_ST);    
            SAMPLER(sampler_AlbedoMap);
            TEXTURE2D(_AlphaMap);
            float4 _ScaleOffset;
            float _Cutoff;

            struct Attributes {
                float3 positionOS : POSITION;
                float2 baseUV : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 baseUV : VAR_BASE_UV;
            };

            bool _ShadowPancaking;

            Varyings ShadowCasterPassVertex (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.positionCS = TransformWorldToHClip(worldPos);

                if(_ShadowPancaking)
                {
                    #if UNITY_REVERSED_Z
                    OUT.positionCS.z = min(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                    #else
                    OUT.positionCS.z = max(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                    #endif
                }
                OUT.baseUV = IN.baseUV;

                return OUT;
            }

            void ShadowCasterPassFragment (Varyings IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                float4 albedo = SAMPLE_TEXTURE2D(_AlbedoMap,sampler_AlbedoMap, IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float4 alpha = SAMPLE_TEXTURE2D(_AlphaMap,sampler_AlbedoMap,IN.baseUV * _ScaleOffset.xy + _ScaleOffset.zw);
                float cut = 0.5f;
                #ifdef _CLIPPING
                    cut = _Cutoff;
                #endif

                clip((albedo.a*alpha) - cut);
            }
			ENDHLSL
		}
    }
}