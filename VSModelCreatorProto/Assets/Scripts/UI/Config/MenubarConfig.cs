using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenubarConfig : UIConfig
{

    public override void RefreshUIFromConfig(UIConfigManager config)
    {
        LayoutElement menubar = GetComponent<LayoutElement>();
        menubar.minHeight = config.menubarHeight;
        menubar.preferredHeight = config.menubarHeight;

        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
        {
            text.fontSize = config.menubarHeight - 6;
        }

    }

}
