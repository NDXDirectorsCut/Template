// Aaron Lanterman, July 2, 2023
// Based heavily on https://catlikecoding.com/unity/tutorials/scriptable-render-pipeline/
using UnityEngine;
using UnityEngine.Rendering;

public class MyPipeline : RenderPipeline {
    bool dynamicBatching, instancing;

    const int maxVisibleLights = 4;
	static int visibleLightColorsId =
		Shader.PropertyToID("_VisibleLightColors");
	static int visibleLightDirectionsOrPositionsId =
		Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
	Vector4[] visibleLightColors = new Vector4[maxVisibleLights];
	Vector4[] visibleLightDirectionsOrPositions = new Vector4[maxVisibleLights];

    public MyPipeline(bool dynamicBatching, bool instancing) {
        this.dynamicBatching = dynamicBatching;
        this.instancing = instancing;
        GraphicsSettings.lightsUseLinearIntensity = true;
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
        var cullingResults = renderContext.Cull(ref cullingParameters);

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
            Debug.Log(visibleLightDirectionsOrPositions[i]);
		} 

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

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;

        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        renderContext.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        renderContext.Submit();
    }
}