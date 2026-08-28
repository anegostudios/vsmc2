using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace VSMC
{
    public class SetAPParentOverlay : MonoBehaviour
    {
        public GameObject elementPrefab;
        LoadedAP storedAP;
        public TMP_Text headingText;
        public Transform hierarchyParent;
        SetAPParentOverlayUIEntry cSelected;
        public Button applyButton;

        public void OpenOverlay(LoadedAP selAP)
        {
            headingText.text = "Set Parent of AP " + selAP.code;
            cSelected = null;
            storedAP = selAP;
            applyButton.interactable = false;
            foreach (Transform t in hierarchyParent)
            {
                Destroy(t.gameObject);
            }

            foreach (ShapeElement e in ShapeElementRegistry.main.GetAllShapeElements())
            {
                Instantiate(elementPrefab, hierarchyParent).GetComponent<SetAPParentOverlayUIEntry>().
                Initialize(e, e.CountParents(), this);
            }

            gameObject.SetActive(true);
        }

        public void OnElementClicked(SetAPParentOverlayUIEntry entry)
        {
            if (cSelected != null) cSelected.GetComponent<Image>().color = Color.clear;
            cSelected = entry;
            entry.GetComponent<Image>().color = new Color(1, 0.75f, 0);
            applyButton.interactable = true;
        }

        public void ApplyChanges()
        {
            AttachmentPointsManager.main.SetAPParent(storedAP, ShapeElementRegistry.main.GetShapeElementByUID(cSelected.elemUID));
            gameObject.SetActive(false);
        }

    }
}