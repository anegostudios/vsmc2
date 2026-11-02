using System.Linq;
using UnityEngine;
using VSMC;

public class AttachmentPointOriginPoint : IOriginPoint
{

    public LoadedAP foundAP;

    public ElementCodeInShape rootElemCode;
    public bool useMountOffset = false;

    public string apCode;

    public void FindAP()
    {
        foundAP = AttachmentPointsManager.main.GetAllAttachmentPointsIncludingOtherShapes().FirstOrDefault(x => x.code == apCode &&
            x.GetElement()?.parentShape == MultiShapeManager.main.GetShapeFromRootElementCodeInShape(rootElemCode));
    }

    public override Matrix4x4 GetDefaultOriginMatrix()
    {
        if (foundAP == null) return Matrix4x4.identity;
        ShapeElement step = foundAP.GetElement();
        Matrix4x4 originMat = Matrix4x4.identity;
        Vector3 apTranslation = Vector3.zero;
        Vector3 apRotation = Vector3.zero;
        bool show = !step.ShouldHideFromView() && step.gameObject != null && step.gameObject.gameObject.activeInHierarchy;
        if (show)
        {
            originMat = (step.GetRootElement().OriginPoint == null ? Matrix4x4.identity : step.GetRootElement().OriginPoint.GetDefaultOriginMatrix())
            * step.meshData.storedMatrix; //Make a copy...
            apTranslation = foundAP.position - (useMountOffset ? new Vector3(8, 0, 8) : Vector3.zero);
            apRotation = foundAP.rotation;
        }

        originMat *= Matrix4x4.Translate(itemOrigin);
        originMat *= Matrix4x4.Scale(itemScaleXYZ);
        originMat *= Matrix4x4.Translate((apTranslation / 16f) + itemTranslation);
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.x + apRotation.x, Vector3.right));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.y + apRotation.y, Vector3.up));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.z + apRotation.z, Vector3.forward));
        originMat *= Matrix4x4.Translate(-itemOrigin);
        return originMat;
    }

    public override Matrix4x4 GetAnimationOriginMatrix()
    {
        if (foundAP == null) return Matrix4x4.identity;
        ShapeElement step = foundAP.GetElement();
        Matrix4x4 originMat = Matrix4x4.identity;
        Vector3 apTranslation = Vector3.zero;
        Vector3 apRotation = Vector3.zero;
        bool show = !step.ShouldHideFromView() && step.gameObject != null && step.gameObject.gameObject.activeInHierarchy;
        if (show)
        {
            originMat = (step.GetRootElement().OriginPoint == null ? Matrix4x4.identity : step.GetRootElement().OriginPoint.GetAnimationOriginMatrix())
            * (AnimationEditorManager.main.GetAnimationShapeControllerFromRootShapeElement(step.GetRootElement())
            .animator.Matrices[step.JointId] * step.meshData.storedMatrix);
            apTranslation = foundAP.position - (useMountOffset ? new Vector3(8, 0, 8) : Vector3.zero);
            apRotation = foundAP.rotation;
        }
        originMat *= Matrix4x4.Translate(itemOrigin);
        originMat *= Matrix4x4.Scale(itemScaleXYZ);
        originMat *= Matrix4x4.Translate((apTranslation / 16f) + itemTranslation);
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.x + apRotation.x, Vector3.right));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.y + apRotation.y, Vector3.up));
        originMat *= Matrix4x4.Rotate(Quaternion.AngleAxis(itemRotation.z + apRotation.z, Vector3.forward));
        originMat *= Matrix4x4.Translate(-itemOrigin);
        return originMat;
    }

    public override void LoadFromOriginData(OriginPointData data)
    {
        itemTranslation = data.itemTranslation.ToVec3();
        itemOrigin = data.itemOrigin.ToVec3();
        itemRotation = data.itemRotation.ToVec3();
        itemScaleXYZ = data.itemScaleXYZ.ToVec3();
        rootElemCode = data.rootElement;
        apCode = data.apCode;
        useMountOffset = data.useMountOffset;
    }

    public override OriginPointData SaveToOriginData()
    {
        return new OriginPointData()
        {
            itemTranslation = new Vector3Ser(this.itemTranslation),
            itemOrigin = new Vector3Ser(this.itemOrigin),
            itemRotation = new Vector3Ser(this.itemRotation),
            itemScaleXYZ = new Vector3Ser(this.itemScaleXYZ),
            rootElement = this.rootElemCode,
            apCode = this.apCode,
            useMountOffset = this.useMountOffset
        };
    }

    public override bool DependsOnOtherShapeElement()
    {
        return true;
    }

    public override ShapeElement GetShapeElementThisDependsOn()
    {
        return foundAP.GetElement();
    }

    public override bool AttemptToResolveOriginPoint()
    {
        FindAP();
        if (foundAP == null)
        {
            Debug.Log("Failed to find a attachment point with code: " + apCode);
        }
        else
        {
            Debug.Log("Found attachment point "+apCode+" in element " + foundAP.GetElement().Name);
        }
        return foundAP != null;
    }

}
