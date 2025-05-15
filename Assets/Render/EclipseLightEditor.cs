using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditorForRenderPipeline(typeof(Light), typeof(EclipseRenderPipelineAsset))]
public class EclipseLightEditor : LightEditor 
{
    public override void OnInspectorGUI() {
		base.OnInspectorGUI();
        if(!settings.lightType.hasMultipleDifferentValues
            && (LightType)settings.lightType.enumValueIndex == LightType.Spot)
        {
            settings.DrawInnerAndOuterSpotAngle();
            settings.ApplyModifiedProperties();
        }
	}
}