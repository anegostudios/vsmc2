using System;
using UnityEngine;

public class StackMatrix4
{
    Matrix4x4[] values;
    const int max = 1024;
    int count;

    public Matrix4x4 Top
    {
        get { return values[count - 1]; }
        set { values[count - 1] = value; }
    }

    public int Count
    {
        get { return count; }
    }

    public StackMatrix4(int max = 1024)
    {
        values = new Matrix4x4[max];
        for (int i = 0; i < max; i++)
        {
            values[i] = Matrix4x4.identity;
        }
    }

    public void PushIdentity()
    {
        values[count] = Matrix4x4.identity;
        count++;

        if (count >= values.Length) throw new Exception("Stack matrix overflow");
    }

    public void Push(Matrix4x4 p)
    {
        values[count] = p * Matrix4x4.identity;
        count++;

        if (count >= values.Length) throw new Exception("Stack matrix overflow");
    }

    public void Push()
    {
        values[count] = Top * Matrix4x4.identity;
        count++;

        if (count >= values.Length) throw new Exception("Stack matrix overflow");
    }


    public Matrix4x4 Pop()
    {
        Matrix4x4 ret = values[count - 1];
        count--;
        if (count < 0) throw new Exception("Stack matrix underflow");
        return ret;
    }

    public void Clear()
    {
        count = 0;
    }

    public void Rotate(double x, double y, double z)
    {
        Top *= Matrix4x4.Rotate(Quaternion.Euler((float)x, (float)y, (float)z));
    }

    public void Translate(double x, double y, double z)
    {
        Top *= Matrix4x4.Translate(new Vector3((float)x, (float)y, (float)z));
    }

    public void Scale(double x, double y, double z)
    {
        Top *= Matrix4x4.Scale(new Vector3((float)x, (float)y, (float)z));
    }
}