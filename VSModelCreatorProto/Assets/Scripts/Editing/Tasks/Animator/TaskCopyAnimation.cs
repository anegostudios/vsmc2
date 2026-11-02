using Newtonsoft.Json;

namespace VSMC
{
    /// <summary>
    /// Creates a new, empty animation.
    /// </summary>
    public class TaskCopyAnimation : IEditTask
    {
        public Animation copiedAnimation;

        public TaskCopyAnimation(Animation toCopy)
        {
            string uniqueName = GetUniqueAnimationNameAndCode(toCopy.Code);
            string copySer = JsonConvert.SerializeObject(toCopy);
            copiedAnimation = JsonConvert.DeserializeObject<Animation>(copySer);
            copiedAnimation.Code = uniqueName;
            copiedAnimation.Name = uniqueName;
        }

        string GetUniqueAnimationNameAndCode(string inName)
        {
            int c = 0;
            string sName = inName;
            bool isTaken = true;
            while (isTaken)
            {
                isTaken = false;
                c++;
                sName = inName + c;
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
            ShapeHolder.CurrentLoadedShape.Animations =
            ShapeHolder.CurrentLoadedShape.Animations.Append(copiedAnimation);
            AnimationEditorManager.main.OnAnimationDataChanged();
            AnimationSelector.main.Select(copiedAnimation);
        }

        public override void UndoTask()
        {
            ShapeHolder.CurrentLoadedShape.Animations =
            ShapeHolder.CurrentLoadedShape.Animations.Remove(copiedAnimation);
            AnimationEditorManager.main.OnAnimationDataChanged();
            AnimationSelector.main.DeselectCurrent();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Copy Animation";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}