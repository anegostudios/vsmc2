using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class BackdropOrAttachmentUIEntry : MonoBehaviour
{

    public TMP_Text nameText;
    public Image backgroundImage;
    public Image isEnabledImage;

    //Backdrop-Data
    public LoadedBackdrop storedBackdrop;
    public LoadedAttachment storedAttachment;

    public void Initialize(LoadedBackdrop bd)
    {
        this.storedBackdrop = bd;
        nameText.text = bd.data.shapeFilepath;
        if (bd.data.enabled)
        {
            OnEnabled();
        }
        else
        {
            OnDisabled();
        }
    }

    public void Initialize(LoadedAttachment ad)
    {
        this.storedAttachment = ad;
        nameText.text = ad.data.shapeFilepath;
        if (ad.data.enabled)
        {
            OnEnabled();
        }
        else
        {
            OnDisabled();
        }
    }

    /// <summary>
    /// Called by UI event.
    /// </summary>
    public void OnClick()
    {
        BackdropAndAttachmentMenuManager.main.SelectBackdropOrAttachment(this);
    }

    public void OnSelected()
    {
        backgroundImage.color = BackdropAndAttachmentMenuManager.main.selectedColor;
        foreach (ShapeElementGameObject g in GetGameObjects())
        {
            foreach (LineRenderer r in g.GetComponentsInChildren<LineRenderer>())
            {
                r.enabled = true;
            }
        }
    }

    public void OnDeselected()
    {
        backgroundImage.color = BackdropAndAttachmentMenuManager.main.deselectedColor;
        foreach (ShapeElementGameObject g in GetGameObjects())
        {
            foreach (LineRenderer r in g.GetComponentsInChildren<LineRenderer>())
            {
                r.enabled = false;
            }
        }
    }

    List<ShapeElementGameObject> GetGameObjects()
    {
        if (storedBackdrop == null) return storedAttachment.attachmentHolder.GetGameObjects();
        return storedBackdrop.backdropHolder.GetGameObjects();
    }

    //Called by UI.
    public void OnIsEnabledButtonClicked()
    {
        BackdropAndAttachmentMenuManager.main.ToggleBackdropOrAttachmentEnabled(this);
    }

    public void OnEnabled()
    {
        isEnabledImage.sprite = BackdropAndAttachmentMenuManager.main.activeUIImage;
    }

    public void OnDisabled()
    {
        isEnabledImage.sprite = BackdropAndAttachmentMenuManager.main.inactiveUIImage;
    }

}
