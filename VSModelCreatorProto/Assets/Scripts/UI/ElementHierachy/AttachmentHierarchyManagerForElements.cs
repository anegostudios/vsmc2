using System.Collections.Generic;
using UnityEngine;
using VSMC;

public class AttachmentHierarchyManagerForElements : GenericHierarchyManager
{

    public AttachObjectToUIOverlay attachmentOverlay;

    public override void StartCreatingEntries()
    {
        //Remove existing elements.
        foreach (Transform t in hierarchyParent)
        {
            Destroy(t.gameObject);
        }
        cSelected = null;
        isSearching = false;
        currentItems = new List<GenericHierarchyItemEntry>();
        if (attachmentOverlay.onlyShowElementsThatMatchStepParentCode.isOn)
        {
            searchValue.SetTextWithoutNotify("");
            searchValue.gameObject.SetActive(false);
            //Create only elements that match, but show all relevant root elements.
            ShapeElement[] allRoots = attachmentOverlay.rootElemsOnOpen;
            string stepParent = attachmentOverlay.forRootElement.StepParentName;
            List<ShapeElement> elemsWithStepParentCode = new List<ShapeElement>();
            foreach (ShapeElement root in allRoots)
            {
                List<ShapeElement> temp = new List<ShapeElement>();
                temp.Add(root);
                while (temp.Count > 0)
                {
                    ShapeElement e = temp[0];
                    temp.RemoveAt(0);
                    if (e.Name == stepParent)
                    {
                        elemsWithStepParentCode.Add(e);
                        break; //Elem names are unique - We can leave when we find one for this root.
                    }
                    if (e.Children != null) temp.AddRange(e.Children);
                }
            }

            foreach (ShapeElement e in elemsWithStepParentCode)
            {
                GameObject parentElemUI = GameObject.Instantiate(elementPrefab, hierarchyParent);
                GenericHierarchyItemEntry entry = parentElemUI.GetComponent<GenericHierarchyItemEntry>();
                entry.replaceSelectionWithCollapse = true;
                entry.Initialize(e.GetRootElement().Name +" in "+e.parentShape.shapeLoadedFromPath, e, 0, true, null, this);
                GameObject elemUI = GameObject.Instantiate(elementPrefab, hierarchyParent);
                GenericHierarchyItemEntry entry2 = elemUI.GetComponent<GenericHierarchyItemEntry>();
                entry2.Initialize(e.Name, e, 1, false, entry, this); 
                currentItems.Add(entry2);
            }
        }
        else
        {
            //Create elements based on dropdown-selected root element.
            searchValue.gameObject.SetActive(true);
            ShapeElement rootElem = attachmentOverlay.GetCurrentlySelectedRootElementForElemHierarchy();
            CreateEntriesRecursive(rootElem);
        }
    }

    private void CreateEntriesRecursive(ShapeElement element, int pCount = 0, GenericHierarchyItemEntry parentEntry = null)
    {
        GameObject elemUI = GameObject.Instantiate(elementPrefab, hierarchyParent);
        GenericHierarchyItemEntry entry = elemUI.GetComponent<GenericHierarchyItemEntry>();
        entry.Initialize(element.Name, element, pCount, element.Children != null && element.Children.Length != 0, parentEntry, this);
        currentItems.Add(entry);
        if (element.Children != null)
        {
            foreach (ShapeElement child in element.Children)
            {
                CreateEntriesRecursive(child, pCount + 1, entry);
            }
        }
    }

    public override void OnAnySelected(GenericHierarchyItemEntry item)
    {
        ShapeElement selected = item.storedData as ShapeElement;
        if (!attachmentOverlay.CheckValidityOfSelectedAttachment(selected))
        {
            item.OnEntryMarkedAsInvalidAfterSelection();
        }
    }
    
}
