namespace VSMC
{
    /// <summary>
    /// Sets the offset values to null for a key frame element, effectively removing the position keyframe.
    /// </summary>
    public class TaskDisablePositionKeyframeForElement : IEditTask
    {

        AnimationKeyFrameElement kfElem;
        Animation animation;
        int frame;
        string elemCode;

        double? storedOffsetX;
        double? storedOffsetY;
        double? storedOffsetZ;

        public TaskDisablePositionKeyframeForElement(Animation animation, int frame, string elemCode)
        {
            this.animation = animation;
            this.frame = frame;
            this.elemCode = elemCode;
            kfElem = animation.GetOrCreateKeyFrame(frame).GetOrCreateKeyFrameElement(elemCode);
            storedOffsetX = kfElem.OffsetX;
            storedOffsetY = kfElem.OffsetY;
            storedOffsetZ = kfElem.OffsetZ;

        }

        public override void DoTask()
        {
            //The keyframe element will always exist at this point.
            kfElem.OffsetX = null;
            kfElem.OffsetY = null;
            kfElem.OffsetZ = null;
            animation.RemoveAnyEmptyKeyFrames();
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
        }

        public override void UndoTask()
        {
            //There's a slim chance that the keyframe got removed if this is a redo... we may need to create it again.
            kfElem = animation.GetOrCreateKeyFrame(frame).GetOrCreateKeyFrameElement(elemCode);
            kfElem.OffsetX = storedOffsetX;
            kfElem.OffsetY = storedOffsetY;
            kfElem.OffsetZ = storedOffsetZ;
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Disable Position Keyframe";
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