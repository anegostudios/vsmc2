using UnityEngine;

/// <summary>
/// This is essentially a config class.
/// Heavily uses Unity's playerprefs system.
/// </summary>
public class ProgramPreferences
{

    //Camera Settings
    public static Preference<bool> InvertCameraButtons = new Preference<bool>("InvertCameraButtons", false);
    public static Preference<int> CurrentCameraMode = new Preference<int>("CurrentCameraMode", 0);

    //UI Scale. Any stored value less than 1 will trigger the default selection.
    public static Preference<float> UIScale = new Preference<float>("UIScale", 0);

    //UV Settings
    public static Preference<bool> UVShowGrid = new Preference<bool>("uvshowgrid", true);
    public static Preference<bool> UVShowLabels = new Preference<bool>("uvshowlabels", true);
    public static Preference<bool> UVShowOrientationMarkers = new Preference<bool>("uvshoworientationmarkers", true);

    //Low power
    public static Preference<bool> LowPowerMode = new Preference<bool>("enableLowPowerMode", false);

    //Asset path manager stuff.
    public static Preference<string> PreferredAssetPaths = new Preference<string>("preferredAssetPaths", "");
    public static Preference<bool> UseLocalAssetPathFirst = new Preference<bool>("useLocalAssetPathFirst", true);
    public static Preference<bool> HideFilePaths = new Preference<bool>("hideFilepaths", false);

    //Misc
    public static Preference<bool> ShowBackdropsAndAttachmentsPanel = new Preference<bool>("showBackdropsMenu", false);
    public static Preference<bool> EnableUpdateChecking = new Preference<bool>("enableUpdateChecking", false);

    //Linux file path for desktop settings.
    public static Preference<string> LastUsedVersion = new Preference<string>("LastUsedVersion", "0.0.0");
    public static Preference<string> LastUsedLaunchPath = new Preference<string>("LastUsedLaunchPath", "");

}
