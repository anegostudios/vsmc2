using UnityEngine;

namespace VSMC
{
    public class TaskSetOriginPointTranslation : IEditTask
    {

        IOriginPoint origin;
        Vector3 oldTranslation;
        Vector3 newTranslation;

        public TaskSetOriginPointTranslation(IOriginPoint origin, Vector3 newTranslation)
        {
            this.origin = origin;
            this.oldTranslation = origin.itemTranslation;
            this.newTranslation = newTranslation;
        }

        public override void DoTask()
        {
            origin.itemTranslation = newTranslation;
            MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
            MultiShapeManager.main.UI.ResolveSelectionUI();
        }

        public override void UndoTask()
        {
            origin.itemTranslation = oldTranslation;
            MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
            MultiShapeManager.main.UI.ResolveSelectionUI();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Set Origin Translation";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}