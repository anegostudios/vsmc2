using System;
using System.Collections.Generic;
using UnityEngine;

public class VSMeshData
{

    public List<Vector3> vertices;
    public List<Vector2> uvs;
    public List<int> indices;

    public VSMeshData()
    {
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        indices = new List<int>();
    }

    public void MatrixTransform(Matrix4x4 matrix)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = matrix.MultiplyPoint(vertices[i]);
        }
    }

    public void AddToFromOther(VSMeshData other)
    {
        int cvc = vertices.Count;
        vertices.AddRange(other.vertices);
        uvs.AddRange(other.uvs);
        foreach (int index in other.indices)
        {
            indices.Add(index + cvc);
        }
    }
}
