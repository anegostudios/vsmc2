using UnityEngine;

namespace VSMC
{
    public class TaskSetOriginPointRotation : IEditTask
    {

        IOriginPoint origin;
        Vector3 oldRotation;
        Vector3 newRotation;

        public TaskSetOriginPointRotation(IOriginPoint origin, Vector3 newRotation)
        {
            this.origin = origin;
            this.oldRotation = origin.itemRotation;
            this.newRotation = newRotation;
        }

        public override void DoTask()
        {
            origin.itemRotation = newRotation;
            MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
            MultiShapeManager.main.UI.ResolveSelectionUI();
        }

        public override void UndoTask()
        {
            origin.itemRotation = oldRotation;
            MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
            MultiShapeManager.main.UI.ResolveSelectionUI();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Set Origin Rotation";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            //Just replace the new rotation value.
            if (nextTask is TaskSetOriginPointRotation t2)
            {
                if (t2.origin == origin)
                {
                    newRotation = t2.newRotation;
                    return true;
                }
            }
            return false;
        }

    }
}