namespace VSMC
{
    public class TaskSetElementRotation : IEditTask
    {
        public int elementUID;
        public EnumAxis axis;
        public double newRot;
        public double oldRot;

        public TaskSetElementRotation(ShapeElement elem, EnumAxis axis, double angle)
        {
            elementUID = elem.elementUID;
            this.axis = axis;
            newRot = angle;
            oldRot = GetRot(elem, axis);
        }

        double GetRot(ShapeElement elem, EnumAxis axis)
        {
            if (axis == EnumAxis.X) return elem.RotationX;
            else if (axis == EnumAxis.Y) return elem.RotationY;
            return elem.RotationZ;
        }
        
        void SetRot(ShapeElement elem, EnumAxis axis, double val)
        {
            if (axis == EnumAxis.X) elem.RotationX = val;
            else if (axis == EnumAxis.Y) elem.RotationY = val;
            else elem.RotationZ = val;
        }

        public override void DoTask()
        {
            //Nice and easy for rotation.
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            SetRot(elem, axis, newRot);
            elem.RecreateTransforms();
        }

        public override void UndoTask()
        {
            //Nice and easy for rotation.
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            SetRot(elem, axis, oldRot);
            elem.RecreateTransforms();
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            //Just replace the new rotation value.
            if (nextTask is TaskSetElementRotation t2)
            {
                if (t2.elementUID == elementUID && t2.axis == axis)
                {
                    newRot = t2.newRot;
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
            return "Rotate Element";
        }
    }
}