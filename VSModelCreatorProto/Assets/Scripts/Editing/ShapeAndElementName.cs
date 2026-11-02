using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Used as an identifier for a specific element in a specific shape.
/// </summary>
[System.Serializable]
public class ShapeAndElementName
{
    public string shapeName;

    [DefaultValue(-1)]
    public int elementUID = -1;
    public string elementName;

    public void GetElement()
    {

    }

}
