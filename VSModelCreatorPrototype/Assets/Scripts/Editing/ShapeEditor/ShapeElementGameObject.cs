using System.Collections.Generic;
using UnityEngine;

namespace VSMC {

    /// <summary>
    /// This is a physical representation of a ShapeElement in Unity.
    /// Every shape element will have a shape element gameobject.
    /// </summary>
    public class ShapeElementGameObject : MonoBehaviour
    {
        public ShapeElement element;

        public void InitializeElement(ShapeElement shapeElement)
        {
            this.element = shapeElement;
            element.gameObject = this;
            gameObject.name = element.Name;
            if (element.ShouldHideFromView() || !element.Shade)
            {
                gameObject.SetActive(false);
            }
            RegenerateMeshFromMeshData();
            ReapplyTransformsFromMeshData();
        }

        public void RegenerateMeshFromMeshData()
        {
            //Unity stores meshes in the 'Mesh' class.
            Mesh unityMesh = new Mesh();
            unityMesh.SetVertices(element.meshData.vertices);
            unityMesh.SetUVs(0, element.meshData.uvs);
            unityMesh.SetTriangles(element.meshData.indices, 0);

            //This is a weird hack to get the materials to work. We're actually using the 2nd UV channel to store the texture index.
            // For instance, a vertex with a UV of 2.5 will use a texture index of 2. The .5 offset is to avoid rounding/flooring errors with floats.
            List<Vector2> textureIndicesV2 = new List<Vector2>();
            foreach (int i in element.meshData.textureIndices)
            {
                textureIndicesV2.Add(new Vector2(i + 0.5f, i + 0.5f));
            }
            unityMesh.SetUVs(1, textureIndicesV2);

            //Automatically calculate the mesh bounds, normals, and tangents just for Unity rendering.
            unityMesh.RecalculateBounds();
            unityMesh.RecalculateNormals();
            unityMesh.RecalculateTangents();

            //Now apply the sections to the Unity object.
            gameObject.GetComponent<MeshFilter>().mesh = unityMesh;

            //This will break if the mesh is too small (i.e. near-0 in more than one dimension)
            Vector3 mSize = unityMesh.bounds.size;
            float tinyBounds = 0.001f;
            if (mSize.x <= tinyBounds && mSize.y <= tinyBounds ||
                mSize.x <= tinyBounds && mSize.z <= tinyBounds ||
                mSize.y <= tinyBounds && mSize.z <= tinyBounds)
            {
                gameObject.GetComponent<MeshCollider>().enabled = false;
            }
            else
            {
                gameObject.GetComponent<MeshCollider>().enabled = true;
                gameObject.GetComponent<MeshCollider>().sharedMesh = unityMesh;
            }
            RefreshMaterialChoice();

            RegenerateSelectionLinesFromMeshData();
        }

        /// <summary>
        /// Reapplies the transforms from the mesh data's stored matrix.
        /// </summary>
        public void ReapplyTransformsFromMeshData(bool doChildren = false)
        {
            Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
            Matrix4x4 m = flipZ * element.meshData.storedMatrix * flipZ;

            gameObject.transform.localPosition = m.GetPosition();
            gameObject.transform.localRotation = m.rotation;
            gameObject.transform.localScale = m.lossyScale;

            if (doChildren && element.Children != null)
            {
                foreach (ShapeElement child in element.Children)
                {
                    child.gameObject.ReapplyTransformsFromMeshData(true);
                }
            }
        }

        public void ReapplyTransformsAndMeshForReparenting(bool doChildren = false)
        {
            if (element.ShouldHideFromView() || !element.Shade)
            {
                gameObject.SetActive(false);
            }
            RegenerateMeshFromMeshData();
            ReapplyTransformsFromMeshData(doChildren);
        }

        /// <summary>
        /// Regenerates the selection lines. Is called when the mesh is regenerated.
        /// </summary>
        public void RegenerateSelectionLinesFromMeshData()
        {
            //Do Object Lines
            //First remove existing lines, but always make sure there is one remaining since we need to clone it.
            LineRenderer[] lineRenderers = gameObject.GetComponentsInChildren<LineRenderer>();
            for (int i = 0; i < lineRenderers.Length - 1; i++)
            {
                DestroyImmediate(lineRenderers[i].gameObject);
            }

            //Now recreate the lines. All properties are copied from the linesBase object - Except with specific line positions.
            LineRenderer linesBase = gameObject.GetComponentInChildren<LineRenderer>();
            foreach (int[] lineSet in element.meshData.lineIndices)
            {
                LineRenderer lines = Instantiate(linesBase.gameObject, gameObject.transform).GetComponentInChildren<LineRenderer>();
                lines.gameObject.name = "Lines";
                Vector3[] linePoses = new Vector3[lineSet.Length];
                for (int li = 0; li < lineSet.Length; li++)
                {
                    linePoses[li] = element.meshData.vertices[lineSet[li]];
                }
                lines.positionCount = linePoses.Length;
                lines.SetPositions(linePoses);
            }

            //We need to have one line that exists at all times so that we can clone it.
            if (element.meshData.lineIndices.Count == 0)
            {
                linesBase.positionCount = 0;
            }
            else
            {
                Destroy(linesBase.gameObject);
            }
        }

        public Vector3 RotateFromWorldToLocalPosition(Vector3 worldVector)
        {
            return transform.rotation * worldVector;
        }

        public Vector3 RotateFromLocalToWorldPosition(Vector3 localVector)
        {
            return Quaternion.Inverse(transform.rotation) * localVector;
        }

        public void RefreshMaterialChoice()
        {
            if (gameObject == null) return;
            if (element.RenderPass == 3) gameObject.GetComponent<MeshRenderer>().material = TextureManager.main.transparentMaterial;
            else gameObject.GetComponent<MeshRenderer>().material = TextureManager.main.modelMaterial;
        }

    }
}