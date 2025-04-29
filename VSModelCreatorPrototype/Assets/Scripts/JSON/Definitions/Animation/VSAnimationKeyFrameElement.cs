using Newtonsoft.Json;

[System.Serializable]
public class VSAnimationKeyFrameElement
{
    [JsonProperty]
    public double? OffsetX = null;
    [JsonProperty]
    public double? OffsetY = null;
    [JsonProperty]
    public double? OffsetZ = null;

    [JsonProperty]
    public double? StretchX = null;
    [JsonProperty]
    public double? StretchY = null;
    [JsonProperty]
    public double? StretchZ = null;
    [JsonProperty]
    public double? RotationX = null;
    [JsonProperty]
    public double? RotationY = null;
    [JsonProperty]
    public double? RotationZ = null;
    [JsonProperty]
    public double? OriginX = null;
    [JsonProperty]
    public double? OriginY = null;
    [JsonProperty]
    public double? OriginZ = null;
    [JsonProperty]
    public bool RotShortestDistanceX = false;
    [JsonProperty]
    public bool RotShortestDistanceY = false;
    [JsonProperty]
    public bool RotShortestDistanceZ = false;
}
