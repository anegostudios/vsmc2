using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GenericHierarchyItemEntry : MonoBehaviour
{

    [Header("Unity Refs")]
    public GameObject emptySpace;
    public Image minMaxButton;
    public GameObject parentedObject;
    public TMP_Text itemName;

    [Header("Runtime")]
    public object storedData;
    public GenericHierarchyManager hierarchyManager;
    public GenericHierarchyItemEntry parent;
    public List<GenericHierarchyItemEntry> children;
    public bool shouldMinimize = false;
    public bool replaceSelectionWithCollapse = false;
    public bool removeOnSearch = false;

    public void Initialize(string name, object data, int parentCount, bool hasChildren, GenericHierarchyItemEntry parent, GenericHierarchyManager hierarchyManager)
    {
        this.storedData = data;
        itemName.text = name;
        this.hierarchyManager = hierarchyManager;
        emptySpace.GetComponent<LayoutElement>().preferredWidth = (parentCount - 1) * 16;
        if (!hasChildren)
        {
            minMaxButton.gameObject.SetActive(false);
        }
        parentedObject.SetActive(parentCount != 0);
        gameObject.name = name;
        children = new List<GenericHierarchyItemEntry>();
        this.parent = parent;
        if (parent != null)
        {
            parent.children.Add(this);
        }
        //Trying to set the element name width using the editor is awful, so this manually sets it after a few frames.
        Invoke("ResolveTextSize", 0.1f);
    }


    public void MatchesSearch(string s)
    {
        if (!removeOnSearch && itemName.text.Contains(s, System.StringComparison.CurrentCultureIgnoreCase))
        {
            gameObject.SetActive(true);
            parentedObject.SetActive(false);
            emptySpace.SetActive(false);
            minMaxButton.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void OnEntrySelected(bool jumpTo = false)
    {
        GetComponent<Image>().color = UIConfigManager.main.HierarchyElementSelectedColor;
        foreach (var v in GetComponentsInChildren<TMP_Text>())
        {
            v.color = UIConfigManager.main.HierarchyElementSelectedFontColor;
            v.fontMaterial = UIConfigManager.main.HierarchyElementSelectedFontMaterial;
        }
        if (jumpTo)
        {
            StartCoroutine(hierarchyManager.JumpScrollviewTo(this));
        }
    }

    public void OnEntryDeselected()
    {
        GetComponent<Image>().color = UIConfigManager.main.HierarchyElementNotSelectedColor;
        foreach (var v in GetComponentsInChildren<TMP_Text>())
        {
            v.color = UIConfigManager.main.HierarchyElementNotSelectedFontColor;
            v.fontMaterial = UIConfigManager.main.HierarchyElementNotSelectedFontMaterial;
        }
    }

    public void OnEntryMarkedAsInvalidAfterSelection()
    {
        GetComponent<Image>().color = UIConfigManager.main.HierarchyElementInvalidSelectedColor;
        foreach (var v in GetComponentsInChildren<TMP_Text>())
        {
            v.color = UIConfigManager.main.HierarchyElementInvalidSelectedFontColor;
            v.fontMaterial = UIConfigManager.main.HierarchyElementSelectedFontMaterial;
        }
    }

    void ResolveTextSize()
    {
        itemName.GetComponent<LayoutElement>().minWidth = itemName.textBounds.size.x;
    }

    public void OnElementNameClicked()
    {
        if (replaceSelectionWithCollapse)
        {
            OnCollapseOrExpandClicked();
            return;
        }
        hierarchyManager.SelectItem(this);
        
    }

    public void OnCollapseOrExpandClicked()
    {
        shouldMinimize = !shouldMinimize;
        minMaxButton.sprite = shouldMinimize ? UIConfigManager.main.HierarchyExpandChildrenSprite : UIConfigManager.main.HierarchyMinimizeChildrenSprite;
        hierarchyManager.DetermineIfElementIsMinimized(this);
    }

    public void OnScroll(BaseEventData data)
    {
        hierarchyManager.hierarchyScroll.OnScroll((PointerEventData)data);
    }

}

