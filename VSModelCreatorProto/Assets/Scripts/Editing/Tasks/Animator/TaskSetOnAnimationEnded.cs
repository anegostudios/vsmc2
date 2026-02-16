namespace VSMC
{
    /// <summary>
    /// Changes the on end behavior for an animation.
    /// </summary>
    public class TaskSetOnAnimationEnded : IEditTask
    {

        Animation animation;
        EnumEntityAnimationEndHandling newOnEnd;
        EnumEntityAnimationEndHandling oldOnEnd;

        public TaskSetOnAnimationEnded(Animation anim, EnumEntityAnimationEndHandling newOnEnd)
        {
            this.animation = anim;
            this.newOnEnd = newOnEnd;
            this.oldOnEnd = anim.OnAnimationEnd;
        }


        public override void DoTask()
        {
            animation.OnAnimationEnd = newOnEnd;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }


        public override void UndoTask()
        {
            animation.OnAnimationEnd = oldOnEnd;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }



        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Set Animation OnAnimationEnd";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) * 2;
        }
    }
}