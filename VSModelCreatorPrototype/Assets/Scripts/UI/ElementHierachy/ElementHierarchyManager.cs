using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class ElementHierarchyManager : MonoBehaviour
{
    public static ElementHierarchyManager AnimationHierarchy;
    public static ElementHierarchyManager ElementHierarchy;

    public GameObject elementPrefab;
    public Transform hierarchyParent;
    public bool isForAnimation = false;
    public bool isMainElementHierarchy = false;

    [Header("Sprites for Elements")]
    public Sprite CollapseChildrenSprite;
    public Sprite ExpandChildrenSprite;
    public Sprite HiddenSprite;
    public Sprite ShownSprite;

    Dictionary<int, GameObject> uiElementsByUID = new Dictionary<int, GameObject>();

    private void Awake()
    {
        if (isForAnimation) AnimationHierarchy = this;
        else if (isMainElementHierarchy) ElementHierarchy = this; 
    }

    public void StartCreatingElementPrefabs(Shape shape)
    {
        //Remove existing elements.
        foreach (Transform t in hierarchyParent)
        {
            Destroy(t.gameObject);
        }
        uiElementsByUID.Clear();

        foreach (ShapeElement elem in shape.Elements)
        {
            CreateElementPrefabs(elem, 0);
            DetermineIfElementIsMinimized(elem);
        }
    }

    /// <summary>
    /// Recursively creates the UI elements based on children.
    /// </summary>
    private void CreateElementPrefabs(ShapeElement parent, int pCount)
    {
        GameObject elemUI = GameObject.Instantiate(elementPrefab, hierarchyParent);
        elemUI.GetComponent<ElementHierarchyItemPrefab>().InitializePrefab(parent, pCount, this);
        uiElementsByUID.Add(parent.elementUID, elemUI);
        if (parent?.Children != null)
        {
            foreach (ShapeElement child in parent.Children)
            {
                CreateElementPrefabs(child, pCount + 1);
            }
        }
    }

    public void DetermineIfElementIsMinimized(ShapeElement elem)
    {
        uiElementsByUID[elem.elementUID].SetActive(!elem.ShouldMinimizeInUI());
        if (elem.Children != null)
        {
            foreach (ShapeElement child in elem.Children)
            {
                DetermineIfElementIsMinimized(child);
            }
        }
    }

    public ElementHierarchyItemPrefab GetElementHierarchyItem(ShapeElement element)
    {
        if (!uiElementsByUID.ContainsKey(element.elementUID))
        {
            Debug.LogError("Trying to access element hierarchy UI element when one does not exist.");
            return null;
        }
        return uiElementsByUID[element.elementUID].GetComponent<ElementHierarchyItemPrefab>();
    }

    private void RecalculateElementsInView()
    {
        /*
        float hYPos = hierarchyParent.transform.localPosition.y;
        foreach (Transform t in hierarchyParent)
        {
            if (t.localPosition.y + hYPos > 0) t.GetComponent<HorizontalLayoutGroup>().enabled = false;
            else t.GetComponent<HorizontalLayoutGroup>().enabled = true;
        }
        */
    }

    public void OnScrollViewChanged()
    {
        //RecalculateElementsInView();
    }



}
