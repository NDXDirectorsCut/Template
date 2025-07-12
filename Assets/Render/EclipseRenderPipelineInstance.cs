using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Unity.Collections;
    
public class EclipseRenderPipelineInstance : RenderPipeline
{
    // Use this variable to a reference to the Render Pipeline Asset that was passed to the constructor
    //Settings
    static RndSettings rndSettings;
    private EclipseRenderPipelineAsset renderPipelineAsset;
    
    // The constructor has an instance of the ExampleRenderPipelineAsset class as its parameter.
    public EclipseRenderPipelineInstance(EclipseRenderPipelineAsset asset,RndSettings renderSet) 
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
        renderPipelineAsset = asset;
        rndSettings = renderSet;
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

    //Lighting
    const int 
        maxDirectionalLights = 32,
        maxDirectionalShadows = 4,
        maxCascades = 4,
        maxOtherLights = 128,
        maxOtherShadows = 32;

    static int
        dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorId = Shader.PropertyToID("_DirectionalLightColors"),
        dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirections"),
		dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData"),

        otherLightCountId = Shader.PropertyToID("_OtherLightCount"),
        otherLightColorId = Shader.PropertyToID("_OtherLightColors"),
        otherLightPositionId = Shader.PropertyToID("_OtherLightPositions"),
        otherLightDirectionsId = Shader.PropertyToID("_OtherLightDirections"),
        otherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles"),
        otherLightShadowDataId = Shader.PropertyToID("_OtherLightShadowData"),

		shadowPancakingId = Shader.PropertyToID("_ShadowPancaking"),
        
        envLightId = Shader.PropertyToID("_EnvironmentLighting"),
        envReflId = Shader.PropertyToID("_EnvironmentReflection"),
        reflProbeId = Shader.PropertyToID("_ReflectionProbeArray"),
        volShdSampleId = Shader.PropertyToID("_VolumeShadowSamples"),
        volShdBlurId = Shader.PropertyToID("_VolumeShadowBlur");

    static Vector4[]
        dirLightColors = new Vector4[maxDirectionalLights],
        dirLightDirections = new Vector4[maxDirectionalLights],
		dirLightShadowData = new Vector4[maxDirectionalLights],

