using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

namespace VSMC {
    /// <summary>
    /// This is a physical representation of a ShapeElement in Unity.
    /// </summary>
    public class ShapeElementGameObject : MonoBehaviour
    {
        public Shape shape;
        public ShapeElement element;

        public void InitializeElement(ShapeElement shapeElement, Shape shape)
        {
            this.shape = shape;
            this.element = shapeElement;
            element.gameObject = this;
            gameObject.name = element.meshData.meshName;
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
            gameObject.GetComponent<MeshRenderer>().material.SetTexture("_AvailableTextures", shape.loadedTextures);
            gameObject.GetComponent<MeshCollider>().sharedMesh = unityMesh;

            RegenerateSelectionLinesFromMeshData();
        }

        /// <summary>
        /// Reapplies the transforms from the mesh data's stored matrix.
        /// </summary>
        public void ReapplyTransformsFromMeshData(bool doChildren = false)
        {
            //The stored matrix gets applied.
            gameObject.transform.position = element.meshData.storedMatrix.GetPosition();
            gameObject.transform.rotation = element.meshData.storedMatrix.rotation;
            gameObject.transform.localScale = element.meshData.storedMatrix.lossyScale;

            if (doChildren && element.Children != null)
            {
                foreach (ShapeElement child in element.Children)
                {
                    child.gameObject.ReapplyTransformsFromMeshData(true);
                }
            }
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
            Destroy(linesBase.gameObject);
        }

    }
}