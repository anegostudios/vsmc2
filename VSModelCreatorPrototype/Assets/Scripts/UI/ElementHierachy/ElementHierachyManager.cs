using UnityEngine;

public class ElementHierachyManager : MonoBehaviour
{
    public GameObject elementPrefab;
    public Transform hierachyParent;

    public void StartCreatingElementPrefabs(ShapeJSON shape)
    {
        foreach (ShapeElementJSON elem in shape.Elements)
        {
            CreateElementPrefabs(elem, 0);
        }
    }

    /// <summary>
    /// Recursively creates the UI elements based on children.
    /// </summary>
    private void CreateElementPrefabs(ShapeElementJSON parent, int pCount)
    {
        GameObject elemUI = GameObject.Instantiate(elementPrefab, hierachyParent);
        elemUI.GetComponent<ElementHierachyItemPrefab>().InitializePrefab(parent, pCount);
        if (parent?.Children != null)
        {
            foreach (ShapeElementJSON child in parent.Children)
            {
                CreateElementPrefabs(child, pCount + 1);
            }
        }
    }

}