        otherLightColors = new Vector4[maxOtherLights],
        otherLightPositions = new Vector4[maxOtherLights],
        otherLightDirections = new Vector4[maxOtherLights],
        otherLightSpotAngles = new Vector4[maxOtherLights],
        otherLightShadowData = new Vector4[maxOtherLights];

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
                SetupDirLight(dirLightCount, i, ref light);
                dirLightCount++;
            }
            if(light.lightType == LightType.Point && otherLightCount < maxOtherLights)
            {
                SetupOtherLight(otherLightCount, i, ref light);
                otherLightCount++;
            }
            if(light.lightType == LightType.Spot && otherLightCount < maxOtherLights)
            {
                SetupOtherLight(otherLightCount, i, ref light);
                otherLightCount++;
            }
        }

        cmdBuffer.SetGlobalInt(otherLightCountId, otherLightCount);
        if(otherLightCount>0)
        {
            cmdBuffer.SetGlobalVectorArray(otherLightColorId, otherLightColors);
            cmdBuffer.SetGlobalVectorArray(otherLightPositionId, otherLightPositions);
            cmdBuffer.SetGlobalVectorArray(otherLightDirectionsId, otherLightDirections);
            cmdBuffer.SetGlobalVectorArray(otherLightSpotAnglesId, otherLightSpotAngles);
            cmdBuffer.SetGlobalVectorArray(otherLightShadowDataId, otherLightShadowData);
        }

        cmdBuffer.SetGlobalInt(dirLightCountId, dirLightCount);
        if(dirLightCount>0)
        {
            cmdBuffer.SetGlobalVectorArray(dirLightColorId, dirLightColors);
            cmdBuffer.SetGlobalVectorArray(dirLightDirectionId, dirLightDirections);
            cmdBuffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
        }

        cmdBuffer.SetGlobalFloat(envLightId, rndSettings.environmentLighting);
        cmdBuffer.SetGlobalFloat(envReflId, rndSettings.environmentReflection);
        cmdBuffer.SetGlobalInt(volShdSampleId, rndSettings.volumeShadowSamples);
        cmdBuffer.SetGlobalFloat(volShdBlurId, rndSettings.volumeShadowBlur);
    }

    void SetupDirLight(int id, int visId, ref VisibleLight light)
    {
        dirLightColors[id] = light.finalColor;
        dirLightDirections[id] = -light.localToWorldMatrix.GetColumn(2);
        dirLightShadowData[id] = SetupShdDirLight(visId,light.light);
    }

    void SetupOtherLight(int id, int visId, ref VisibleLight light)
    {
        otherLightColors[id] = light.finalColor;
        Vector4 position = light.localToWorldMatrix.GetColumn(3);
        position.w =  1f / Mathf.Max(light.range * light.range, 0.00001f);
        otherLightPositions[id] = position;
        otherLightShadowData[id] = SetupShdOthLight(visId,light.light);

        if(light.lightType == LightType.Spot)
        {
            otherLightDirections[id] = -light.localToWorldMatrix.GetColumn(2);
            Light innerLight = light.light;
            float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * innerLight.innerSpotAngle);
            float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * innerLight.spotAngle);
            float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
            otherLightSpotAngles[id] = new Vector4(
                angleRangeInv, -outerCos * angleRangeInv
            );
        }
        else
        {
            otherLightSpotAngles[id] = new Vector4(0f, 1f);
        }
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
    }

    //Shadows
    int ShdDirLightCount = 0,   
        ShdOthLightCount = 0;

    struct ShdDirLight
    {
        public int visibleLightIndex;
        public float slopeScaleBias;
        public float nearPlaneOffset;
    };

    struct ShdOthLight
    {
        public int visibleLightIndex;
        public bool spot;
        public float slopeScaleBias;
        public float normalBias;
    }

    ShdDirLight[] ShdDirLights = new ShdDirLight[maxDirectionalShadows];
    ShdOthLight[] ShdOthLights = new ShdOthLight[maxOtherShadows];

    Vector2 SetupShdDirLight(int id, Light light)
    {
        if(ShdDirLightCount < maxDirectionalShadows 
        && light.shadows != LightShadows.None
        && light.shadowStrength > 0f
        && cullingResults.GetShadowCasterBounds(id, out Bounds b))
        {
            ShdDirLights[ShdDirLightCount] = new ShdDirLight{ 
                visibleLightIndex = id,
                slopeScaleBias = light.shadowBias,
                nearPlaneOffset = light.shadowNearPlane
                };
            Vector2 v2 = new Vector2(light.shadowStrength,rndSettings.cascadeCount * ShdDirLightCount);
            ShdDirLightCount++;
            return v2;
        }
        return Vector2.zero;
    }

    Vector4 SetupShdOthLight(int id, Light light)
    {
        int tileCount = ShdOthLightCount + (light.type == LightType.Spot ? 1 : 6);
        if(tileCount < maxOtherShadows
        && light.shadows != LightShadows.None
        && light.shadowStrength >0f
        && cullingResults.GetShadowCasterBounds(id, out Bounds b))
        {
            ShdOthLights[ShdOthLightCount] = new ShdOthLight{ 
                visibleLightIndex = id,
                spot = light.type == LightType.Spot ? true : false
                };
            int isSpot = light.type == LightType.Spot ? 1 : 0;
            Vector4 v4 = new Vector4(light.shadowStrength,ShdOthLightCount, isSpot, 0);
            ShdOthLightCount = tileCount;
            return v4;
        }
        return Vector4.zero;
    }

    static int 
        dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
		dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
        dirShadowInvMatricesId = Shader.PropertyToID("_DirectionalShadowInverseMatrices"),
        cascadeCountId = Shader.PropertyToID("_CascadeCount"),
        cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
        cascadeDataId = Shader.PropertyToID("_CascadeData"),

        othShadowAtlasId = Shader.PropertyToID("_OtherShadowAtlas"),
        othShadowMatricesId = Shader.PropertyToID("_OtherShadowMatrices");

    static Vector4[] 
        cascadeCullingSpheres = new Vector4[maxCascades],
        cascadeData = new Vector4[maxCascades];
    
    Vector4 shdAtlasSize; //XY Directional //ZW Other

    static Matrix4x4[]
		dirShadowMatrices = new Matrix4x4[maxDirectionalShadows * maxCascades],
        dirShadowInvMatrices = new Matrix4x4[maxDirectionalShadows * maxCascades],
        othShadowMatrices = new Matrix4x4[maxOtherShadows];

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

    Vector2 SetTileViewport(int id, int split, float tileSize)
    {
        Vector2 offset = new Vector2(id % split, id/split);
        shdBuffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize,
        tileSize, tileSize));
        return offset;
    }


    void RenderShadows(ScriptableRenderContext context, CullingResults cullingResults)
    {
        if(ShdDirLightCount > 0)
        {
            int dirAtlasSize = (int)rndSettings.dirShadowAtlas;
            shdBuffer.GetTemporaryRT(dirShadowAtlasId, dirAtlasSize, dirAtlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

            shdBuffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            shdBuffer.ClearRenderTarget(true, false, Color.clear);
            shdBuffer.SetGlobalFloat(shadowPancakingId, 1f);

            context.ExecuteCommandBuffer(shdBuffer);
            shdBuffer.Clear();

            int tiles = ShdDirLightCount * rndSettings.cascadeCount;
            int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
            int tileSize = dirAtlasSize/split;

            for(int i=0; i<ShdDirLightCount; i++)
            {
                RenderDirShadow(i, split, tileSize, context);
            }

            int arrayCount = ShdDirLightCount*rndSettings.cascadeCount;

            for(int i=0; i<arrayCount; i++)
            {
                dirShadowInvMatrices[i] = dirShadowMatrices[i].inverse;
            }

            shdBuffer.SetGlobalInt(cascadeCountId, rndSettings.cascadeCount);
            shdBuffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
            shdBuffer.SetGlobalVectorArray(cascadeDataId, cascadeData);
            shdBuffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
            shdBuffer.SetGlobalMatrixArray(dirShadowInvMatricesId, dirShadowInvMatrices);
            context.ExecuteCommandBuffer(shdBuffer);
            shdBuffer.Clear();
        }
        else
        {
            shdBuffer.GetTemporaryRT(dirShadowAtlasId, 4, 4,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }

        if(ShdOthLightCount > 0)
        {
            //Debug.Log(ShdOthLightCount);
            int othAtlasSize = (int)rndSettings.othShadowAtlas;
            shdBuffer.GetTemporaryRT(othShadowAtlasId, othAtlasSize, othAtlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            shdBuffer.SetRenderTarget(othShadowAtlasId,RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            shdBuffer.ClearRenderTarget(true, false, Color.clear);
            shdBuffer.SetGlobalFloat(shadowPancakingId, 0f);

            context.ExecuteCommandBuffer(shdBuffer);
            shdBuffer.Clear();

            int tiles = ShdOthLightCount;
            int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
            int tileSize = othAtlasSize/split;

            for(int i=0; i<ShdOthLightCount;)
            {
                RenderOthShadow(i,split,tileSize,context);
                i+= ShdOthLights[i].spot ? 1 : 6 ;
            }
            
            shdBuffer.SetGlobalMatrixArray(othShadowMatricesId, othShadowMatrices);

            context.ExecuteCommandBuffer(shdBuffer);
            shdBuffer.Clear();
        }
        else
        {
            shdBuffer.SetGlobalTexture(othShadowAtlasId, dirShadowAtlasId);
        }
    }

    void RenderDirShadow(int id, int split, int tileSize, ScriptableRenderContext context)
    {
        //  Debug.Log(id);
        ShdDirLight light = ShdDirLights[id];
        var shadowDrawSettings = new ShadowDrawingSettings(
			cullingResults, light.visibleLightIndex,
			BatchCullingProjectionType.Orthographic
		);

        int cascadeCount = rndSettings.cascadeCount;
        int tileOffset = id * cascadeCount;
        Vector3 cascadeRatios = new Vector3(
            rndSettings.cascadeRatio1,
            rndSettings.cascadeRatio2,
            rndSettings.cascadeRatio3
        );

        for(int i=0; i<cascadeCount; i++)
        {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, i, cascadeCount, cascadeRatios, tileSize, light.nearPlaneOffset,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData
            );
            splitData.shadowCascadeBlendCullingFactor = 0f;
            shadowDrawSettings.splitData = splitData;
            if(id == 0)
            {
                Vector4 cullingSphere = splitData.cullingSphere;

                cascadeData[i].x = 1f/cullingSphere.w;
                float texelSize = 2f*cullingSphere.w / tileSize;
                cascadeData[i].y = texelSize;

                cullingSphere.w *= cullingSphere.w;
                cascadeCullingSpheres[i] = cullingSphere; 
            }
            int tileIndex = tileOffset + i;
            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(
                projectionMatrix * viewMatrix,
                SetTileViewport(tileIndex, split, tileSize), 
                split);
            shdBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

            context.ExecuteCommandBuffer(shdBuffer);
            shdBuffer.Clear();

            shdBuffer.SetGlobalDepthBias(0.2f,1f);
            context.DrawShadows(ref shadowDrawSettings);
        }

        context.ExecuteCommandBuffer(shdBuffer);
		shdBuffer.Clear();
    }

    void RenderOthShadow(int id, int split, int tileSize, ScriptableRenderContext context)
    {
        ShdOthLight light = ShdOthLights[id];
        var shadowDrawSettings = new ShadowDrawingSettings(
            cullingResults, light.visibleLightIndex,
            BatchCullingProjectionType.Perspective
        );
        if(light.spot == true)
        {
            //Debug.Log(light.visibleLightIndex);
            cullingResults.ComputeSpotShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, out Matrix4x4 viewMatrix,
                out Matrix4x4 projectionMatrix, out ShadowSplitData splitData
            );
            shadowDrawSettings.splitData = splitData;
            othShadowMatrices[id] = ConvertToAtlasMatrix(
                projectionMatrix * viewMatrix,
                SetTileViewport(id, split, tileSize),
                split
            );
            shdBuffer.SetViewProjectionMatrices(viewMatrix,projectionMatrix);
            //shdBuffer.SetGlobalDepthBias(0f, light.slopeScaleBias);

            context.ExecuteCommandBuffer(shdBuffer);
            shdBuffer.Clear();

            context.DrawShadows(ref shadowDrawSettings);

            context.ExecuteCommandBuffer(shdBuffer);
		    shdBuffer.Clear();
        }
        else if(light.spot == false)
        {
            for(int i=0; i<6; i++)
            {
                //Debug.Log(i);
                cullingResults.ComputePointShadowMatricesAndCullingPrimitives(
                    light.visibleLightIndex, (CubemapFace)i, 12,
                    out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                    out ShadowSplitData splitData
                );
                viewMatrix.m11 = -viewMatrix.m11;
                viewMatrix.m12 = -viewMatrix.m12;
                viewMatrix.m13 = -viewMatrix.m13;
                shadowDrawSettings.splitData = splitData;
                int tileId = id + i;

                othShadowMatrices[tileId] = ConvertToAtlasMatrix(
                    projectionMatrix * viewMatrix,
                    SetTileViewport(tileId, split, tileSize),
                    split
                );
                shdBuffer.SetViewProjectionMatrices(viewMatrix,projectionMatrix);

                context.ExecuteCommandBuffer(shdBuffer);
                shdBuffer.Clear();

                context.DrawShadows(ref shadowDrawSettings);
            }
        }
    }

    //PostProcessingStack postStack = new PostProcessingStack;

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
            cullingParameters.shadowDistance = rndSettings.shadowDistance;
            // Use the culling parameters to perform a cull operation, and store the results
            cullingResults = context.Cull(ref cullingParameters);

            ShdDirLightCount = 0;
            ShdOthLightCount = 0;
            SetupLighting(cullingResults);
            //SetupReflections(cullingResults);
            RenderShadows(context, cullingResults);

            // Update the value of built-in shader variables, based on the current Camera
            context.SetupCameraProperties(camera);
            cmdBuffer.ClearRenderTarget(true, false, Color.clear); // color (0,0,0,0), totally transparent

            context.ExecuteCommandBuffer(cmdBuffer);
            cmdBuffer.Clear();
            
            // Tell Unity how to sort the geometry, based on the current Camera
            var sortingSettings = new SortingSettings(camera) {
			    criteria = SortingCriteria.CommonTransparent
		    };
            // Create a DrawingSettings struct that describes which geometry to draw and how to draw it
            DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings)
            {
                perObjectData = PerObjectData.ReflectionProbes
            };
            // Tell Unity how to filter the culling results, to further specify which geometry to draw
            // Use FilteringSettings.defaultValue to specify no filtering
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);//FilteringSettings.defaultValue;
            
            // Schedule a command to draw the Skybox if required
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                context.DrawSkybox(camera);
            }

            // Schedule a command to draw the geometry, based on the settings you have defined
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

            #if UNITY_EDITOR
            if (Handles.ShouldRenderGizmos())
            {
			    context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
			    context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
		    }
            #endif

            shdBuffer.ReleaseTemporaryRT(dirShadowAtlasId);
            if(ShdOthLightCount > 0)
                shdBuffer.ReleaseTemporaryRT(othShadowAtlasId);

            context.Submit();
    }
}
