namespace VSMC
{
    /// <summary>
    /// Sets the in-editor category for an existing animation.
    /// </summary>
    public class TaskSetAnimationCategory : IEditTask
    {

        Animation animation;
        string newCategory;
        string oldCategory;

        public TaskSetAnimationCategory(Animation anim, string newCategory)
        {
            this.animation = anim;
            this.newCategory = newCategory;
            this.oldCategory = anim.CategoryInEditor;
        }


        public override void DoTask()
        {
            animation.CategoryInEditor = newCategory;
            AnimationEditorManager.main.UI.animationHierarchy.StartCreatingEntries();
        }


        public override void UndoTask()
        {
            animation.CategoryInEditor = oldCategory;
            AnimationEditorManager.main.UI.animationHierarchy.StartCreatingEntries();
        }



        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Set Animation Category";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}