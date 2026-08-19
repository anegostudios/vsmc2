using TMPro;
using UnityEngine;

public class FrameRateController : MonoBehaviour
{

    public GameObject lowPowerModeToggleIcon;

    void Awake()
    {
        bool lowPowerMode = ProgramPreferences.LowPowerMode.GetValue();
        lowPowerModeToggleIcon.SetActive(lowPowerMode);
        Application.targetFrameRate = lowPowerMode ? 30 : 60;
        QualitySettings.vSyncCount = lowPowerMode ? 0 : 1; //Enable vsync on non low power mode.
        Application.runInBackground = !lowPowerMode;
    }

    public void ToggleLowPowerMode()
    {
        ProgramPreferences.LowPowerMode.SetValue(!ProgramPreferences.LowPowerMode.GetValue());
        bool lowPowerMode = ProgramPreferences.LowPowerMode.GetValue(); 
        lowPowerModeToggleIcon.SetActive(lowPowerMode);
        Application.targetFrameRate = lowPowerMode ? 30 : 60;
        QualitySettings.vSyncCount = lowPowerMode ? 0 : 1; //Enable vsync on non low power mode.
        Application.runInBackground = !lowPowerMode;
        InfoLogger.main.LogText(lowPowerMode ? "Enabled low power mode" : "Disabled low power mode");
    }

}
