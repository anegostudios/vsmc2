namespace VSMC
{
    /// <summary>
    /// Sets the rotation of a keyframe element.
    /// </summary>
    public class TaskSetKeyframeRotationForElement : IEditTask
    {

        AnimationKeyFrameElement kfElem;
        double rotX;
        double rotY;
        double rotZ;
        double? iRotX;
        double? iRotY;
        double? iRotZ;

        public TaskSetKeyframeRotationForElement(AnimationKeyFrameElement kfElem, double rotX, double rotY, double rotZ)
        {
            this.kfElem = kfElem;
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
            this.iRotX = kfElem.RotationX;
            this.iRotY = kfElem.RotationY;
            this.iRotZ = kfElem.RotationZ;
        }

        public override void DoTask()
        {
            kfElem.RotationX = rotX;
            kfElem.RotationY = rotY;
            kfElem.RotationZ = rotZ;
            AnimationEditorManager.main.OnAnyKeyframeChanged();
        }

        public override void UndoTask()
        {
            kfElem.RotationX = iRotX;
            kfElem.RotationY = iRotY;
            kfElem.RotationZ = iRotZ;
            AnimationEditorManager.main.OnAnyKeyframeChanged();
        }

        public EnumAxis? GetChangedAxis()
        {
            if (rotX != iRotX.GetValueOrDefault(0)) return EnumAxis.X;
            if (rotY != iRotY.GetValueOrDefault(0)) return EnumAxis.Y;
            if (rotZ != iRotZ.GetValueOrDefault(0)) return EnumAxis.Z;
            return null;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Edit Keyframe Rotation";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            if (nextTask is TaskSetKeyframeRotationForElement t2)
            {
                //If the start rotation of the next task is equal to the ideal rotation of this task, then merge.
                if (GetChangedAxis() == t2.GetChangedAxis() &&
                t2.iRotX.GetValueOrDefault(0) == rotX &&
                t2.iRotY.GetValueOrDefault(0) == rotY &&
                t2.iRotZ.GetValueOrDefault(0) == rotZ)
                {
                    rotX = t2.rotX;
                    rotY = t2.rotY;
                    rotZ = t2.rotZ; 
                    return true;
                }
            }
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(double) * 6;
        }
    }
}