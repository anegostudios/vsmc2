using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class APUIEntry : MonoBehaviour
{

    public TMP_Text nameText;
    public Image backgroundImage;
    public LoadedAP ap;

    public void Initialize(LoadedAP ap)
    {
        this.ap = ap;
        nameText.text = ap.code;
        ap.uiEntry = this;
    }

    /// <summary>
    /// Called by UI event.
    /// </summary>
    public void OnClick()
    {
        AttachmentPointsManager.main.SelectAttachmentPoint(ap);
    }

    public void OnSelected()
    {
        backgroundImage.color = BackdropAndAttachmentMenuManager.main.selectedColor;
    }

    public void OnDeselected()
    {
        backgroundImage.color = BackdropAndAttachmentMenuManager.main.deselectedColor;
    }

}
