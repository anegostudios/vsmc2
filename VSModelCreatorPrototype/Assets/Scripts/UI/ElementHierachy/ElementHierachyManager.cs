using UnityEngine;
using VSMC;

public class ElementHierachyManager : MonoBehaviour
{
    public GameObject elementPrefab;
    public Transform hierachyParent;

    public void StartCreatingElementPrefabs(Shape shape)
    {
        foreach (ShapeElement elem in shape.Elements)
        {
            CreateElementPrefabs(elem, 0);
        }
    }

    /// <summary>
    /// Recursively creates the UI elements based on children.
    /// </summary>
    private void CreateElementPrefabs(ShapeElement parent, int pCount)
    {
        GameObject elemUI = GameObject.Instantiate(elementPrefab, hierachyParent);
        elemUI.GetComponent<ElementHierachyItemPrefab>().InitializePrefab(parent, pCount);
        if (parent?.Children != null)
        {
            foreach (ShapeElement child in parent.Children)
            {
                CreateElementPrefabs(child, pCount + 1);
            }
        }
    }

}
