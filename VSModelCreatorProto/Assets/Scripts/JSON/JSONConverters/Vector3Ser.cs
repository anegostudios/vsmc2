using UnityEngine;

[System.Serializable]
public class Vector3Ser
{

    public float x;
    public float y;
    public float z;

    public Vector3Ser()
    { }

    public Vector3Ser(float x, float y, float z)
    {
        this.x = x; this.y = y; this.z = z;
    }

    public Vector3Ser(Vector3 vec3)
    {
        this.x = vec3.x; this.y = vec3.y; this.z = vec3.z;
    }
    
    public Vector3 ToVec3()
    {
        return new Vector3(x, y, z);
    }

}
