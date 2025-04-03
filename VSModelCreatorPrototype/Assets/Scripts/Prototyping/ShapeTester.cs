using UnityEngine;

public class ShapeTester : MonoBehaviour
{

    //Unity fields.
    public string filePath;
    public ShapeJSON loadedShape;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadedShape = ShapeAccessor.DeserializeShapeFromFile(filePath);   
    }
}
