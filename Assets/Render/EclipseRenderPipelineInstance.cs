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
		dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData"),

        otherLightCountId = Shader.PropertyToID("_OtherLightCount"),
        otherLightColorId = Shader.PropertyToID("_OtherLightColors"),
        otherLightPositionId = Shader.PropertyToID("_OtherLightPositions"),
        
        envLightId = Shader.PropertyToID("_EnvironmentLighting"),
        envReflId = Shader.PropertyToID("_EnvironmentReflection"),
        reflProbeId = Shader.PropertyToID("_ReflectionProbeArray");

    static Vector4[]
        dirLightColors = new Vector4[maxDirectionalLights],
        dirLightDirections = new Vector4[maxDirectionalLights],
		dirLightShadowData = new Vector4[maxDirectionalLights],

        otherLightColors = new Vector4[maxOtherLights],
        otherLightPositions = new Vector4[maxOtherLights];

    CubemapArray reflectionProbeArray = new CubemapArray(256,8,TextureFormat.RGBAHalf,true);

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
            cmdBuffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
        }

        cmdBuffer.SetGlobalFloat(envLightId, lightingSettings.environmentLighting);
        cmdBuffer.SetGlobalFloat(envReflId, lightingSettings.environmentReflection);
    }

    void SetupDirLight(int id, ref VisibleLight light)
    {
        dirLightColors[id] = light.finalColor;
        dirLightDirections[id] = -light.localToWorldMatrix.GetColumn(2);
        dirLightShadowData[id] = SetupShdDirLight(id,light.light);
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
        
        for(int i=0; i<visibleProbes.Length;i++)
        {
            var probe = visibleProbes[i];
            Texture probeTex = probe.texture;
            if(probeTex != null)
            {
                for (int mip = 0; mip < 1; mip++)
                {
                    for (int side = 0; side < 6; side++)
                    {
                        Graphics.CopyTexture(probeTex, side, mip, reflectionProbeArray, (i * 6) + side, mip);
                    }
                }
            }
        }
        cmdBuffer.SetGlobalTexture(reflProbeId,reflectionProbeArray);

        // if(visibleProbes.Length>0)
        // {
        //     //Debug.Log((visibleProbes[0].texture as Cubemap).format);
        //     //CubemapArray reflectionProbeArray = new CubemapArray(2048,8,format,true);

        //     for(int i=0; i<Mathf.Max(visibleProbes.Length,8); i++)
        //     {
        //         VisibleReflectionProbe probe = visibleProbes[i];
        //         //Cubemap probeTex = probe.texture as Cubemap;
        //         for(var face = 0; face<6; ++face)
        //         {
        //             //reflectionProbeArray.SetPixels(probe.texture,(CubemapFace)face,i);
        //         }
                
        //     }
        // }
        //cmdBuffer.SetGlobalTexture(reflProbeId,reflectionProbeArray);
    }

    //Shadows
    int ShdDirLightCount = 0;
    struct ShdDirLight
    {
        public int visibleLightIndex;
    };

    ShdDirLight[] ShdDirLights = new ShdDirLight[maxDirectionalLights];

    Vector2 SetupShdDirLight(int id, Light light)
    {
        if(ShdDirLightCount < maxDirectionalLights 
        && light.shadows != LightShadows.None
        && light.shadowStrength > 0f
        && cullingResults.GetShadowCasterBounds(id, out Bounds b))
        {
            ShdDirLights[ShdDirLightCount] = new ShdDirLight {
                visibleLightIndex = id
            };
            Vector2 v2 = new Vector2(light.shadowStrength,ShdDirLightCount);
            ShdDirLightCount++;
            return v2;
        }
        return Vector2.zero;
    }

    static int 
        dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
		dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");

    static Matrix4x4[]
		dirShadowMatrices = new Matrix4x4[maxDirectionalLights];

    Matrix4x4 ConvertToAtlasMatrix (Matrix4x4 m, Vector2 offset, int split) {
        if (SystemInfo.usesReversedZBuffer) {
			m.m20 = -m.m20;
			m.m21 = -m.m21;
			m.m22 = -m.m22;
			m.m23 = -m.m23;
		}
		float scale = 1f / split;
		m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
		m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
		m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
		m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
		m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
		m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
		m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
		m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
		m.m20 = 0.5f * (m.m20 + m.m30);
		m.m21 = 0.5f * (m.m21 + m.m31);
		m.m22 = 0.5f * (m.m22 + m.m32);
		m.m23 = 0.5f * (m.m23 + m.m33);
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

    Vector2 SetTileViewport(int id, int split, float tileSize)
    {
        Vector2 offset = new Vector2(id % split, id/split);
        shdBuffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize,
        tileSize, tileSize));
        return offset;
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
        dirShadowMatrices[id] = ConvertToAtlasMatrix(
			projectionMatrix * viewMatrix,
			SetTileViewport(id, split, tileSize), split
		);
        shdBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        context.ExecuteCommandBuffer(shdBuffer);
        shdBuffer.Clear();

        shdBuffer.SetGlobalDepthBias(1f,.25f);
        context.DrawShadows(ref shadowDrawSettings);

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
