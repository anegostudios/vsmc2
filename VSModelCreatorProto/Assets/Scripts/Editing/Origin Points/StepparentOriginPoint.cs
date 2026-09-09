using UnityEngine;
using VSMC;

/// <summary>
/// An origin point that connects to an existing element.
/// </summary>
public class StepparentOriginPoint : IOriginPoint
{

    public ShapeElement foundStepparent;

    public int shapeUID;
    public int foundElementUID;
    public string elementName;

    public void FindShape()
    {

    }

    public override Matrix4x4 GetDefaultOriginMatrix()
    {
        Matrix4x4 originMat = (foundStepparent.OriginPoint == null ? Matrix4x4.identity : foundStepparent.OriginPoint.GetDefaultOriginMatrix())
         * foundStepparent.meshData.storedMatrix;
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
        Matrix4x4 originMat = (foundStepparent.OriginPoint == null ? Matrix4x4.identity : foundStepparent.OriginPoint.GetAnimationOriginMatrix()) *
        (AnimationEditorManager.main.GetAnimationShapeControllerFromShape(foundStepparent.parentShape)
        .animator.Matrices[foundStepparent.JointId] * foundStepparent.meshData.storedMatrix);
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
        shapeUID = data.shapeUID;
        elementName = data.shapeElementCode;
    }

    public override OriginPointData SaveToOriginData()
    {
        return new OriginPointData()
        {
            itemTranslation = this.itemTranslation,
            itemOrigin = this.itemOrigin,
            itemRotation = this.itemRotation,
            itemScaleXYZ = this.itemScaleXYZ,
            shapeUID = this.shapeUID,
            shapeElementCode = this.elementName
        };
    }

    public override bool DependsOnOtherRootShapeElement()
    {
        return true;
    }

    public override ShapeElement GetRootShapeElementThisDependsOn()
    {
        return foundStepparent;
    }

    public override bool AttemptToResolveOriginPoint()
    {
        FindShape();
        return foundStepparent != null;
    }

}
