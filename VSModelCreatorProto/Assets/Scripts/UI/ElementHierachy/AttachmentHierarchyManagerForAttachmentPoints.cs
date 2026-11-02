using System;
using System.Collections.Generic;
using UnityEngine;
using VSMC;

public class AttachmentHierarchyManagerForAttachmentPoints : GenericHierarchyManager
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
        currentItems = new System.Collections.Generic.List<GenericHierarchyItemEntry>();

        List<LoadedAP> aps = AttachmentPointsManager.main.GetAllAttachmentPointsIncludingOtherShapes();
        Dictionary<ShapeElement, List<LoadedAP>> rootElemToAPs = new Dictionary<ShapeElement, List<LoadedAP>>();
        foreach (LoadedAP ap in aps)
        {
            ShapeElement r = ap.GetElement().GetRootElement();
            if (!rootElemToAPs.ContainsKey(r))
            {
                rootElemToAPs[r] = new List<LoadedAP>();
            }
            rootElemToAPs[r].Add(ap);
        }
        foreach (var v in rootElemToAPs)
        {
            GameObject parentElemUI = GameObject.Instantiate(elementPrefab, hierarchyParent);
            GenericHierarchyItemEntry entry = parentElemUI.GetComponent<GenericHierarchyItemEntry>();
            entry.replaceSelectionWithCollapse = true;
            entry.Initialize(v.Key.Name + " in " + v.Key.parentShape.shapeLoadedFromPath, null, 0, true, null, this);
            foreach (LoadedAP ap in v.Value)
            {
                GameObject elemUI = GameObject.Instantiate(elementPrefab, hierarchyParent);
                GenericHierarchyItemEntry entry2 = elemUI.GetComponent<GenericHierarchyItemEntry>();
                entry2.Initialize(ap.code, ap, 1, false, entry, this);
                currentItems.Add(entry2);
            }
        }
    }

    public override void OnAnySelected(GenericHierarchyItemEntry item)
    {
        if (item.storedData == null) return;
        LoadedAP selected = item.storedData as LoadedAP;
        if (!attachmentOverlay.CheckValidityOfSelectedAttachment(selected.GetElement()))
        {
            item.OnEntryMarkedAsInvalidAfterSelection();
        }
    }

}
