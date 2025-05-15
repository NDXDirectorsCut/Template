using UnityEngine;
using UnityEngine.Rendering;

public enum MapSize 
{
		_256 = 256, _512 = 512, _1024 = 1024,
		_2048 = 2048, _4096 = 4096, _8192 = 8192    
}

// [System.Serializable]
// public struct DirectionalSettings
// {
// 	public MapSize shadowAtlas;
// }

// [System.Serializable]
// public struct LightingSettings
// {
//     [Range(0,10)] public float environmentLighting;
//     [Range(0,10)] public float environmentReflection;
// }
    
// [System.Serializable]
// public class ShadowSettings {

// 	[Min(0f)]
// 	public float shadowDistance = 100f;
//     public DirectionalSettings directional = new DirectionalSettings
//     {
//         shadowAtlas = MapSize._1024
//     };
    
// }

[System.Serializable]
public class RndSettings
{
    [Header("Lighting")]
    [Range(0,10)] public float environmentLighting;
    [Range(0,10)] public float environmentReflection;
    [Header("Shadows")]
    public float shadowDistance = 100f;
    [Header("Directional")]
    public MapSize dirShadowAtlas = MapSize._1024;
    [Range(1,4)] public int cascadeCount = 4;
    [Range(0f,1f)]
    public float 
        cascadeRatio1 = .1f,
        cascadeRatio2 = .25f,
        cascadeRatio3 = .5f;
    [Header("Other")]
    public MapSize othShadowAtlas = MapSize._1024;
}

// The CreateAssetMenu attribute lets you create instances of this class in the Unity Editor.
[CreateAssetMenu(menuName = "Rendering/EclipseRenderPipelineAsset")]
public class EclipseRenderPipelineAsset : RenderPipelineAsset
{
    // This data can be defined in the Inspector for each Render Pipeline Asset
    [SerializeField]
    RndSettings renderSettings;
    
        // Unity calls this method before rendering the first frame.
        // If a setting on the Render Pipeline Asset changes, Unity destroys the current Render Pipeline Instance and calls this method again before rendering the next frame.
    protected override RenderPipeline CreatePipeline() {
        // Instantiate the Render Pipeline that this custom SRP uses for rendering, and pass a reference to this Render Pipeline Asset.
        // The Render Pipeline Instance can then access the configuration data defined above.
        return new EclipseRenderPipelineInstance(this,renderSettings);
    }
}