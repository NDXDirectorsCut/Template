using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
    
public class EclipseRenderPipelineInstance : RenderPipeline
{

    const int 
        maxDirectionalLights = 32,
        maxOtherLights = 128;

    static int
        dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorId = Shader.PropertyToID("_DirectionalLightColors"),
        dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirections"),

        otherLightCountId = Shader.PropertyToID("_OtherLightCount"),
        otherLightColorId = Shader.PropertyToID("_OtherLightColors"),
        otherLightDirectionId = Shader.PropertyToID("_OtherLightPositions");

    static Vector4[]
        dirLightColors = new Vector4[maxDirectionalLights],
        dirLightDirections = new Vector4[maxDirectionalLights],

        otherLightColors = new Vector4[maxOtherLights],
        otherLightPositions = new Vector4[maxOtherLights];

    // Use this variable to a reference to the Render Pipeline Asset that was passed to the constructor
    private EclipseRenderPipelineAsset renderPipelineAsset;
    
    // The constructor has an instance of the ExampleRenderPipelineAsset class as its parameter.
    public EclipseRenderPipelineInstance(EclipseRenderPipelineAsset asset) 
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
        renderPipelineAsset = asset;
    }
    
    ShaderTagId shaderTagId = new ShaderTagId("EclipseLightModeTag");

    void SetupLighting(CullingResults cullingResults, CommandBuffer cmdBuffer)
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0, otherLightCount = 0;
        for(int i=0; i<visibleLights.Length; i++)
        {
            VisibleLight light = visibleLights[i];
            switch (light.lightType)
            {
                case LightType.Directional:
                    if(dirLightCount < maxDirectionalLights)
                    {
                        SetupDirLight(i, ref light);
                        dirLightCount++;
                    }
                    break;
                case LightType.Point:
                    if(otherLightCount < maxOtherLights)
                    {
                        SetupOtherLight(i, ref light);
                        otherLightCount++;
                    }
                    break;
            }
        }

        cmdBuffer.SetGlobalInt(dirLightCountId, dirLightCount);
        cmdBuffer.SetGlobalVectorArray(dirLightColorId, dirLightColors);
        cmdBuffer.SetGlobalVectorArray(dirLightDirectionId, dirLightDirections);

        cmdBuffer.SetGlobalInt(otherLightCountId, otherLightCount);
        cmdBuffer.SetGlobalVectorArray(otherLightColorId, otherLightColors);
        cmdBuffer.SetGlobalVectorArray(otherLightDirectionId, otherLightPositions);
    }

    void SetupDirLight(int id, ref VisibleLight light)
    {
        dirLightColors[id] = light.finalColor;
        dirLightDirections[id] = -light.localToWorldMatrix.GetColumn(2);
    }

    void SetupOtherLight(int id,ref VisibleLight light)
    {
        otherLightColors[id] = light.finalColor;
        otherLightPositions[id] = light.localToWorldMatrix.GetColumn(3);
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        // Create and schedule a command to clear the current render target
        CommandBuffer cmdBuffer = new CommandBuffer{ name = "Enigma Render" };
        cmdBuffer.ClearRenderTarget(true,true, Color.black);
        context.ExecuteCommandBuffer(cmdBuffer);
        cmdBuffer.Release();

        // Iterate over all Cameras
        foreach (Camera camera in cameras)
        {
            // Get the culling parameters from the current Camera
            camera.TryGetCullingParameters(out var cullingParameters);

            // Use the culling parameters to perform a cull operation, and store the results
            var cullingResults = context.Cull(ref cullingParameters);

            SetupLighting(cullingResults,cmdBuffer);

            // Update the value of built-in shader variables, based on the current Camera
            context.SetupCameraProperties(camera);

            // Tell Unity how to sort the geometry, based on the current Camera
            var sortingSettings = new SortingSettings(camera);

            // Create a DrawingSettings struct that describes which geometry to draw and how to draw it
            DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);

            // Tell Unity how to filter the culling results, to further specify which geometry to draw
            // Use FilteringSettings.defaultValue to specify no filtering
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
        
            // Schedule a command to draw the geometry, based on the settings you have defined
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

            // Schedule a command to draw the Skybox if required
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                context.DrawSkybox(camera);
            }

            // Instruct the graphics API to perform all scheduled commands
            context.Submit();
        }
    }
}
