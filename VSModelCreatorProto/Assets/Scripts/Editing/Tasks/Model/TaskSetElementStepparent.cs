using UnityEngine;

namespace VSMC
{
    public class TaskSetElementStepparent : IEditTask
    {

        public int elemUID;
        public string newStepparentName;
        public string oldStepparentName;

        public TaskSetElementStepparent(ShapeElement elem, string stepParentName)
        {
            elemUID = elem.elementUID;
            this.newStepparentName = stepParentName;
            this.oldStepparentName = elem.StepParentName;
        }


        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.StepParentName = newStepparentName;
            ShapeLoader.main.shapeHolder.RefreshAllStepparents();
        }

        public override void UndoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.StepParentName = oldStepparentName;
            ShapeLoader.main.shapeHolder.RefreshAllStepparents();
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 0;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Set Element Stepparent";
        }
    }
}