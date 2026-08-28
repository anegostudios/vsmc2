using TMPro;
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
        foreach (TMP_Text g in GetComponentsInChildren<TMP_Text>(true))
        {
            if (g.gameObject != gameObject)
            {
                if (state == SelectionState.Disabled)
                {
                    g.CrossFadeAlpha(0.5f, 0, true);
                }
                else
                {
                    g.CrossFadeAlpha(1, 0, true);
                }
            }
        }
        base.DoStateTransition(state, instant);
    }
}
