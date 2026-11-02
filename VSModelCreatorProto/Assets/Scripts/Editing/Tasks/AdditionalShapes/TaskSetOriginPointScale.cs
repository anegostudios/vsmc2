using UnityEngine;

namespace VSMC
{
    public class TaskSetOriginPointScale : IEditTask
    {

        IOriginPoint origin;
        Vector3 oldScaleXYZ;
        Vector3 newScaleXYZ;

        public TaskSetOriginPointScale(IOriginPoint origin, Vector3 newScaleXYZ)
        {
            this.origin = origin;
            this.oldScaleXYZ = origin.itemScaleXYZ;
            this.newScaleXYZ = newScaleXYZ;
        }

        public override void DoTask()
        {
            origin.itemScaleXYZ = newScaleXYZ;
            MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
            MultiShapeManager.main.UI.ResolveSelectionUI();
        }

        public override void UndoTask()
        {
            origin.itemScaleXYZ = oldScaleXYZ;
            MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
            MultiShapeManager.main.UI.ResolveSelectionUI();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Set Origin Scale";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}