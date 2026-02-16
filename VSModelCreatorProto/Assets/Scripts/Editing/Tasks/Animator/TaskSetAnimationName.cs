namespace VSMC
{
    /// <summary>
    /// Sets the name for an existing animation.
    /// </summary>
    public class TaskSetAnimationName : IEditTask
    {

        Animation animation;
        string newName;
        string oldName;

        public TaskSetAnimationName(Animation anim, string newName)
        {
            this.animation = anim;
            this.newName = newName;
            this.oldName = anim.Name;
        }


        public override void DoTask()
        {
            animation.Name = newName;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }


        public override void UndoTask()
        {
            animation.Name = oldName;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }



        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Set Animation Name";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(char) * (newName.Length + oldName.Length);
        }
    }
}