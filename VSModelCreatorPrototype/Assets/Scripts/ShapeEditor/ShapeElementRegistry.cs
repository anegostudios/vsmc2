using System.Collections.Generic;
using UnityEngine;
using VSMC;

public class ShapeElementRegistry : MonoBehaviour
{
    public static ShapeElementRegistry main;
    public Dictionary<int, ShapeElement> shapeElementByUID;
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
}
