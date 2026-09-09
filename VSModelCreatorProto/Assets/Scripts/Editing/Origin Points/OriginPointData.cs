using System.ComponentModel;
using UnityEngine;

[System.Serializable]
public class OriginPointData
{

    public Vector3 itemTranslation;
    public Vector3 itemRotation;
    public Vector3 itemOrigin;
    public Vector3 itemScaleXYZ = Vector3.one;

    public int shapeUID;
    public string shapeElementCode;
    public string apCode;


}
