using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScaleManager : MonoBehaviour
{
    [Header("Unity References")]
    public CanvasScaler mainCanvasScaler;
    public GameObject scaleOverlay;
    public Slider scaleSlider;
    public TMP_Text scaleText;


    [Header("Other Vals")]    
    public int recommendedWidth;
    int highestWidth;

    private void Awake()
    {
        //Setup events.
        scaleSlider.onValueChanged.AddListener(x => { scaleText.text = "UI Scale: " + x.ToString("0.00") + "x.\nDetected width: "+ highestWidth.ToString(); 
            mainCanvasScaler.scaleFactor = x;
            ProgramPreferences.UIScale.SetValue(x);
        });

        //Testing...
        //ProgramPreferences.UIScale.SetValue(-1);

        //Set UI Scale based on preferences.
        if (ProgramPreferences.UIScale.GetValue() < 1)
        {
            if (GetRecommendedScale() > 1)
            {
                //Open the scale overlay.
                OpenScaleSlider();
            }
            return;
        }
        SetScale(ProgramPreferences.UIScale.GetValue());
    }

    public void OpenScaleSlider()
    {
        scaleSlider.minValue = 1;
        scaleSlider.maxValue = GetRecommendedScale() * 2;
        scaleOverlay.SetActive(true);
    }

    public void SetToRecommended()
    {
        scaleSlider.value = GetRecommendedScale();
    }

    void CalcHighestSupportedWidth()
    {
        //This will adapt to the width of whatever screen the window is currently on.
        highestWidth = Screen.mainWindowDisplayInfo.width;
        return;
        /* In a world of virtual upscaled resolutions, this is not an ideal solution.
        highestWidth = 0;
        foreach (Resolution r in Screen.resolutions)
        {
            if (r.width > highestWidth)
            {
                highestWidth = r.width;
            }
        }
        */
    }

    public float GetRecommendedScale()
    {
        CalcHighestSupportedWidth();
        if (highestWidth > recommendedWidth)
        {
            return highestWidth / recommendedWidth;
        }
        return 1;
    }

    public void SetScale(float scale)
    {
        if (scale < 1) return;
        scaleSlider.value = scale;
    }

}
