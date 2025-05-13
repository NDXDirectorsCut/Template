using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
    
public class EclipseRenderPipelineInstance : RenderPipeline
{
    //Lighting
    CommandBuffer cmdBuffer = new CommandBuffer
    {
        name = "Eclipse Render"
    };
    ShaderTagId shaderTagId = new ShaderTagId("Eclipse");

    const int 
        maxDirectionalLights = 32,
        maxOtherLights = 128;

    static int
        dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorId = Shader.PropertyToID("_DirectionalLightColors"),
        dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirections"),

        otherLightCountId = Shader.PropertyToID("_OtherLightCount"),
        otherLightColorId = Shader.PropertyToID("_OtherLightColors"),
        otherLightPositionId = Shader.PropertyToID("_OtherLightPositions"),
        
        envLightId = Shader.PropertyToID("_EnvironmentLighting"),
        envReflId = Shader.PropertyToID("_EnvironmentReflection");    

    static Vector4[]
        dirLightColors = new Vector4[maxDirectionalLights],
        dirLightDirections = new Vector4[maxDirectionalLights],

        otherLightColors = new Vector4[maxOtherLights],
        otherLightPositions = new Vector4[maxOtherLights];

    void SetupLighting(CullingResults cullingResults)
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0, otherLightCount = 0;
        for(int i=0; i<visibleLights.Length; i++)
        {
            VisibleLight light = visibleLights[i];
            if(light.lightType == LightType.Directional && dirLightCount < maxDirectionalLights)
            {
                SetupDirLight(dirLightCount, ref light);
                dirLightCount++;
            }
            if(light.lightType == LightType.Point && otherLightCount < maxOtherLights)
            {
                SetupOtherLight(otherLightCount, ref light);
                otherLightCount++;
            }
        }

        cmdBuffer.SetGlobalInt(otherLightCountId, otherLightCount);
        if(otherLightCount>0)
        {
            cmdBuffer.SetGlobalVectorArray(otherLightColorId, otherLightColors);
            cmdBuffer.SetGlobalVectorArray(otherLightPositionId, otherLightPositions);
        }

        cmdBuffer.SetGlobalInt(dirLightCountId, dirLightCount);
        if(dirLightCount>0)
        {
            cmdBuffer.SetGlobalVectorArray(dirLightColorId, dirLightColors);
            cmdBuffer.SetGlobalVectorArray(dirLightDirectionId, dirLightDirections);
        }

        cmdBuffer.SetGlobalFloat(envLightId, lightingSettings.environmentLighting);
        cmdBuffer.SetGlobalFloat(envReflId, lightingSettings.environmentReflection);
    }

    void SetupDirLight(int id, ref VisibleLight light)
    {
        dirLightColors[id] = light.finalColor;
        dirLightDirections[id] = -light.localToWorldMatrix.GetColumn(2);
    }

    void SetupOtherLight(int id,ref VisibleLight light)
    {
        otherLightColors[id] = light.finalColor;
        Vector4 position = light.localToWorldMatrix.GetColumn(3);
        position.w =  1f / Mathf.Max(light.range * light.range, 0.00001f);
        otherLightPositions[id] = position;
    }

    //Shadows
    CommandBuffer shdBuffer = new CommandBuffer
    {
        name = "Eclipse Shadow"
    };
    
    struct ShadowedDirectionalLight 
    {
		public int visibleLightIndex;
	}

	ShadowedDirectionalLight[] ShadowedDirectionalLights =
		new ShadowedDirectionalLight[maxDirectionalLights];

    int ShadowedDirectionalLightCount;
    void SetupShdDirLight(Light light )
    {
        ShadowedDirectionalLights[ShadowedDirectionalLightCount] 
        = new ShadowedDirectionalLight {
            visibleLightIndex = visibleLightIndex
        };
        ShadowedDirectionalLightCount++;
    }

    static LightingSettings lightingSettings;
    static ShadowSettings shadowSettings;

    // Use this variable to a reference to the Render Pipeline Asset that was passed to the constructor
    private EclipseRenderPipelineAsset renderPipelineAsset;
    
    // The constructor has an instance of the ExampleRenderPipelineAsset class as its parameter.
    public EclipseRenderPipelineInstance(EclipseRenderPipelineAsset asset,LightingSettings lightSet,ShadowSettings shadowSet) 
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
        renderPipelineAsset = asset;
        lightingSettings = lightSet;
        shadowSettings = shadowSet;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        // Iterate over all Cameras
        foreach (Camera camera in cameras)
        {
                CameraRender(context,camera);
        }
    }

    private void CameraRender(ScriptableRenderContext context, Camera camera)
    {
            // Get the culling parameters from the current Camera
            camera.TryGetCullingParameters(out var cullingParameters);

            // Use the culling parameters to perform a cull operation, and store the results
            var cullingResults = context.Cull(ref cullingParameters);

            SetupLighting(cullingResults);

            // Update the value of built-in shader variables, based on the current Camera
            context.SetupCameraProperties(camera);
            cmdBuffer.ClearRenderTarget(true, false, Color.clear); // color (0,0,0,0), totally transparent

            context.ExecuteCommandBuffer(cmdBuffer);
            cmdBuffer.Clear();

            
            // Tell Unity how to sort the geometry, based on the current Camera
            var sortingSettings = new SortingSettings(camera);
            // Create a DrawingSettings struct that describes which geometry to draw and how to draw it
            DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings)
            {
                perObjectData = PerObjectData.ReflectionProbes
            };
            // Tell Unity how to filter the culling results, to further specify which geometry to draw
            // Use FilteringSettings.defaultValue to specify no filtering
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            
            // Schedule a command to draw the Skybox if required
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                context.DrawSkybox(camera);
            }
            // Schedule a command to draw the geometry, based on the settings you have defined
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

            // Instruct the graphics API to perform all scheduled commands
            context.Submit();
    }
}
