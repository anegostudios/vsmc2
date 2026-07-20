using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIConfigManager : MonoBehaviour
{
    public static UIConfigManager main;

    [Header("Unity References")]
    public Transform uiMainCanvas;

    [Header("Config Settings - Text")]
    public TMP_FontAsset defaultFont;
    public int defaultTextSize;
    public int smallTextSize;
    public int largeTextSize;
    public Color defaultTextColor;
    public Color alternateTextColor;
    public string decimalFormatting = "0.###";

    [Header("Config Settings - Menubar")]
    [Tooltip("Menubar text font size will be this minus 6 pixels.")]
    public int menubarHeight;

    [Tooltip("The size of all scrollbars.")]
    public int scrollBarSizePx;
    public int smallScrollbarSizePx;
    public float scrollBarIncrementerSpeed;
    public float optionSliderChangeSpeed = 2;

    [Header("Config Settings - Colors")]
    public Color xAxisColor;
    public Color yAxisColor;
    public Color zAxisColor;

    void Awake()
    {
        main = this;    
    }

    public void TriggerAllUIConfigEvents()
    {
        foreach (UIConfig config in uiMainCanvas.GetComponentsInChildren<UIConfig>())
        {
            config.RefreshUIFromConfig(this);
        }
    }

}
