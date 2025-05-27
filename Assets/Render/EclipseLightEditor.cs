using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
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
#endif