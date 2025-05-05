using TMPro;
using UnityEngine;
using SFB;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class ShapeTester : MonoBehaviour
{

    public TMP_Text shapeDetails;
    public TMP_Text errorDetails;
    public GameObject shapePrefab;
    public ShapeJSON shape;
    public ElementHierachyManager hierachy;

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

        //Do this on the next frame. Gives Unity time to actually destroy the old game objects if it needs to, since the earlier "Destroy" is not immediate.
        Invoke("RecalculatePolyCount", 0);
    }

    void RecalculatePolyCount()
    {
        int pCount = 0;
        foreach (MeshFilter filters in GetComponentsInChildren<MeshFilter>())
        {
            pCount += filters.mesh.triangles.Length;
        }
        if (shapeDetails != null)
        {
            shapeDetails.text = "Currently loaded " + transform.childCount + " models with a total of " + (pCount / 3) + " polygons.";
        }
    }

    void AddNewShapeFromPath(string filePath)
    {
        try
        {
            // The tesselator will be used to generate the mesh data.
            ShapeTesselator tess = new ShapeTesselator();

            //ShapeAccessor turns the shape from JSON into the appropriate JSON properties.
            // We also load textures at this point.
            shape = ShapeAccessor.DeserializeShapeFromFile(filePath);

            //Create the element hierachy.
            hierachy.StartCreatingElementPrefabs(shape);

            //VSMeshData stores a single 'box' in the modeler.
            List<VSMeshData> meshes = tess.TesselateShape(shape);

            //Debug just to show loaded textures.
            if (errorDetails != null)
            {
                errorDetails.text = "Textures:";
                foreach (var val in shape.Textures)
                {
                    errorDetails.text += "\n" + val.Key + " : " + val.Value;
                }
            }

            foreach (VSMeshData meshData in meshes)
            {
                //Clone the pre-created 'shapePrefab' Unity object. This is preconfigured with the correct materials to render the mesh.
                GameObject ch = GameObject.Instantiate(shapePrefab, transform);

                //The stored matrix gets applied.
                ch.transform.position = meshData.storedMatrix.GetPosition();
                ch.transform.rotation = meshData.storedMatrix.rotation;
                ch.transform.localScale = meshData.storedMatrix.lossyScale;

                //Unity stores meshes in the 'Mesh' class.
                Mesh unityMesh = new Mesh();
                unityMesh.SetVertices(meshData.vertices);
                unityMesh.SetUVs(0, meshData.uvs);
                unityMesh.SetTriangles(meshData.indices, 0);

                //This is a weird hack to get the materials to work. We're actually using the 2nd UV channel to store the texture index.
                // For instance, a vertex with a UV of 2.5 will use a texture index of 2. The .5 offset is to avoid rounding/flooring errors with floats.
                List<Vector2> textureIndicesV2 = new List<Vector2>();
                foreach (int i in meshData.textureIndices)
                {
                    textureIndicesV2.Add(new Vector2(i + 0.5f, i + 0.5f));
                }
                unityMesh.SetUVs(1, textureIndicesV2);

                //Automatically calculate the mesh bounds, normals, and tangents just for Unity rendering.
                unityMesh.RecalculateBounds();
                unityMesh.RecalculateNormals();
                unityMesh.RecalculateTangents();

                //Now apply the sections to the Unity object.
                ch.GetComponent<MeshFilter>().mesh = unityMesh;
                ch.GetComponent<MeshRenderer>().material.SetTexture("_AvailableTextures", shape.loadedTextures);
                ch.GetComponent<MeshCollider>().sharedMesh = unityMesh;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            if (errorDetails != null)
            {
                errorDetails.text = "Failed to add shape from path: " + filePath + " with following exception: " + e.Message;
                errorDetails.color = Color.red;
            }
        }
    }

    private void Update()
    {
        // Silly code, literally just to showcase how the elements can be moved in the program.
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                hit.collider.transform.position += Vector3.up;
            }
        }
    }


}
