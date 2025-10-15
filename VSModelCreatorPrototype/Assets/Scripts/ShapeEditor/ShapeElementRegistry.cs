using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VSMC;

public class ShapeElementRegistry : MonoBehaviour
{
    public static ShapeElementRegistry main;
    Dictionary<int, ShapeElement> shapeElementByUID;
    int prevUID = -1;

    private void Awake()
    {
        shapeElementByUID = new Dictionary<int, ShapeElement>();
        prevUID = -1;
        main = this;
    }

    private void Start()
    {
    }

    public void ClearForNewModel()
    {
        shapeElementByUID.Clear();
        prevUID = -1;
    }

    /// <summary>
    /// Registers a shape element, and returns its UID.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public int AddShapeElement(ShapeElement element)
    {
        prevUID++;
        shapeElementByUID[prevUID] = element;
        return prevUID;
    }

    public ShapeElement GetShapeElementByUID(int uid)
    {
        return shapeElementByUID[uid];
    }

    /// <summary>
    /// Finds and returns a shape element by its name.
    /// Will search all elements until one matches - Use with caution for efficiency reasons.
    /// Should return null if not found.
    /// </summary>
    public ShapeElement GetShapeElementByName(string name)
    {
        return shapeElementByUID.Values.FirstOrDefault(x => x.Name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase));
    }

    public IEnumerable<ShapeElement> GetAllShapeElements()
    {
        return shapeElementByUID.Values;
    }

    public void UnregisterShapeElement(ShapeElement element, bool doChildren = false)
    {
        shapeElementByUID.Remove(element.elementUID);
        if (doChildren && element.Children != null)
        {
            foreach (ShapeElement child in element.Children)
            {
                UnregisterShapeElement(child, true);
            }
        }
    }

    public void ReregisterShapeElement(ShapeElement element, bool doChildren = false)
    {
        shapeElementByUID.Add(element.elementUID, element);
        if (doChildren && element.Children != null)
        {
            foreach (ShapeElement child in element.Children)
            {
                ReregisterShapeElement(child, true);
            }
        }
    }
}
