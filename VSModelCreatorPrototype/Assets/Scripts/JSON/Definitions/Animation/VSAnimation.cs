using Newtonsoft.Json;

[System.Serializable]
public class VSAnimation
{
    [JsonProperty]
    public int QuantityFrames;

    [JsonProperty]
    public string Name;

    [JsonProperty]
    public string Code;

    [JsonProperty]
    public int Version;

    [JsonProperty]
    public bool EaseAnimationSpeed = false;

    [JsonProperty]
    public VSAnimationKeyFrame[] KeyFrames;

    //[JsonProperty]
    //public EnumEntityActivityStoppedHandling OnActivityStopped = EnumEntityActivityStoppedHandling.Rewind;

    //[JsonProperty]
    //public EnumEntityAnimationEndHandling OnAnimationEnd = EnumEntityAnimationEndHandling.Repeat;
    
}
