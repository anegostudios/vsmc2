using TMPro;
using UnityEngine;

public class FrameRateController : MonoBehaviour
{

    public TMP_Text lowPowerModeText;

    void Awake()
    {
        bool lowPowerMode = ProgramPreferences.LowPowerMode.GetValue();
        lowPowerModeText.text = lowPowerMode ? "Disable Low Power Mode" : "Enable Low Power Mode";
        Application.targetFrameRate = lowPowerMode ? 30 : 60;
        QualitySettings.vSyncCount = lowPowerMode ? 0 : 1; //Enable vsync on non low power mode.
        Application.runInBackground = !lowPowerMode;
    }

    public void ToggleLowPowerMode()
    {
        ProgramPreferences.LowPowerMode.SetValue(!ProgramPreferences.LowPowerMode.GetValue());
        bool lowPowerMode = ProgramPreferences.LowPowerMode.GetValue();
        lowPowerModeText.text = lowPowerMode ? "Disable Low Power Mode" : "Enable Low Power Mode";
        Application.targetFrameRate = lowPowerMode ? 30 : 60;
        QualitySettings.vSyncCount = lowPowerMode ? 0 : 1; //Enable vsync on non low power mode.
        Application.runInBackground = !lowPowerMode;
        InfoLogger.main.LogText(lowPowerMode ? "Enabled low power mode" : "Disabled low power mode");
    }

}
