// Aaron Lanterman, July 2, 2023
// Based heavily on https://catlikecoding.com/unity/tutorials/scriptable-render-pipeline/
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]
public class MyPipelineAsset : RenderPipelineAsset {

    public bool dynamicBatching;
    public bool instancing;

    protected override RenderPipeline CreatePipeline () {
        return new MyPipeline(dynamicBatching, instancing);
    }
}