// Aaron Lanterman, July 2, 2023
// Based heavily on https://catlikecoding.com/unity/tutorials/scriptable-render-pipeline/
using UnityEngine;
using UnityEngine.Rendering;

public class MyShadowPipeline : RenderPipeline {
    bool dynamicBatching, instancing;

    const int maxVisibleLights = 4;
	static int visibleLightColorsId =
		Shader.PropertyToID("_VisibleLightColors");
	static int visibleLightDirectionsOrPositionsId =
		Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
	Vector4[] visibleLightColors = new Vector4[maxVisibleLights];
	Vector4[] visibleLightDirectionsOrPositions = new Vector4[maxVisibleLights];

    RenderTexture shadowMap;
    static int shadowMapId = Shader.PropertyToID("_ShadowMap");
    static int worldToShadowMatrixId =
		Shader.PropertyToID("_WorldToShadowMatrix");
    static int shadowBiasId = Shader.PropertyToID("_ShadowBias");

    CullingResults cullingResults;

    public MyShadowPipeline(bool dynamicBatching, bool instancing) {
        this.dynamicBatching = dynamicBatching;
        this.instancing = instancing;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

	void RenderShadows (ScriptableRenderContext renderContext) {
		shadowMap = RenderTexture.GetTemporary(
			1024, 1024, 16, RenderTextureFormat.Shadowmap
		);
		shadowMap.filterMode = FilterMode.Bilinear;
		shadowMap.wrapMode = TextureWrapMode.Clamp;

        CoreUtils.SetRenderTarget(shadowBuffer, shadowMap, RenderBufferLoadAction.DontCare, 
                                  RenderBufferStoreAction.Store, ClearFlag.Depth);
        shadowBuffer.BeginSample("Render Shadows");
		renderContext.ExecuteCommandBuffer(shadowBuffer);
		shadowBuffer.Clear();

		Matrix4x4 viewMatrix, projectionMatrix;
		ShadowSplitData splitData;
		cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            0, 0, 1, Vector3.zero, 1024,
            cullingResults.visibleLights[0].light.shadowNearPlane, 
            out viewMatrix, out projectionMatrix, out splitData);
        shadowBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        shadowBuffer.SetGlobalFloat(
			shadowBiasId, cullingResults.visibleLights[0].light.shadowBias
		);
		renderContext.ExecuteCommandBuffer(shadowBuffer);
		shadowBuffer.Clear();
 
	    var shadowSettings = new ShadowDrawingSettings(cullingResults, 0);
		renderContext.DrawShadows(ref shadowSettings);

        if (SystemInfo.usesReversedZBuffer) {
			projectionMatrix.m20 = -projectionMatrix.m20;
			projectionMatrix.m21 = -projectionMatrix.m21;
			projectionMatrix.m22 = -projectionMatrix.m22;
			projectionMatrix.m23 = -projectionMatrix.m23;
		}
        var scaleOffset = Matrix4x4.TRS(
			Vector3.one * 0.5f, Quaternion.identity, Vector3.one * 0.5f
		);
        Matrix4x4 worldToShadowMatrix = scaleOffset * (projectionMatrix * viewMatrix);
		shadowBuffer.SetGlobalMatrix(worldToShadowMatrixId, worldToShadowMatrix);
        shadowBuffer.SetGlobalTexture(shadowMapId, shadowMap);
		shadowBuffer.EndSample("Render Shadows");
		renderContext.ExecuteCommandBuffer(shadowBuffer);
		shadowBuffer.Clear();
	}

    protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras) {
        foreach (var camera in cameras) {
            Render(renderContext, camera);
        }
    }

    // Create one reusable CommandBuffer object to help avoid garbage buildup
    CommandBuffer commandBuffer = new CommandBuffer {
        name = "My Render Camera"
    };

    CommandBuffer shadowBuffer = new CommandBuffer {
		name = "Render Shadows"
	};

    ShaderTagId shaderTagId = new ShaderTagId("SRPDefaultUnlit");

    private void Render(ScriptableRenderContext renderContext, Camera camera) {
         #if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView) {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
         #endif

        ScriptableCullingParameters cullingParameters;
        if (!camera.TryGetCullingParameters(out cullingParameters)) {
            return;
        }
        cullingParameters.shadowDistance = 20;
        cullingResults = renderContext.Cull(ref cullingParameters);

        int realMaxLights = Mathf.Min(cullingResults.visibleLights.Length, maxVisibleLights);
		for (int i = 0; i < realMaxLights; i++) {
			VisibleLight light = cullingResults.visibleLights[i];
            visibleLightColors[i] = light.finalColor; // finalcolor = lightcolor * intensity
            if (light.lightType == LightType.Directional) {
                Vector4 v = light.localToWorldMatrix.GetColumn(2);
			    v.x = -v.x;
			    v.y = -v.y;
			    v.z = -v.z;
			    visibleLightDirectionsOrPositions[i] = v;
            } else { // Assume light is a point light
                visibleLightDirectionsOrPositions[i] = light.localToWorldMatrix.GetColumn(3);
            }
		} 

        RenderShadows(renderContext);

        renderContext.SetupCameraProperties(camera);
        commandBuffer.ClearRenderTarget(true, false, Color.clear); // color (0,0,0,0), totally transparent
        commandBuffer.SetGlobalVectorArray(visibleLightColorsId, visibleLightColors);
		commandBuffer.SetGlobalVectorArray(visibleLightDirectionsOrPositionsId,
                                           visibleLightDirectionsOrPositions);
        renderContext.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();

        var sortingSettings = new SortingSettings(camera) {
            criteria = SortingCriteria.CommonOpaque
        };

        var drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
        drawingSettings.enableDynamicBatching = dynamicBatching;
        drawingSettings.enableInstancing = instancing;
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        renderContext.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        // can do DrawSkybox directly; other commands need command buffer
        renderContext.DrawSkybox(camera); 
        renderContext.Submit();
        
		RenderTexture.ReleaseTemporary(shadowMap);
    }
}