
using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class VSAnimationKeyFrame
{

    // <summary>
    /// The ID of the keyframe.
    /// </summary>
    [JsonProperty]
    public int Frame;

    /// <summary>
    /// The elements of the keyframe.
    /// </summary>
    [JsonProperty]
    public Dictionary<string, VSAnimationKeyFrameElement> Elements;

}
