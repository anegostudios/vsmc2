namespace VSMC {
    /// <summary>
    /// Creates a new, empty animation.
    /// </summary>
    public class TaskCreateNewAnimation : IEditTask
    {

        string animationName;

        public TaskCreateNewAnimation()
        {
            this.animationName = GetUniqueAnimationNameAndCode();
        }

        string GetUniqueAnimationNameAndCode()
        {
            int c = 0;
            string sName = "anim";
            bool isTaken = true;
            while (isTaken)
            {
                isTaken = false;
                c++;
                sName = "anim" + c;
                foreach (Animation anim in ShapeHolder.CurrentLoadedShape.Animations)
                {
                    if (anim.Code == sName || anim.Name == sName)
                    {
                        isTaken = true;
                        break;
                    }
                }
            }
            return sName;
        }

        public override void DoTask()
        {
            Animation newAnim = new Animation(animationName);
            ShapeHolder.CurrentLoadedShape.Animations =
            ShapeHolder.CurrentLoadedShape.Animations.Append(
                newAnim
            );
            AnimationEditorManager.main.OnAnimationDataChanged();
            AnimationSelector.main.Select(newAnim);
        }

        public override void UndoTask()
        {
            ShapeHolder.CurrentLoadedShape.Animations =
            ShapeHolder.CurrentLoadedShape.Animations.RemoveAt(
                ShapeHolder.CurrentLoadedShape.Animations.IndexOf(x => x.Code == animationName)
            );
            AnimationEditorManager.main.OnAnimationDataChanged();
            AnimationSelector.main.DeselectCurrent();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Create New Animation";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(char) * animationName.Length;
        }
    }
}