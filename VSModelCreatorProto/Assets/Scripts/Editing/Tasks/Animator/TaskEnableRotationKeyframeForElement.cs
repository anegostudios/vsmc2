namespace VSMC
{
    /// <summary>
    /// Sets the rotation values to their unmodified values for a key frame element, effectively adding a rotation keyframe..
    /// </summary>
    public class TaskEnableRotationKeyframeForElement : IEditTask
    {

        AnimationKeyFrameElement kfElem;
        Animation animation;
        int frame;
        string elemCode;

        public TaskEnableRotationKeyframeForElement(Animation animation, int frame, string elemCode)
        {
            this.animation = animation;
            this.frame = frame;
            this.elemCode = elemCode;
            this.kfElem = animation.GetOrCreateKeyFrame(frame).GetOrCreateKeyFrameElement(elemCode);
        }

        public override void DoTask()
        {
            //We should try and calculate the ideal rotation based on the two keyframes this is between.
            //Is this as simple as lerping between the two in-between rotations?
            this.kfElem = animation.GetOrCreateKeyFrame(frame).GetOrCreateKeyFrameElement(elemCode);
            kfElem.RotationX = 0;
            kfElem.RotationY = 0;
            kfElem.RotationZ = 0;
            animation.RemoveAnyEmptyKeyFrames();
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
        }

        public override void UndoTask()
        {
            //Just set the rotations to null.
            //This should not need to save any stored rotation.
            kfElem.RotationX = null;
            kfElem.RotationY = null;
            kfElem.RotationZ = null;
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Enable Rotation Keyframe";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 8;
        }
    }
}