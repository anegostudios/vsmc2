namespace VSMC
{
    /// <summary>
    /// Changes the on stop behavior for an animation.
    /// </summary>
    public class TaskSetAnimationOnActivityStopped : IEditTask
    {

        Animation animation;
        EnumEntityActivityStoppedHandling newOnStopped;
        EnumEntityActivityStoppedHandling oldOnStopped;

        public TaskSetAnimationOnActivityStopped(Animation anim, EnumEntityActivityStoppedHandling newOnStopped)
        {
            this.animation = anim;
            this.newOnStopped = newOnStopped;
            this.oldOnStopped = anim.OnActivityStopped;
        }


        public override void DoTask()
        {
            animation.OnActivityStopped = newOnStopped;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }


        public override void UndoTask()
        {
            animation.OnActivityStopped = oldOnStopped;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }



        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Set Animation OnActivityStopped";
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