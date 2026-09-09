using System.Linq;
using UnityEngine;
using VSMC;

public class AttachmentPointOriginPoint : IOriginPoint
{

    public LoadedAP foundAP;

    public int shapeUID;
    public string apCode;

    public void FindAP()
    {
        if (shapeUID == 0)
        {
            foundAP = AttachmentPointsManager.main.allAPs.FirstOrDefault(x => x.code == apCode);
        }
        else
        {
            Shape shape = MultiShapeManager.main.GetShapeFromUID(shapeUID);
            foundAP = AttachmentPointsManager.main.allAPs.FirstOrDefault(x => x.code == apCode && x.GetElement()?.parentShape == shape);
        }
    }

    public override Matrix4x4 GetDefaultOriginMatrix()
    {
        ShapeElement step = foundAP.GetElement();
        Matrix4x4 originMat = (step.OriginPoint == null ? Matrix4x4.identity : step.OriginPoint.GetDefaultOriginMatrix())
        * step.meshData.storedMatrix; //Make a copy...
        originMat *= Matrix4x4.Translate(itemOrigin / 16f);
        originMat *= Matrix4x4.Scale(itemScaleXYZ);
        originMat *= Matrix4x4.Translate((foundAP.position + itemTranslation) / 16f);
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.x + foundAP.rotation.x, Vector3.right));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.y + foundAP.rotation.y, Vector3.up));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.z + foundAP.rotation.z, Vector3.forward));
        originMat *= Matrix4x4.Translate(-itemOrigin / 16f);
        return originMat;
    }

    public override Matrix4x4 GetAnimationOriginMatrix()
    {
        ShapeElement step = foundAP.GetElement();
        Matrix4x4 originMat = (step.OriginPoint == null ? Matrix4x4.identity : step.OriginPoint.GetAnimationOriginMatrix())
        * (AnimationEditorManager.main.GetAnimationShapeControllerFromShape(step.parentShape)
        .animator.Matrices[step.JointId] * step.meshData.storedMatrix);
        originMat *= Matrix4x4.Translate(itemOrigin / 16f);
        originMat *= Matrix4x4.Scale(itemScaleXYZ);
        originMat *= Matrix4x4.Translate((foundAP.position + itemTranslation) / 16f);
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.x + foundAP.rotation.x, Vector3.right));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.y + foundAP.rotation.y, Vector3.up));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.z + foundAP.rotation.z, Vector3.forward));
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
        apCode = data.apCode;
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
            apCode = this.apCode
        };
    }

    public override bool DependsOnOtherRootShapeElement()
    {
        return true;
    }

    public override ShapeElement GetRootShapeElementThisDependsOn()
    {
        return foundAP.GetElement();
    }

    public override bool AttemptToResolveOriginPoint()
    {
        FindAP();
        return foundAP != null;
    }

}
