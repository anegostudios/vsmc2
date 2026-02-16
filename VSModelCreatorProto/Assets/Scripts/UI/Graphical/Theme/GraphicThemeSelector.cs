using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This component will set the colour of any graphic element, based on the current theme and selected element.
/// </summary>
[RequireComponent(typeof(Graphic))]
public class GraphicThemeSelector : ThemeSelectorBase
{

    public override void OnThemeReload()
    {
        GetComponent<Graphic>().color = Color();
    }


}
