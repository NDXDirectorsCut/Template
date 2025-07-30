using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SettingsJSON
{
    public int shadowFilter = 2;
    public int blurSamples = 4;
    public int shadowAtlasSize = 4096;
}

public class SaveSettings : MonoBehaviour
{
    public EclipseRenderPipelineAsset pipelineAsset;
    public TMP_Dropdown getAtlas;
    public TMP_Dropdown getFilter;
    public Slider getSamples;

    [SerializeField] SettingsJSON settingsData = new SettingsJSON();

    void Start()
    {
    //     //Init
        getFilter.value = (int)pipelineAsset.renderSettings.softShadowMode;
        getSamples.value = (int)pipelineAsset.renderSettings.softShadowSamples;
        getAtlas.value = ((int)pipelineAsset.renderSettings.othShadowAtlas/256)-1;
    }

    public void UpdateSettings()
    {
        pipelineAsset.renderSettings.softShadowMode = (SoftShadowMode)getFilter.value;
        pipelineAsset.renderSettings.othShadowAtlas = (MapSize)((getAtlas.value+1)*512);
        pipelineAsset.renderSettings.dirShadowAtlas = (MapSize)((getAtlas.value+1)*512);
        Debug.Log((MapSize)((getAtlas.value+1)*256));
        pipelineAsset.renderSettings.softShadowSamples = (int)getSamples.value;
    }

    public void Save()
    {
        string settings = JsonUtility.ToJson(settingsData);
        System.IO.File.WriteAllText(Application.persistentDataPath+"/Settings.json", settings);
    }
}
