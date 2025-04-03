using TMPro;
using UnityEngine;
using SFB;

public class ShapeTester : MonoBehaviour
{

    public TMP_Text shapeDetails;
    public TMP_Text errorDetails;
    public GameObject shapePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {    }

    public void OnAddShapeFromFile(bool deleteCurrent)
    {

        string[] selectedFiles = StandaloneFileBrowser.OpenFilePanel("Open Shape Files", "", "json", true);
        if (selectedFiles == null || selectedFiles.Length == 0) { return; }

        if (deleteCurrent)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (string s in selectedFiles)
        {
            AddNewShapeFromPath(s);
        }

        //Do this on the next frame. Gives Unity time to actually destroy the old game objects if it needs to.
        Invoke("RecalculatePolyCount", 0);
    }

    void RecalculatePolyCount()
    {
        int pCount = 0;
        foreach (MeshFilter filters in GetComponentsInChildren<MeshFilter>())
        {
            pCount += filters.mesh.vertexCount;
        }
        shapeDetails.text = "Currently loaded " + transform.childCount + " models with a total of " + pCount + " polygons.";
    }

    void AddNewShapeFromPath(string filePath)
    {
        try
        {
            ShapeTesselator tess = new ShapeTesselator();
            VSMeshData mesh = tess.TesselateShape(ShapeAccessor.DeserializeShapeFromFile(filePath));

            GameObject ch = GameObject.Instantiate(shapePrefab, transform);
            Mesh unityMesh = new Mesh();
            unityMesh.SetVertices(mesh.vertices);
            unityMesh.SetUVs(0, mesh.uvs);
            unityMesh.SetTriangles(mesh.indices, 0);

            unityMesh.RecalculateBounds();
            unityMesh.RecalculateNormals();
            unityMesh.RecalculateTangents();

            ch.GetComponent<MeshFilter>().mesh = unityMesh;

        } catch (System.Exception e)
        {
            errorDetails.text = "Failed to add shape from path: "+filePath+" with following exception: "+e.Message;
            errorDetails.color = Color.red;
        }
    }


}
