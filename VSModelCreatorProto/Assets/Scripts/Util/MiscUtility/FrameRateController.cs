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
        Application.runInBackground = !lowPowerMode;
    }

    public void ToggleLowPowerMode()
    {
        ProgramPreferences.LowPowerMode.SetValue(!ProgramPreferences.LowPowerMode.GetValue());
        bool lowPowerMode = ProgramPreferences.LowPowerMode.GetValue();
        lowPowerModeText.text = lowPowerMode ? "Disable Low Power Mode" : "Enable Low Power Mode";
        Application.targetFrameRate = lowPowerMode ? 30 : 60;
        Application.runInBackground = !lowPowerMode;
    }

}
