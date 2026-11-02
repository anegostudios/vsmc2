using UnityEngine;
using VSMC;

public class TaskSetAllOriginPointData : IEditTask
{

    IOriginPoint origin;
    public Vector3[] newValues;
    public Vector3[] oldValues;

    public TaskSetAllOriginPointData(IOriginPoint origin, Vector3 newOrig, Vector3 newTranslation, Vector3 newRot, Vector3 newScale)
    {
        this.origin = origin;
        newValues = new Vector3[4] { newOrig, newTranslation, newRot, newScale };
        oldValues = new Vector3[4] { origin.itemOrigin, origin.itemTranslation, origin.itemRotation, origin.itemScaleXYZ };
    }

    public override void DoTask()
    {
        origin.itemOrigin = newValues[0];
        origin.itemTranslation = newValues[1];
        origin.itemRotation = newValues[2];
        origin.itemScaleXYZ = newValues[3];
        MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override void UndoTask()
    {
        origin.itemOrigin = oldValues[0];
        origin.itemTranslation = oldValues[1];
        origin.itemRotation = oldValues[2];
        origin.itemScaleXYZ = oldValues[3];
        MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Paste Origin Point Data";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
