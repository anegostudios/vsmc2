using Unity.VisualScripting;

namespace VSMC
{
    /// <summary>
    /// Deletes an entire animation. Use with caution.
    /// </summary>
    public class TaskDeleteAnimation : IEditTask
    {

        Animation deletedAnimation;
        int removalIndex;

        public TaskDeleteAnimation(Animation toDelete)
        {
            deletedAnimation = toDelete;
            removalIndex = ShapeHolder.CurrentLoadedShape.Animations.IndexOf(x => x.Code == toDelete.Code);
        }

        public override void DoTask()
        {
            ShapeHolder.CurrentLoadedShape.Animations =
            ShapeHolder.CurrentLoadedShape.Animations.RemoveAt(removalIndex);
            AnimationEditorManager.main.OnAnimationDataChanged();
            AnimationSelector.main.DeselectCurrent();
        }

        public override void UndoTask()
        {
            ShapeHolder.CurrentLoadedShape.Animations =
            ShapeHolder.CurrentLoadedShape.Animations.InsertAt(deletedAnimation, removalIndex);
            AnimationEditorManager.main.OnAnimationDataChanged();
            AnimationSelector.main.Select(deletedAnimation);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Delete "+deletedAnimation.Name+" Animation";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            //*shrug*
            return 2048;
        }
    }
}