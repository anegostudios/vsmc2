using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    public class SetStepparentElementOverlay : MonoBehaviour
    {

        public ModelEditor modelEditor;

        public GameObject noBackdropSelectedWarning;

        public GameObject elementPrefab;

        public TMP_Text headingText;
        public Transform hierarchyParent;
        StepParentElementOverlayUIEntry cSelected;
        public Button applyButton;
        public Button setNoParentButton;
        public int selectedUID;

        public void OpenOverlay(ShapeElement selElem)
        {
            headingText.text = "Set Step-parent of element " + selElem.Name;
            cSelected = null;
            applyButton.interactable = false;
            selectedUID = selElem.elementUID;
            foreach (Transform t in hierarchyParent)
            {
                Destroy(t.gameObject);
            }

            if (BackdropManager.main.cActiveBackdrop == null)
            {
                noBackdropSelectedWarning.SetActive(true);
                gameObject.SetActive(true);
                return;
            }

            noBackdropSelectedWarning.SetActive(false);
            foreach (ShapeElementGameObject e in BackdropManager.main.cActiveBackdrop.backdropHolder.GetGameObjects())
            {
                Instantiate(elementPrefab, hierarchyParent).GetComponent<StepParentElementOverlayUIEntry>().
                Initialize(e.element, e.element.CountParents(), this);
            }

            gameObject.SetActive(true);
        }

        public void OnElementClicked(StepParentElementOverlayUIEntry entry)
        {
            if (cSelected != null) cSelected.GetComponent<Image>().color = Color.clear;
            cSelected = entry;
            entry.GetComponent<Image>().color = new Color(1, 0.75f, 0);
            applyButton.interactable = true;
        }

        public void ApplyChanges()
        {
            modelEditor.SetStepParentElement(selectedUID, cSelected.GetComponent<StepParentElementOverlayUIEntry>().elementName.text);
            gameObject.SetActive(false);
        }

        public void SetNoParent()
        {
            modelEditor.SetStepParentElement(selectedUID, "");
            gameObject.SetActive(false);
        }

    }
}