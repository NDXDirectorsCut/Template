//#include "../Include/Common.hlsl"

//TEXTURE2D(_AlbedoMap);
//SAMPLER(sampler_AlbedoMap);

//UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
  //  UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
//UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

float4x4 unity_MatrixVP;
float4x4 unity_ObjectToWorld;

struct Attributes
{
    float4 positionOS : POSITION;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
};

Varyings vert (Attributes IN)
{
    Varyings OUT;
    float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
    OUT.positionCS = mul(unity_MatrixVP, worldPos);
    return OUT;
}

float4 frag (Varyings IN) : SV_TARGET
{
    //float4 albedoMap = SAMPLE_TEXTURE2D(_AlbedoMap, sampler_AlbedoMap, IN.baseUV);
    //float surfaceColor = /*albedoMap*/ _Tint;
    return float4(1,0,0,1);//surfaceColor;
}