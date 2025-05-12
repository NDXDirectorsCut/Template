#ifndef ECLIPSE_AMBIENTCUBE_INCLUDED
#define ECLIPSE_AMBIENTCUBE_INCLUDED

float3 GetAmbientLight(SurfaceData surface)
{
    float3 dir = reflect(-surface.normal,surface.normal);
    float lod = PerceptualRoughnessToMipmapLevel(.75);
    float3 ambientLight = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, dir,lod);
    float3 brdfSpecular = lerp(0.04, surface.diffuse, surface.metalness);
    //ambientLight *= brdfSpecular;
    return ambientLight;
}

#endif