using UnityEngine;
using VSMC;

/// <summary>
/// An origin point that does not attach to anything - It is just a transformation.
/// </summary>
public class TransformOnlyOriginPoint : IOriginPoint
{

    public override Matrix4x4 GetDefaultOriginMatrix()
    {
        Matrix4x4 originMat = Matrix4x4.identity;
        originMat *= Matrix4x4.Translate(itemOrigin);
        originMat *= Matrix4x4.Scale(itemScaleXYZ);
        originMat *= Matrix4x4.Translate(itemTranslation);
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.x, Vector3.right));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.y, Vector3.up));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.z, Vector3.forward));
        originMat *= Matrix4x4.Translate(-itemOrigin);
        return originMat;
    }

    public override Matrix4x4 GetAnimationOriginMatrix()
    {
        Matrix4x4 originMat = Matrix4x4.identity;
        originMat *= Matrix4x4.Translate(itemOrigin);
        originMat *= Matrix4x4.Scale(itemScaleXYZ);
        originMat *= Matrix4x4.Translate(itemTranslation);
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.x, Vector3.right));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.y, Vector3.up));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.z, Vector3.forward));
        originMat *= Matrix4x4.Translate(-itemOrigin);
        return originMat;
    }

    public override void LoadFromOriginData(OriginPointData data)
    {
        itemTranslation = data.itemTranslation.ToVec3();
        itemOrigin = data.itemOrigin.ToVec3();
        itemRotation = data.itemRotation.ToVec3();
        itemScaleXYZ = data.itemScaleXYZ.ToVec3();
    }

    public override OriginPointData SaveToOriginData()
    {
        return new OriginPointData()
        {
            itemTranslation = new Vector3Ser(this.itemTranslation),
            itemOrigin = new Vector3Ser(this.itemOrigin),
            itemRotation = new Vector3Ser(this.itemRotation),
            itemScaleXYZ = new Vector3Ser(this.itemScaleXYZ)
        };
    }

    public override bool DependsOnOtherShapeElement()
    {
        return false;
    }

    public override ShapeElement GetShapeElementThisDependsOn()
    {
        return null;
    }

    public override bool AttemptToResolveOriginPoint()
    {
        return true;
    }

    public bool HasAnyEdits()
    {
        return itemTranslation != Vector3.zero ||
               itemRotation != Vector3.zero ||
               itemOrigin != new Vector3(0.5f, 0.5f, 0.5f) ||
               itemScaleXYZ != Vector3.one; 
    }

}
