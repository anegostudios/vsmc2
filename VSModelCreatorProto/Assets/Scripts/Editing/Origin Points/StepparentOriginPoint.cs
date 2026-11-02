using System.Collections.Generic;
using UnityEngine;
using VSMC;

/// <summary>
/// An origin point that connects to an existing element.
/// </summary>
public class StepparentOriginPoint : IOriginPoint
{

    public ShapeElement foundStepparent;

    public ElementCodeInShape elemCode;
    public int foundElementUID;

    public void FindShapeElement()
    {
        foundStepparent = MultiShapeManager.main.GetShapeElementFromRootElementCodeInShape(elemCode);
        if (foundStepparent != null)
        {
            foundElementUID = foundStepparent.elementUID;
        }
    }

    public override Matrix4x4 GetDefaultOriginMatrix()
    {
        if (foundStepparent == null) return Matrix4x4.identity;
        Matrix4x4 originMat = Matrix4x4.identity;
        
        bool show = !foundStepparent.ShouldHideFromView() && foundStepparent.gameObject != null && foundStepparent.gameObject.gameObject.activeInHierarchy;
        if (show)
        {
            originMat = (foundStepparent.GetRootElement().OriginPoint == null ? Matrix4x4.identity : foundStepparent.GetRootElement().OriginPoint.GetDefaultOriginMatrix())
            * foundStepparent.meshData.storedMatrix;
        }
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
        if (foundStepparent == null) return Matrix4x4.identity;
        Matrix4x4 originMat = Matrix4x4.identity;

        bool show = !foundStepparent.ShouldHideFromView() && foundStepparent.gameObject != null && foundStepparent.gameObject.gameObject.activeInHierarchy;
        if (show)
        {
            originMat = (foundStepparent.GetRootElement().OriginPoint == null ? Matrix4x4.identity : foundStepparent.GetRootElement().OriginPoint.GetAnimationOriginMatrix()) *
            (AnimationEditorManager.main.GetAnimationShapeControllerFromRootShapeElement(foundStepparent.GetRootElement())
            .animator.Matrices[foundStepparent.JointId] * foundStepparent.meshData.storedMatrix);
        }
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
        elemCode = data.rootElement;
    }

    public override OriginPointData SaveToOriginData()
    {
        return new OriginPointData()
        {
            itemTranslation = new Vector3Ser(this.itemTranslation),
            itemOrigin = new Vector3Ser(this.itemOrigin),
            itemRotation = new Vector3Ser(this.itemRotation),
            itemScaleXYZ = new Vector3Ser(this.itemScaleXYZ),
            rootElement = elemCode,
        };
    }

    public override bool DependsOnOtherShapeElement()
    {
        return true;
    }

    public override ShapeElement GetShapeElementThisDependsOn()
    {
        if (foundStepparent != null)
        {
            return foundStepparent;
        }   
        return null;
    }

    public override bool AttemptToResolveOriginPoint()
    {
        FindShapeElement();
        if (foundStepparent == null)
        {
            Debug.Log("Failed to find a stepparent element with code: "+ elemCode.element);
        }
        else
        {
            Debug.Log("Found stepparent element of " + foundStepparent.Name);
        }
        return foundStepparent != null;
    }

}
