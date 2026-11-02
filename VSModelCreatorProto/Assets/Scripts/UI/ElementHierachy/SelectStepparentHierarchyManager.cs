using UnityEngine;
using System.Collections.Generic;
using VSMC;

public class SelectStepparentHierarchyManager : GenericHierarchyManager
{

    public SetStepparentElementOverlay stepparentOverlay;

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
        
        //Create elements based on dropdown-selected root element.
        ShapeElement rootElem = stepparentOverlay.GetCurrentlySelectedRootElementForElemHierarchy();
        CreateEntriesRecursive(rootElem);
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
        if (!stepparentOverlay.CheckValidityOfSelectedAttachment(selected))
        {
            item.OnEntryMarkedAsInvalidAfterSelection();
        }
    }
}