using UnityEngine;
using UnityEngine.UI;

public class VSMCButton : Button
{
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        Color color;
        switch (state)
        {
            case SelectionState.Normal:
                color = colors.normalColor;
                break;
            case SelectionState.Highlighted:
                color = colors.highlightedColor;
                break;
            case SelectionState.Pressed:
                color = colors.pressedColor;
                break;
            case SelectionState.Selected:
                color = colors.selectedColor;
                break;
            case SelectionState.Disabled:
                color = colors.disabledColor;
                break;
            default:
                color = Color.black;
                break;
        }
        foreach (Graphic g in GetComponentsInChildren<Graphic>())
        {
            if (g.gameObject != gameObject)
            {
                g.CrossFadeColor(color, 0, true, true);
            }
        }
        base.DoStateTransition(state, instant);
    }
}
