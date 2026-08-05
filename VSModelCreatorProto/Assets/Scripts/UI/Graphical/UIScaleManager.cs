using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScaleManager : MonoBehaviour
{
    [Header("Unity References")]
    public CanvasScaler mainCanvasScaler;
    public GameObject scaleOverlay;
    public Slider scaleSlider;
    public TMP_InputField scaleInput;
    public Button applyButton;


    [Header("Other Vals")]    
    public int recommendedWidth;
    int highestWidth;

    private void Awake()
    {
        //Setup events.
        scaleSlider.onValueChanged.AddListener(x =>
        {
            scaleInput.SetTextWithoutNotify(x.ToString("0.00"));
        });

        scaleInput.onEndEdit.AddListener(x =>
        {
            try
            {
                scaleSlider.value = Mathf.Clamp(float.Parse(x), scaleSlider.minValue, scaleSlider.maxValue);
            }
            catch
            {
                scaleInput.SetTextWithoutNotify(scaleSlider.value.ToString("0.00"));
            }
        });

        applyButton.onClick.AddListener(() =>
        {
            SetScale(scaleSlider.value);
        });

        //Testing...
        //ProgramPreferences.UIScale.SetValue(-1);

    }

    void Start()
    {
        //Set UI Scale based on preferences.
        if (ProgramPreferences.UIScale.GetValue() < 1) //scale not set - probably first launch.
        {
            if (GetRecommendedScale() > 1)
            {
                //Default to an appropriate scale..
                SetScale(GetRecommendedScale());
            }
            return;
        }
        SetScale(ProgramPreferences.UIScale.GetValue());
    }

    public void OpenScaleSlider()
    {
        scaleSlider.minValue = 1;
        scaleSlider.maxValue = GetRecommendedScale() * 2;
        SetScale(ProgramPreferences.UIScale.GetValue());
        scaleOverlay.SetActive(true);
    }

    public void OnScrollInputFieldOrSlider(BaseEventData data)
    {
        if (Input.mouseScrollDelta.y > Mathf.Epsilon) { scaleSlider.value += Input.GetKey(KeyCode.LeftShift) ? 0.01f : 0.1f; }
        else if (Input.mouseScrollDelta.y < -Mathf.Epsilon) { scaleSlider.value -= Input.GetKey(KeyCode.LeftShift) ? 0.01f : 0.1f; }
    }

    public void SetToRecommended()
    {
        SetScale(GetRecommendedScale());
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
        mainCanvasScaler.scaleFactor = scale;
        ProgramPreferences.UIScale.SetValue(scale);
        scaleSlider.value = scale;
    }

}
