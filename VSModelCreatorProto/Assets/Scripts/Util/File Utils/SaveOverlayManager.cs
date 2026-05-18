using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VSMC;

public class SaveOverlayManager : MonoBehaviour
{

    public static SaveOverlayManager main;

    public GameObject saveOverlay;
    public TMP_Text saveOverlayTitle;
    public TMP_Text saveOverlayBody;
    
    UnityAction currentOnCancel;
    UnityAction currentOnSaveOrDiscard;
    bool hasCloseBeenConfirmed = false;

    void Awake()
    {
        main = this;
        Application.wantsToQuit += OnWantsToQuit;
    }

    public void OpenSaveOverlayWithFunctions(UnityAction onCancel, UnityAction onSaveOrDiscard, string titleText, string bodyTextP1, bool addConfirmText = true)
    {
        saveOverlayTitle.text = titleText;
        saveOverlayBody.text = bodyTextP1 + (addConfirmText ? "\n\nDo you want to save?" : "");
        currentOnCancel = onCancel;
        currentOnSaveOrDiscard = onSaveOrDiscard;
        saveOverlay.SetActive(true);
    }

    public void OnCancel()
    {
        saveOverlay.SetActive(false);
        CallOnCancel();
    }

    public void OnSave()
    {
        ShapeLoader.main.SaveShapeToStoredPath();
        saveOverlay.SetActive(false);
        CallOnSaveOrDiscard();
    }

    public void OnSaveAs()
    {
        ShapeLoader.main.SaveShapeWithFileSelect();
        saveOverlay.SetActive(false);
        CallOnSaveOrDiscard();
    }

    public void OnDiscard()
    {
        saveOverlay.SetActive(false);
        CallOnSaveOrDiscard();
    }

    void CallOnCancel()
    {
        if (currentOnCancel != null) currentOnCancel.Invoke();
    }

    void CallOnSaveOrDiscard()
    {
        if (currentOnSaveOrDiscard != null) currentOnSaveOrDiscard.Invoke();
    }

    //Wants to quit logic.
    bool OnWantsToQuit()
    {
        if (hasCloseBeenConfirmed || ShapeHolder.CurrentLoadedShape == null)
        {
            return true;
        }
        OpenSaveOverlayWithFunctions(null, ConfirmClose, "Close VSMC2?", "Would you like to save before closing?", false);
        return false;
    }

    void ConfirmClose()
    {
        hasCloseBeenConfirmed = true;
        Application.Quit();
    }
    
}
