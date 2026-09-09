using UnityEngine;
using VSMC;

/// <summary>
/// An origin point that does not attach to anything - It is just a transformation.
/// </summary>
public class TransformOnlyOriginPoint : IOriginPoint
{

    public override Matrix4x4 GetDefaultOriginMatrix()
    {
        Matrix4x4 originMat = new Matrix4x4();
        originMat *= Matrix4x4.Translate(itemOrigin / 16f);
        originMat *= Matrix4x4.Scale(itemScaleXYZ);
        originMat *= Matrix4x4.Translate(itemTranslation / 16f);
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.x, Vector3.right));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.y, Vector3.up));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.z, Vector3.forward));
        originMat *= Matrix4x4.Translate(-itemOrigin / 16f);
        return originMat;
    }

    public override Matrix4x4 GetAnimationOriginMatrix()
    {
        Matrix4x4 originMat = new Matrix4x4();
        originMat *= Matrix4x4.Translate(itemOrigin / 16f);
        originMat *= Matrix4x4.Scale(itemScaleXYZ);
        originMat *= Matrix4x4.Translate(itemTranslation / 16f);
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.x, Vector3.right));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.y, Vector3.up));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.z, Vector3.forward));
        originMat *= Matrix4x4.Translate(-itemOrigin / 16f);
        return originMat;
    }

    public override void LoadFromOriginData(OriginPointData data)
    {
        itemTranslation = data.itemTranslation;
        itemOrigin = data.itemOrigin;
        itemRotation = data.itemRotation;
        itemScaleXYZ = data.itemScaleXYZ;
    }

    public override OriginPointData SaveToOriginData()
    {
        return new OriginPointData()
        {
            itemTranslation = this.itemTranslation,
            itemOrigin = this.itemOrigin,
            itemRotation = this.itemRotation,
            itemScaleXYZ = this.itemScaleXYZ
        };
    }

    public override bool DependsOnOtherRootShapeElement()
    {
        return false;
    }

    public override ShapeElement GetRootShapeElementThisDependsOn()
    {
        return null;
    }

    public override bool AttemptToResolveOriginPoint()
    {
        return true;
    }

}
