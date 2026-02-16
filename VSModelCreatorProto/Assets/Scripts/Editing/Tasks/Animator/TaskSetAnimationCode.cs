namespace VSMC
{
    /// <summary>
    /// Sets the code for an existing animation. This does not check the validity of the new code.
    /// </summary>
    public class TaskSetAnimationCode : IEditTask
    {

        Animation animation;
        string newCode;
        string oldCode;

        /// <summary>
        /// This will <b>NOT</b> check the validity of the new code. Use with caution.
        /// </summary>
        public TaskSetAnimationCode(Animation anim, string newCode)
        {
            this.animation = anim;
            this.newCode = newCode;
            this.oldCode = anim.Code;
        }


        public override void DoTask()
        {
            animation.Code = newCode;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }


        public override void UndoTask()
        {
            animation.Code = oldCode;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }



        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Set Animation Code";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(char) * (newCode.Length + oldCode.Length);
        }
    }
}