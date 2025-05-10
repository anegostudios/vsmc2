using TMPro;
using UnityEngine;
using SFB;
using System.Collections.Generic;
using VSMC;

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

    public GameObject animPrefab;
    public Transform animListParent;
    public Dictionary<string, AnimationMetaData> allAnimations;
    public Dictionary<string, AnimationMetaData> activeAnimations;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

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
                foreach (var anim in shape.Animations)
                {
                    AnimationMetaData meta = new AnimationMetaData(anim.Name, anim.Code);
                    allAnimations.Add(anim.Code, meta);
                    Instantiate(animPrefab, animListParent).GetComponent<AnimationEntryPrefab>().InitializePrefab(anim.Name, anim.Code, this);
                    animID++;
                }

                animator.OnFrame(activeAnimations, 1 / 30f);
                foreach (var mat in animator.Matrices)
                {
                    Debug.Log(mat);
                }
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
                GameObject ch = GameObject.Instantiate(shapePrefab, transform);

                //The anim matrix needs to be rotated by the model rotation...
                Matrix4x4 animMatrix = animator.Matrices[meshData.jointID];
                List<Vector3> newVertices = new List<Vector3>();
                for (int i = 0; i < meshData.vertices.Count; i++)
                {
                    newVertices.Add(animMatrix.MultiplyPoint(meshData.storedMatrix.MultiplyPoint(meshData.vertices[i])));
                }

                //The stored matrix gets applied.
                //ch.transform.position = meshData.storedMatrix.GetPosition();
                //ch.transform.rotation = meshData.storedMatrix.rotation;
                //ch.transform.localScale = meshData.storedMatrix.lossyScale;

                //Unity stores meshes in the 'Mesh' class.
                Mesh unityMesh = new Mesh();
                unityMesh.SetVertices(newVertices);
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
        // Silly code, literally just to showcase how the elements can be moved in the program.
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                hit.collider.transform.position += Vector3.up;
            }
        }

        if (animator != null && animate)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
             
            animator.OnFrame(activeAnimations, Time.deltaTime / 2);

            foreach (MeshData meshData in meshes)
            {
                //Clone the pre-created 'shapePrefab' Unity object. This is preconfigured with the correct materials to render the mesh.
                GameObject ch = GameObject.Instantiate(shapePrefab, transform);

                //The anim matrix needs to be rotated by the model rotation...
                Matrix4x4 animMatrix = animator.Matrices[meshData.jointID];
                List<Vector3> newVertices = new List<Vector3>();
                for (int i = 0; i < meshData.vertices.Count; i++)
                {
                    newVertices.Add(animMatrix.MultiplyPoint(meshData.storedMatrix.MultiplyPoint(meshData.vertices[i])));
                }

                //The stored matrix gets applied.
                //ch.transform.position = meshData.storedMatrix.GetPosition();
                //ch.transform.rotation = meshData.storedMatrix.rotation;
                //ch.transform.localScale = meshData.storedMatrix.lossyScale;

                //Unity stores meshes in the 'Mesh' class.
                Mesh unityMesh = new Mesh();
                unityMesh.SetVertices(newVertices);
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
