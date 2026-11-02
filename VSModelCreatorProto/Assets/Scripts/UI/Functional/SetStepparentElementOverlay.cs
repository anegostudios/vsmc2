using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    public class SetStepparentElementOverlay : MonoBehaviour
    {

        public ModelEditor modelEditor;
        public TMP_Text headingText;
        public GameObject noOtherRootObjectsWarning;
        public GameObject attachmentOrderWarning;
        public Button applyButton;
        public int selectedUID;
        public TMP_Dropdown rootElementInShapeDropdownList;
        public SelectStepparentHierarchyManager elemHierarchy;
        public ShapeElement[] rootElemsOnOpen;


        public void OpenOverlay(ShapeElement selElem)
        {
            headingText.text = "Set Step-parent of element " + selElem.Name;
            applyButton.interactable = false;
            selectedUID = selElem.elementUID;

            rootElementInShapeDropdownList.ClearOptions();
            rootElemsOnOpen = MultiShapeManager.main.GetAllRootShapeElements(false).ToArray();
            List<string> options = new List<string>();
            foreach (ShapeElement e in rootElemsOnOpen)
            {
                if (e == selElem) continue;
                options.Add(e.Name + " in " + e.parentShape.shapeLoadedFromPath);
            }
            rootElemsOnOpen = rootElemsOnOpen.Remove(selElem);
            rootElementInShapeDropdownList.AddOptions(options);

            attachmentOrderWarning.SetActive(false);
            if (options.Count == 0)
            {
                noOtherRootObjectsWarning.SetActive(true);
                gameObject.SetActive(true);
                elemHierarchy.gameObject.SetActive(false);
                return;
            }

            elemHierarchy.gameObject.SetActive(true);
            noOtherRootObjectsWarning.SetActive(false);
            rootElementInShapeDropdownList.SetValueWithoutNotify(0);
        

            elemHierarchy.StartCreatingEntries();
            gameObject.SetActive(true);
        }


        public void OnSelectedRootElementForElemHierarchyChanged()
        {
            elemHierarchy.StartCreatingEntries();
        }

        public ShapeElement GetCurrentlySelectedRootElementForElemHierarchy()
        {
            return rootElemsOnOpen[rootElementInShapeDropdownList.value];
        }

        public bool CheckValidityOfSelectedAttachment(ShapeElement otherSelected)
        {
            ShapeElement toChange = ShapeElementRegistry.main.GetShapeElementByUID(selectedUID);
            ShapeElement t = ShapeElementRegistry.main.GetShapeElementByName(otherSelected.Name);
            bool valid = t == null || t.GetRootElement() != toChange;
            valid = valid && MultiShapeManager.main.CanRootElementBeAttachedToOther(toChange, otherSelected);
            if (t != null)
            {
                valid = valid && MultiShapeManager.main.CanRootElementBeAttachedToOther(toChange, t);
            }
            applyButton.interactable = valid;
            attachmentOrderWarning.SetActive(!valid);
            return valid;
        }

        public void ApplyChanges()
        {
            GenericHierarchyItemEntry sel = elemHierarchy.GetSelected();
            if (sel == null) return;
            modelEditor.SetStepParentElement(selectedUID, (sel.storedData as ShapeElement).Name);
            gameObject.SetActive(false);
        }

        public void SetNoParent()
        {
            modelEditor.SetStepParentElement(selectedUID, "");
            gameObject.SetActive(false);
        }

    }
}