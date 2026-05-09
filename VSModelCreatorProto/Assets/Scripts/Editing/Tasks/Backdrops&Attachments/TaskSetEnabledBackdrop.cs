using UnityEngine;
using UnityEngine.AI;

namespace VSMC
{
    /// <summary>
    /// Sets whether the selected backdrop is the currently enabled one, or disables it.
    /// </summary>
    public class TaskSetEnabledBackdrop : IEditTask
    {

        public string backdrop;
        public bool newEnabled;
        public string previouslyEnabled;

        public TaskSetEnabledBackdrop(LoadedBackdrop backdrop, bool newEnabled)
        {
            this.previouslyEnabled = BackdropManager.main.cActiveBackdrop?.data?.shapeFilepath;
            this.newEnabled = newEnabled;
            this.backdrop = backdrop.data.shapeFilepath;
        }

        public override void DoTask()
        {
            if (newEnabled == true)
            {
                BackdropManager.main.SetEnabledBackdrop(backdrop);
            }
            else if (newEnabled == false && BackdropManager.main.CurrentActiveBackdropPath == backdrop)
            {
                BackdropManager.main.DisableCurrentBackdrop();
            }
        }

        public override void UndoTask()
        {
            if (previouslyEnabled == null) BackdropManager.main.DisableCurrentBackdrop();
            else BackdropManager.main.SetEnabledBackdrop(previouslyEnabled);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Enable/Disable Backdrop";
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 17;
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}