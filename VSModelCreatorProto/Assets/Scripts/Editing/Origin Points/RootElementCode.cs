using System;
using System.ComponentModel;
using VSMC;

[System.Serializable]
public class ElementCodeInShape
{
    [DefaultValue("Current Shape")]
    public string shapePath = "Current Shape";
    public string element;

    public ElementCodeInShape()
    { }

    public ElementCodeInShape(string shapePath, string element)
    {
        this.shapePath = shapePath;
        this.element = element;
    }

    public override bool Equals(object obj)
    {
        if (obj is ElementCodeInShape c)
        {
            return c.shapePath.Equals(shapePath, StringComparison.OrdinalIgnoreCase)
            && c.element.Equals(element, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(shapePath),
            StringComparer.OrdinalIgnoreCase.GetHashCode(element));
    }

    public static bool operator ==(ElementCodeInShape obj1, ElementCodeInShape obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;
        if (ReferenceEquals(obj1, null))
            return false;
        if (ReferenceEquals(obj2, null))
            return false;
        return obj1.Equals(obj2);
    }

    public static bool operator !=(ElementCodeInShape obj1, ElementCodeInShape obj2)
    {
        return !(obj1 == obj2); 
    }

    public override string ToString()
    {
        return element + " in " + shapePath;
    }
} 