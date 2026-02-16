namespace VSMC
{
    /// <summary>
    /// Sets the rotation values to null for a key frame element, effectively removing the rotation keyframe.
    /// </summary>
    public class TaskDisableRotationKeyframeForElement : IEditTask
    {

        AnimationKeyFrameElement kfElem;
        Animation animation;
        int frame;
        string elemCode;

        double? storedRotX;
        double? storedRotY;
        double? storedRotZ;

        public TaskDisableRotationKeyframeForElement(Animation animation, int frame, string elemCode)
        {
            this.animation = animation;
            this.frame = frame;
            this.elemCode = elemCode;
            kfElem = animation.GetOrCreateKeyFrame(frame).GetOrCreateKeyFrameElement(elemCode);
            storedRotX = kfElem.RotationX;
            storedRotY = kfElem.RotationY;
            storedRotZ = kfElem.RotationZ;
        }

        public override void DoTask()
        {
            kfElem.RotationX = null;
            kfElem.RotationY = null;
            kfElem.RotationZ = null;
            animation.RemoveAnyEmptyKeyFrames();
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
        }

        public override void UndoTask()
        {
            //There's a slim chance that the keyframe got removed if this is a redo... we may need to create it again.
            kfElem = animation.GetOrCreateKeyFrame(frame).GetOrCreateKeyFrameElement(elemCode);
            kfElem.RotationX = storedRotX;
            kfElem.RotationY = storedRotY;
            kfElem.RotationZ = storedRotZ;
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Disable Rotation Keyframe";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            //*shrug*
            return sizeof(double) * 3;
        }
    }
}