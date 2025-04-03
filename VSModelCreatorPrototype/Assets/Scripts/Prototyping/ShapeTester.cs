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
        ShapeTesselator tess = new ShapeTesselator();
        VSMeshData mesh = tess.TesselateShape(loadedShape);

        Mesh unityMesh = new Mesh();
        unityMesh.SetVertices(mesh.vertices);
        unityMesh.SetUVs(0, mesh.uvs);
        unityMesh.SetTriangles(mesh.indices, 0);

        unityMesh.RecalculateBounds();
        unityMesh.RecalculateNormals();
        unityMesh.RecalculateTangents();

        GetComponent<MeshFilter>().mesh = unityMesh;

    }
}
