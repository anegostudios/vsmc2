using TMPro;
using UnityEngine;

/// <summary>
/// Sets a font, size, and color from the config settings.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class SimpleTextConfig : UIConfig
{
    public int fontIndex;
    public bool defaultTextSize;
    public bool smallText;
    public bool largeText;
    public bool defaultColor;
    public bool alternateColor;
    

    public override void RefreshUIFromConfig(UIConfigManager config)
    {
        TMP_Text t = GetComponent<TMP_Text>();
        //Font.
        if (fontIndex == 0)
        {
            t.font = config.defaultFont;
        }
        //Size
        if (smallText) t.fontSize = config.smallTextSize;
        else if (largeText) t.fontSize = config.largeTextSize;
        else if (defaultTextSize) t.fontSize = config.defaultTextSize;
        //Color
        if (defaultColor) t.color = config.defaultTextColor;
        else if (alternateColor) t.color = config.alternateTextColor;
    }
}
