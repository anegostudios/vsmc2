using UnityEngine;

namespace VSMC
{
    public class TaskSetOriginPointOrigin : IEditTask
    {

        IOriginPoint origin;
        Vector3 oldOrigin;
        Vector3 newOrigin;

        public TaskSetOriginPointOrigin(IOriginPoint origin, Vector3 newOrigin)
        {
            this.origin = origin;
            this.oldOrigin = origin.itemOrigin;
            this.newOrigin = newOrigin;
        }

        public override void DoTask()
        {
            origin.itemOrigin = newOrigin;
            MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
            MultiShapeManager.main.UI.ResolveSelectionUI();
        }

        public override void UndoTask()
        {
            origin.itemOrigin = oldOrigin;
            MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
            MultiShapeManager.main.UI.ResolveSelectionUI();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Set Origin Origin";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}