using System.Collections.Generic;
using UnityEngine;
using VSMC;

public class ElementHierarchyManager : MonoBehaviour
{
    public GameObject elementPrefab;
    public Transform hierarchyParent;

    [Header("Sprites for Elements")]
    public Sprite CollapseChildrenSprite;
    public Sprite ExpandChildrenSprite;
    public Sprite HiddenSprite;
    public Sprite ShownSprite;

    Dictionary<int, GameObject> uiElementsByUID = new Dictionary<int, GameObject>();

    public void StartCreatingElementPrefabs(Shape shape)
    {
        //Remove existing elements.
        foreach (Transform t in hierarchyParent.transform)
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



}
