namespace VSMC
{
    /// <summary>
    /// An undoable task to set an element's rotation origin.
    /// Used by the UI controls.
    /// </summary>
    public class TaskSetElementRotationOrigin : IEditTask
    {
        public int elementUID;
        public EnumAxis axis;
        public double newOrigin;
        public double oldOrigin;

        public TaskSetElementRotationOrigin(ShapeElement elem, EnumAxis axis, double origin)
        {
            elementUID = elem.elementUID;
            this.axis = axis;
            newOrigin = origin;
            oldOrigin = elem.RotationOrigin[(int)axis];
        }

        public override void DoTask()
        {
            //Nice and easy for origins.
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            elem.RotationOrigin[(int)axis] = newOrigin;
            elem.RecreateTransforms();
        }

        public override void UndoTask()
        {
            //Nice and easy for origins.
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            elem.RotationOrigin[(int)axis] = oldOrigin;
            elem.RecreateTransforms();
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            if (nextTask is TaskSetElementRotationOrigin t2)
            {
                if (t2.elementUID == elementUID && t2.axis == axis)
                {
                    //Keep old origin the same but update new origin.
                    newOrigin = t2.newOrigin;
                    return true;
                }
            }
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return
                sizeof(int) +
                sizeof(EnumAxis) +
                sizeof(double) * 2;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Move Element Origin";
        }
    }
}