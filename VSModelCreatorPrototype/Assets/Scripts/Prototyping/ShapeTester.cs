using TMPro;
using UnityEngine;
using SFB;
using System.Collections.Generic;
using VSMC;
using System.IO;

public class ShapeTester : MonoBehaviour
{

    public TMP_Text shapeDetails;
    public TMP_Text errorDetails;
    public GameObject shapePrefab;
    public Shape shape;
    public ElementHierachyManager hierachy;
    public bool animate = false;

    List<MeshData> meshes;
    ClientAnimator animator = null;
    Dictionary<string, AnimationMetaData> test;

    public GameObject[] joints;
    public int maxJoints = 60;
    public GameObject animPrefab;
    public Transform animListParent;
    public Dictionary<string, AnimationMetaData> allAnimations;
    public Dictionary<string, AnimationMetaData> activeAnimations;

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

    public void OnAddShapeFromFile(bool deleteCurrent)
    {

        string[] selectedFiles = StandaloneFileBrowser.OpenFilePanel("Open Shape Files", "", "json", true);
        if (selectedFiles == null || selectedFiles.Length == 0) { return; }

        string vsPath = selectedFiles[0];
        while (!vsPath.EndsWith("assets"))
        {
            Debug.Log(vsPath);
            vsPath = Directory.GetParent(vsPath).FullName;
        }
        Shape.TexturesPath = vsPath + "//survival//textures";

        foreach (GameObject joint in joints)
        {
            foreach (Transform child in joint.transform)
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

            //Create the element hierachy.
            hierachy.StartCreatingElementPrefabs(shape);

            //Create the animator.
            if (shape.Animations != null)
            {
                animator = ClientAnimator.Create(shape.Animations, shape.Elements, shape.JointsById);

                //Create animation lists
                allAnimations = new Dictionary<string, AnimationMetaData>();
                activeAnimations = new Dictionary<string, AnimationMetaData>();
                int animID = 0;

                foreach (Transform child in animListParent)
                {
                    Destroy(child.gameObject);
                }

                foreach (var anim in shape.Animations)
                {
                    AnimationMetaData meta = new AnimationMetaData(anim.Name, anim.Code);
                    allAnimations.Add(anim.Code, meta);
                    Instantiate(animPrefab, animListParent).GetComponent<AnimationEntryPrefab>().InitializePrefab(anim.Name, anim.Code, this);
                    animID++;
                }

                animator.OnFrame(activeAnimations, 1 / 30f);
                
            }
            //VSMeshData stores a single 'box' in the modeler.
            meshes = tess.TesselateShape(shape);


            //Debug just to show loaded textures.
            if (errorDetails != null)
            {
                errorDetails.text = "Textures:";
                foreach (var val in shape.Textures)
                {
                    errorDetails.text += "\n" + val.Key + " : " + val.Value;
                }
            }
            
            foreach (MeshData meshData in meshes)
            {
                //Clone the pre-created 'shapePrefab' Unity object. This is preconfigured with the correct materials to render the mesh.
                GameObject ch = GameObject.Instantiate(shapePrefab, joints[meshData.jointID].transform);
                ch.name = meshData.meshName;

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


                //Do Object Lines
                LineRenderer linesBase = ch.GetComponentInChildren<LineRenderer>();
                foreach (int[] lineSet in meshData.lineIndices)
                {
                    //This is getting a little convoluted...
                    LineRenderer lines = Instantiate(linesBase.gameObject, ch.transform).GetComponentInChildren<LineRenderer>();

                    Vector3[] linePoses = new Vector3[lineSet.Length];
                    for (int li = 0; li < lineSet.Length; li++)
                    {
                        linePoses[li] = meshData.vertices[lineSet[li]];
                    }
                    lines.positionCount = linePoses.Length;
                    lines.SetPositions(linePoses);
                }
                Destroy(linesBase.gameObject);

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

    void ListElementJoints(ShapeElement elem)
    {
        Debug.Log("Got joint " + elem.JointId + " for elem " + elem.Name);
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
        /*
        // Silly code, literally just to showcase how the elements can be moved in the program.
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                hit.collider.transform.position += Vector3.up;
            }
        }
        */

        outlineGradVal += Time.deltaTime * outlineGradSpeed;
        outlineGradVal = outlineGradVal % 1;
        outlineMat.color = outlineGrad.Evaluate(outlineGradVal);

        if (animator != null && animate)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
             
            animator.OnFrame(activeAnimations, Time.deltaTime);


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

    public void SetAnimationPlaying(string animID, bool isPlaying)
    {
        if (isPlaying)
        {
            activeAnimations.Add(animID, allAnimations[animID]);
        }
        else
        {
            activeAnimations.Remove(animID);
        }
    }

}
