using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;

namespace VSMC
{
    /// <summary>
    /// Sets the duration for an animation. This will remove keyframes to reduce animation duration.
    /// </summary>
    public class TaskSetAnimationDuration : IEditTask
    {

        Animation animation;
        int newDuration;
        int oldDuration;
        public List<AnimationKeyFrame> removedKeyframes;

        public TaskSetAnimationDuration(Animation anim, int newDuration)
        {
            this.animation = anim;
            this.newDuration = newDuration;
            this.oldDuration = anim.QuantityFrames;
            removedKeyframes = new List<AnimationKeyFrame>();
            foreach (AnimationKeyFrame i in anim.KeyFrames)
            {
                if (i.Frame >= newDuration)
                {
                    removedKeyframes.Add(i);
                }
            }
        }


        public override void DoTask()
        {
            foreach (AnimationKeyFrame kfs in removedKeyframes)
            {
                animation.KeyFrames= animation.KeyFrames.Remove(kfs);
            }
            animation.QuantityFrames = newDuration;
            animation.PrevNextKeyFrameByFrame = null;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }


        public override void UndoTask()
        {
            foreach (AnimationKeyFrame kfs in removedKeyframes)
            {
                animation.KeyFrames = animation.KeyFrames.Append(kfs);
            }
            animation.QuantityFrames = oldDuration;
            animation.PrevNextKeyFrameByFrame = null;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }



        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Set Animation Duration";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            //Shrug again.
            return 128;
        }
    }
}