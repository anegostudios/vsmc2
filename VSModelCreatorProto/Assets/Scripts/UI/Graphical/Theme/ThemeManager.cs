using System.Collections.Generic;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager main;

    public ThemeDefinition[] themes;
    private List<ThemeSelectorBase> themedElements;

    private void Awake()
    {
        themedElements = new List<ThemeSelectorBase>();
        main = this;
    }

    private void Start()
    {
        ReloadTheme();
    }

    public void RegisterElement(ThemeSelectorBase element)
    {
        themedElements.Add(element);
    }

    public Color GetThemedColour(ThemeElementEnum elementType)
    {
        return themes[0].themeColors[(int)elementType];
    }

    public void ReloadTheme()
    {
        foreach (ThemeSelectorBase elem in themedElements)
        {
            elem.OnThemeReload();
        }
    }
}
