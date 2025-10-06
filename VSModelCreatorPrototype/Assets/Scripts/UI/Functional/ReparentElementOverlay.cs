using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class ReparentElementOverlay : MonoBehaviour
{

    public ModelEditor modelEditor;
    public GameObject elementHierarchyToCopy;
    public TMP_Text reparentElementText;
    public Transform hierarchyParent;
    GameObject clonedHierarchy;
    GameObject cSelected;
    public Button applyButton;
    int selID;

    public void OpenOverlay(ShapeElement selElem)
    {
        if (clonedHierarchy != null) DestroyImmediate(clonedHierarchy);
        clonedHierarchy = Instantiate(elementHierarchyToCopy, hierarchyParent);
        reparentElementText.text = "Reparent Element " + selElem.Name;
        this.selID = selElem.elementUID;
        cSelected = null;
        applyButton.interactable = false;

        //Iterate through all children.
        foreach (ElementHierarchyItemPrefab item in clonedHierarchy.GetComponentsInChildren<ElementHierarchyItemPrefab>(true))
        {
            item.gameObject.SetActive(true);
            item.minMaxButton.enabled = false;
            item.GetComponent<Image>().color = Color.clear;
            GameObject temp = item.gameObject;
            item.GetComponent<Button>().onClick.AddListener(() => OnElementClicked(temp));
            Destroy(item);
        }
        gameObject.SetActive(true);
    }

    public void OnElementClicked(GameObject g)
    {
        if (cSelected != null) cSelected.GetComponent<Image>().color = Color.clear;
        cSelected = g;
        //An object cannot be a child of itself, or a child of its own children...
        //Long-winded way...
        //Get the shape, if any of the children IDs match then we cannot set the child here.
        ShapeElement shape = ShapeElementRegistry.main.GetShapeElementByUID(selID);
        List<ShapeElement> toCheck = new List<ShapeElement>() { shape };

        while (toCheck.Count > 0)
        {
            //g.name is set to the exact ID of the selected object.
            if (toCheck[0].elementUID.ToString() == g.name)
            {
                applyButton.interactable = false;
                g.GetComponent<Image>().color = new Color(1, 0.35f, 0.35f);
                return;
            }
            if (toCheck[0].Children != null) toCheck.AddRange(toCheck[0].Children);
            toCheck.RemoveAt(0);
        }
        g.GetComponent<Image>().color = new Color(1, 0.75f, 0);
        applyButton.interactable = true;
    }

    public void ApplyChanges()
    {
        modelEditor.ReparentElement(selID, int.Parse(cSelected.name));
    }   

}
