using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class AttachObjectToUIOverlay : MonoBehaviour
{

    public OptionSlider elementsOrAttachmentPointSlider;
    public TMP_Text overlayTitle;
    public Button applyButton;
    public Button unattachButton;

    [Header("Element Selection")]
    public GameObject elementSelectionParent;
    public GameObject matchStepParentCodeHolder;
    public TMP_Text foundStepParentText;
    public Toggle onlyShowElementsThatMatchStepParentCode;
    public GameObject nonMatchingStepParentCodeWarning;
    public GameObject rootElementInShapeDropdownHolder;
    public TMP_Dropdown rootElementInShapeDropdownList;
    public AttachmentHierarchyManagerForElements hierarchyForElements;

    [Header("Attachment Points Selection")]
    public GameObject attachmentPointSelectionParent;
    public AttachmentHierarchyManagerForAttachmentPoints hierarchyForAPs;

    public ShapeElement[] rootElemsOnOpen;
    public ShapeElement forRootElement;

    void Awake()
    {
        elementsOrAttachmentPointSlider.AddListenerOnValueChanged(ResolveShapeElementOrAttachmentPointSelection);
    }

    public void Open(ShapeElement forRootElement)
    {
        overlayTitle.text = "Attach '" + forRootElement.Name + "' To...";
        applyButton.interactable = false;
        this.forRootElement = forRootElement;
        ResolveShapeElementOrAttachmentPointSelection();
        unattachButton.interactable = forRootElement.OriginPoint != null && forRootElement.OriginPoint is not TransformOnlyOriginPoint;
        gameObject.SetActive(true);
    }

    public void ResolveShapeElementOrAttachmentPointSelection(int v = 0)
    {
        if (elementsOrAttachmentPointSlider.cSelectedValue == 0)
        {
            OpenForShapeElementSelection();
        }
        else
        {
            OpenForAttachmentPointSelection();
        }
    }

    public void OpenForShapeElementSelection()
    {
        attachmentPointSelectionParent.SetActive(false);
        elementSelectionParent.SetActive(true);
        if (forRootElement.StepParentName == null || forRootElement.StepParentName == "")
        {
            foundStepParentText.text = "No Step Parent Code Specified";
            onlyShowElementsThatMatchStepParentCode.SetIsOnWithoutNotify(false);
            nonMatchingStepParentCodeWarning.SetActive(true);
            matchStepParentCodeHolder.SetActive(false);
        }
        else
        {
            foundStepParentText.text = "Step Parent: '" + forRootElement.StepParentName + "'";
            matchStepParentCodeHolder.SetActive(true);
        }

        rootElementInShapeDropdownHolder.SetActive(!onlyShowElementsThatMatchStepParentCode.isOn);
        nonMatchingStepParentCodeWarning.SetActive(!onlyShowElementsThatMatchStepParentCode.isOn);

        rootElementInShapeDropdownList.ClearOptions();
        rootElemsOnOpen = MultiShapeManager.main.GetAllRootShapeElements(false).ToArray();
        List<string> options = new List<string>();
        foreach (ShapeElement e in rootElemsOnOpen)
        {
            options.Add(e.Name + " in " + e.parentShape.shapeLoadedFromPath);
        }
        rootElementInShapeDropdownList.AddOptions(options);
        rootElementInShapeDropdownList.SetValueWithoutNotify(0);
        hierarchyForElements.StartCreatingEntries();
    }

    public void OpenForAttachmentPointSelection()
    {
        attachmentPointSelectionParent.SetActive(true);
        elementSelectionParent.SetActive(false);
        hierarchyForAPs.StartCreatingEntries();
    }

    public void OnOnlyShowMatchingStepElementsToggleChange()
    {
        rootElementInShapeDropdownHolder.SetActive(!onlyShowElementsThatMatchStepParentCode.isOn);
        nonMatchingStepParentCodeWarning.SetActive(!onlyShowElementsThatMatchStepParentCode.isOn);
        hierarchyForElements.StartCreatingEntries();
    }

    public void OnSelectedRootElementForElemHierarchyChanged()
    {
        hierarchyForElements.StartCreatingEntries();
    }

    public ShapeElement GetCurrentlySelectedRootElementForElemHierarchy()
    {
        return rootElemsOnOpen[rootElementInShapeDropdownList.value];
    }

    public bool CheckValidityOfSelectedAttachment(ShapeElement otherSelected)
    {
        bool valid = MultiShapeManager.main.CanRootElementBeAttachedToOther(forRootElement, otherSelected);
        applyButton.interactable = valid;
        return valid;
    }

    public void OnApplyButtonClicked()
    {
        if (elementsOrAttachmentPointSlider.cSelectedValue == 0)
        {
            GenericHierarchyItemEntry sel = hierarchyForElements.GetSelected();
            if (sel == null) return;
            bool valid = MultiShapeManager.main.CanRootElementBeAttachedToOther(forRootElement, sel.storedData as ShapeElement);
            if (!valid)
            {
                //UH wha?
                applyButton.interactable = false;
                return;
            }
            MultiShapeManager.main.SetOriginOfShapeElementToOtherShapeElement(forRootElement, sel.storedData as ShapeElement);
        }
        else
        {

            GenericHierarchyItemEntry sel = hierarchyForAPs.GetSelected();
            if (sel == null) return;
            bool valid = MultiShapeManager.main.CanRootElementBeAttachedToOther(forRootElement, (sel.storedData as LoadedAP).GetElement());
            if (!valid)
            {
                //again, this shouldn't be possible.
                applyButton.interactable = false;
                return;
            }
            MultiShapeManager.main.SetOriginOfShapeElementToAttachmentPoint(forRootElement, sel.storedData as LoadedAP);
        }
        gameObject.SetActive(false);
    }

    public void OnUnattachButtonClicked()
    {
        MultiShapeManager.main.RemoveOriginOfShapeElement(forRootElement);
        gameObject.SetActive(false);
    }

}
