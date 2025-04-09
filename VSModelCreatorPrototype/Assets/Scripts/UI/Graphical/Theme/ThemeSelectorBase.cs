using UnityEngine;

public abstract class ThemeSelectorBase : MonoBehaviour
{

    public ThemeElementEnum elementType;

    private void Start()
    {
        ThemeManager.main.RegisterElement(this);
        OnThemeReload();
    }

    protected Color Color()
    {
        return ThemeManager.main.GetThemedColour(elementType);
    }

    public abstract void OnThemeReload();

}
