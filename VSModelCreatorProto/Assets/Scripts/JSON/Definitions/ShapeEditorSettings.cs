using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// This controls the editor settings that are per-model
/// </summary>
[System.Serializable]
public class ShapeEditorSettings
{

    /// <summary>
    /// Are angles locked to 15degree intervals?
    /// </summary>
    [JsonProperty(PropertyName = "allAngles")]
    public bool unlockAllAngles = true;

    /// <summary>
    /// Is this model using entity texture mode?
    /// </summary>
    [JsonProperty]
    public bool entityTextureMode = false;

    [JsonProperty]
    public string backDropShape = null;

    [JsonProperty]
    public string mountBackDropShape = null;

    

}
