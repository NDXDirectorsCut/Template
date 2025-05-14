using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
    
public class EclipseRenderPipelineInstance : RenderPipeline
{
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

    CommandBuffer cmdBuffer = new CommandBuffer
    {
        name = "Eclipse Render"
    };
    ShaderTagId shaderTagId = new ShaderTagId("Eclipse");
    
    CommandBuffer shdBuffer = new CommandBuffer
    {
        name = "Eclipse Shadow"
    };

    CullingResults cullingResults;

    //Settings
    static LightingSettings lightingSettings;
    static ShadowSettings shadowSettings;

    //Lighting
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
    
    static Texture[]
        reflectionProbeTextures = new Texture[32];

    static int reflectionTest = Shader.PropertyToID("_ReflectionTest");

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
        SetupShdDirLight(id,light.light);
    }

    void SetupOtherLight(int id,ref VisibleLight light)
    {
        otherLightColors[id] = light.finalColor;
        Vector4 position = light.localToWorldMatrix.GetColumn(3);
        position.w =  1f / Mathf.Max(light.range * light.range, 0.00001f);
        otherLightPositions[id] = position;
    }

    //Reflections
    void SetupReflections(CullingResults cullingResults)
    {
        NativeArray<VisibleReflectionProbe> visibleProbes = cullingResults.visibleReflectionProbes;
        for(int i=0; i<visibleProbes.Length; i++)
        {
            VisibleReflectionProbe probe = visibleProbes[i];
            //Debug.Log(probe.reflectionProbe.gameObject);
        }
        //cmdBuffer.SetGlobalTexture(reflectionTest,visibleProbes[0].texture);
    }

    //Shadows
    int ShdDirLightCount = 0;
    struct ShdDirLight
    {
        public int visibleLightIndex;
    };

    ShdDirLight[] ShdDirLights = new ShdDirLight[maxDirectionalLights];

    void SetupShdDirLight(int id, Light light)
    {
        if(ShdDirLightCount < maxDirectionalLights 
        && light.shadows != LightShadows.None
        && light.shadowStrength > 0f
        && cullingResults.GetShadowCasterBounds(id, out Bounds b))
        {
            ShdDirLights[ShdDirLightCount] = new ShdDirLight {
                visibleLightIndex = id
            };
            ShdDirLightCount++;
        }
    }

    static int 
        dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
		dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");

    static Matrix4x4[]
		dirShadowMatrices = new Matrix4x4[maxDirectionalLights];

    Matrix4x4 ConvertToAtlasMatrix (Matrix4x4 m, Vector2 offset, int split) {
		return m;
	}

    void RenderShadows(ScriptableRenderContext context, CullingResults cullingResults)
    {
        if(ShdDirLightCount > 0)
        {
            int dirAtlasSize = (int)shadowSettings.directional.shadowAtlas;
            shdBuffer.GetTemporaryRT(dirShadowAtlasId, dirAtlasSize, dirAtlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

            shdBuffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            shdBuffer.ClearRenderTarget(true, false, Color.clear);

            context.ExecuteCommandBuffer(shdBuffer);
            shdBuffer.Clear();

            int split = ShdDirLightCount <= 1 ? 1 : 2;
            int tileSize = dirAtlasSize/split;

            for(int i=0; i<ShdDirLightCount; i++)
            {
                RenderDirShadow(i, split, tileSize, context);
            }

            shdBuffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
            context.ExecuteCommandBuffer(shdBuffer);
            shdBuffer.Clear();
        }
        else
        {
            shdBuffer.GetTemporaryRT(dirShadowAtlasId, 4, 4,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
        
    }

    void SetTileViewport(int id, int split, float tileSize)
    {
        Vector2 offset = new Vector2(id % split, id/split);
        shdBuffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize,
        tileSize, tileSize));
    }

    void RenderDirShadow(int id, int split, int tileSize, ScriptableRenderContext context)
    {
        //  Debug.Log(id);
        ShdDirLight light = ShdDirLights[id];
        var shadowDrawSettings = new ShadowDrawingSettings(
			cullingResults, light.visibleLightIndex,
			BatchCullingProjectionType.Orthographic
		);

        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
			light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
			out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
			out ShadowSplitData splitData
		);
        shadowDrawSettings.splitData = splitData;
        SetTileViewport(id,split,tileSize);
        dirShadowMatrices[id] = projectionMatrix * viewMatrix;
        shdBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        context.ExecuteCommandBuffer(shdBuffer);
        shdBuffer.Clear();

        context.DrawShadows(ref shadowDrawSettings);

        if (SystemInfo.usesReversedZBuffer) 
        {
		    projectionMatrix.m20 = -projectionMatrix.m20;
		    projectionMatrix.m21 = -projectionMatrix.m21;
		    projectionMatrix.m22 = -projectionMatrix.m22;
			projectionMatrix.m23 = -projectionMatrix.m23;
		}
        
        context.ExecuteCommandBuffer(shdBuffer);
		shdBuffer.Clear();
    }

    void Cleanup(ScriptableRenderContext context)
    {
        
        context.ExecuteCommandBuffer(cmdBuffer);
        cmdBuffer.Clear();
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
                CameraRender(context,camera);
        }
    }

    private void CameraRender(ScriptableRenderContext context, Camera camera)
    {
            // Get the culling parameters from the current Camera
            camera.TryGetCullingParameters(out var cullingParameters);
            cullingParameters.shadowDistance = shadowSettings.shadowDistance;
            // Use the culling parameters to perform a cull operation, and store the results
            cullingResults = context.Cull(ref cullingParameters);

            ShdDirLightCount = 0;
            SetupLighting(cullingResults);
            SetupReflections(cullingResults);
            RenderShadows(context, cullingResults);

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
            shdBuffer.ReleaseTemporaryRT(dirShadowAtlasId);
            context.Submit();
    }
}
