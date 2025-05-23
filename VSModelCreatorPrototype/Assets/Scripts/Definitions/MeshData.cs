using System;
using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    public class MeshData
    {
        public string meshName;
        public List<Vector3> vertices;
        public List<Vector2> uvs;
        public List<int> indices;
        public int jointID;
        public List<int[]>  lineIndices;

        //One texture index per vertex.
        public List<int> textureIndices;

        public Matrix4x4 storedMatrix;

        public MeshData()
        {
            meshName = "New Object";
            vertices = new List<Vector3>();
            uvs = new List<Vector2>();
            indices = new List<int>();
            textureIndices = new List<int>();
            lineIndices = new List<int[]>();
        }

        public void MatrixTransform(Matrix4x4 matrix)
        {
            storedMatrix = matrix;
        }

        public void AddToFromOther(MeshData other)
        {
            //Merges two mesh data together.
            //Unused now - Due to each shape being seperate.
            int cvc = vertices.Count;
            vertices.AddRange(other.vertices);
            uvs.AddRange(other.uvs);
            foreach (int index in other.indices)
            {
                indices.Add(index + cvc);
            }
            textureIndices.AddRange(other.textureIndices);
        }
    }
}