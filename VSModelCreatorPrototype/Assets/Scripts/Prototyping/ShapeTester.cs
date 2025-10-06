using TMPro;
using UnityEngine;
using SFB;
using System.Collections.Generic;
using VSMC;
using System.IO;
public class ShapeTester : MonoBehaviour
{

    public ShapeHolder shapeHolder;

    public TMP_Text shapeDetails;
    public TMP_Text errorDetails;
    public GameObject shapePrefab;
    public Shape shape;
    public bool animate = false;

    List<MeshData> meshes;
    ClientAnimator animator = null;
    Dictionary<string, AnimationMetaData> test;

    public GameObject[] joints;
    public int maxJoints = 60;

    public Material outlineMat;
    public Gradient outlineGrad;
    public float outlineGradSpeed = 1;
    float outlineGradVal = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        joints = new GameObject[maxJoints];
        for (int i = 0; i < maxJoints; i++)
        {
            joints[i] = new GameObject("Joint " + i);
        }
    }

    public void CreateNewShape()
    {

    }

    public void OnAddShapeFromFile(bool deleteCurrent)
    {

        string[] selectedFiles = StandaloneFileBrowser.OpenFilePanel("Open Shape Files", "", "json", true);
        if (selectedFiles == null || selectedFiles.Length == 0) { return; }

        //Guess the texture path from the file.
        string vsPath = selectedFiles[0];
        while (!vsPath.EndsWith("assets") && Directory.GetParent(vsPath) != null)
        {
            vsPath = Directory.GetParent(vsPath).FullName;
        }
        if (Directory.GetParent(vsPath) == null)
        {
            TextureManager.main.textureBasePath = "";
        }
        else
        {
            TextureManager.main.textureBasePath = Path.Combine(vsPath, "survival", "textures");
        }

        foreach (GameObject joint in joints)
        {
            foreach (Transform child in joint.transform)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (string s in selectedFiles)
        {
            //AddNewShapeFromPath(s);
            ShapeLoader.main.LoadShape(s);
        }

        //Do this on the next frame. Gives Unity time to actually destroy the old game objects if it needs to, since the earlier "Destroy" is not immediate.
        Invoke("RecalculatePolyCount", 0);
    }

    public void SaveShapeFile()
    {
        string saveTo = StandaloneFileBrowser.SaveFilePanel("Save file path", "", "shape.json", "json");
        if (saveTo == null || saveTo.Length < 1)
        {
            return;
        }
        ShapeAccessor.SerializeShapeToFile(ShapeLoader.main.shapeHolder.cLoadedShape, saveTo);
        Debug.Log("Exported successfully.");
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

            
            shape.InitForAnimations("root");
            if (shape.Animations != null)
            {
                foreach (VSMC.Animation i in shape.Animations)
                {
                    Debug.Log("Loaded animation: " + i.Name);
                }
            }
            foreach (var joint in shape.JointsById)
            {
                Debug.Log("Loaded joint for element " + joint.Value.Element.Name + " with ID " + joint.Key);
            }
            foreach (var elem in shape.Elements)
            {
                ListElementJoints(elem);
            }

            //Create the element hierarchy.
            //hierarchy.StartCreatingElementPrefabs(shape);

            //Create the animator.
            
            //VSMeshData stores a single 'box' in the modeler.
            ShapeTesselator.TesselateShape(shape);
            //CreateShapes(shape.Elements);
            
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

    void ListElementJoints(ShapeElement elem)
    {
        if (elem.Children != null)
        {
            foreach (ShapeElement t in elem.Children)
            {
                ListElementJoints(t);
            }
        }
    }

    private void Update()
    {
        outlineGradVal += Time.deltaTime * outlineGradSpeed;
        outlineGradVal = outlineGradVal % 1;
        outlineMat.color = outlineGrad.Evaluate(outlineGradVal);

        if (animator != null && animate)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
             


            int maxVal = Mathf.Min(animator.Matrices.Length, maxJoints);
            for (int i = 0; i < maxVal; i++)
            {
                GameObject joint = joints[i];
                joint.transform.position = animator.Matrices[i].GetPosition();
                joint.transform.rotation = animator.Matrices[i].rotation;
                joint.transform.localScale = animator.Matrices[i].lossyScale;
            }
        }
    }

    

}
