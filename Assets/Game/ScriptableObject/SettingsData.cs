using UnityEngine;

[CreateAssetMenu(fileName = "SettingsData", menuName = "ScriptableObjects/SettingsData", order = 1)]
public class SettingsData : ScriptableObject
{
    [Header("Graphics")]
    public RenderSettings rndSettings;
}