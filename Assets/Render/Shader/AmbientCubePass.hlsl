#ifndef ECLIPSE_AMBIENTCUBE_INCLUDED
#define ECLIPSE_AMBIENTCUBE_INCLUDED

float4 unity_SpecCube0_HDR;

float3 GetAmbientLight(SurfaceData surface)
{
    float3 dir = normalize(reflect(-surface.normal,surface.normal));
    float lod = PerceptualRoughnessToMipmapLevel(.75);
    float4 ambientLight = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, dir,lod);
    float3 brdfSpecular = lerp(0.04, surface.diffuse, surface.metalness);

    float4 cubeTest = float4(0,0,0,0);
    //cubeTest = _ReflectionProbeArray.SampleLevel(sampler_ReflectionProbeArray,float4(dir,1),1);//UNITY_SAMPLE_TEXCUBEARRAY(_ReflectionProbeArray,float4(dir,1));
    
    //ambientLight *= brdfSpecular;
    //ambientLight /= 10;
    return float3((DecodeHDREnvironment(ambientLight,unity_SpecCube0_HDR)/10).xyz );//clamp(DecodeHDREnvironment(cubeTest,unity_SpecCube0_HDR)/10,0,1);
}

#endif