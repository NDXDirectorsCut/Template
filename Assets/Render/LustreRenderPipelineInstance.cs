using UnityEngine;
using UnityEngine.Rendering;
    
public class LustreRenderPipelineInstance : RenderPipeline
{
    // Use this variable to a reference to the Render Pipeline Asset that was passed to the constructor
    private LustreRenderPipelineAsset renderPipelineAsset;
    
    // The constructor has an instance of the ExampleRenderPipelineAsset class as its parameter.
    public LustreRenderPipelineInstance(LustreRenderPipelineAsset asset) {
        renderPipelineAsset = asset;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        // This is an example of using the data from the Render Pipeline Asset.
        Debug.Log(renderPipelineAsset.exampleString);
            
        // This is where you can write custom rendering code. Customize this method to customize your SRP.
    }
}
