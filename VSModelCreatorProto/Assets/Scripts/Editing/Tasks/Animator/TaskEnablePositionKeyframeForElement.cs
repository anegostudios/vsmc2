namespace VSMC
{
    /// <summary>
    /// Sets the position values to their unmodified values for a key frame element, effectively adding a position keyframe.
    /// </summary>
    public class TaskEnablePositionKeyframeForElement : IEditTask
    {

        AnimationKeyFrameElement kfElem;
        Animation animation;
        int frame;
        string elemCode;

        public TaskEnablePositionKeyframeForElement(Animation animation, int frame, string elemCode)
        {
            this.animation = animation;
            this.frame = frame;
            this.elemCode = elemCode;
            this.kfElem = animation.GetOrCreateKeyFrame(frame).GetOrCreateKeyFrameElement(elemCode);
        }

        public override void DoTask()
        {
            ElementPose pose = animation.GenerateFrameForFlag(frame, ShapeElementRegistry.main.GetShapeElementByName(elemCode), 0);
            this.kfElem = animation.GetOrCreateKeyFrame(frame).GetOrCreateKeyFrameElement(elemCode);
            if (pose == null)
            {
                kfElem.OffsetX = 0;
                kfElem.OffsetY = 0;
                kfElem.OffsetZ = 0;
            }
            else
            {
                //Translatiosn are converted in model space, but we actually want to undo that conversion, hence the * 16.
                kfElem.OffsetX = pose.translateX * 16;
                kfElem.OffsetY = pose.translateY * 16;
                kfElem.OffsetZ = pose.translateZ * 16;
            }
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
        }

        public override void UndoTask()
        {
            //Just set the offsets to null.
            //This should not need to save any stored positions.
            kfElem.OffsetX = null;
            kfElem.OffsetY = null;
            kfElem.OffsetZ = null;
            animation.RemoveAnyEmptyKeyFrames();
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Enable Position Keyframe";
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