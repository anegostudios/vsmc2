using System;
using Newtonsoft.Json;
using UnityEngine;
using VSMC;

/// <summary>
/// This controls the editor settings that are per-model
/// </summary>
[System.Serializable]
public class ShapeEditorSettings
{

    /// <summary>
    /// The version of the editor that was last used to save this model.
    /// </summary>
    [JsonProperty]
    public string editorVersion = "";

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
    public BackdropOrAttachmentData[] backdrops = null;

    [JsonProperty]
    public BackdropOrAttachmentData[] attachments = null;

    [JsonProperty]
    [Obsolete("Kept for backwards compatiblity, but now using 'backdrops' array.")]
    public string backDropShape = null;

    [JsonProperty]
    public string mountBackDropShape = null;

    /// <summary>
    /// Should the auto UVs sizes be rounded rather than clamped?
    /// </summary>
    [JsonProperty("roundUVs")]
    public bool roundAutoUVs = false;


}
