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

    public static Preference<bool> UVShowGrid = new Preference<bool>("uvshowgrid", true);
    public static Preference<bool> UVShowLabels = new Preference<bool>("uvshowlabels", true);
    public static Preference<bool> UVShowOrientationMarkers = new Preference<bool>("uvshoworientationmarkers", true);
    public static Preference<bool> LowPowerMode = new Preference<bool>("enableLowPowerMode", false);

}
