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

}
