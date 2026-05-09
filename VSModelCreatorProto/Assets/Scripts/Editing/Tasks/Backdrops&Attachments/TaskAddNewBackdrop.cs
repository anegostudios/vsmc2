using UnityEngine;

namespace VSMC
{
    public class TaskCreateNewBackdrop : IEditTask
    {

        public string localPathToBackdropShape;
        public TaskCreateNewBackdrop(string fromFilePath)
        {
            this.localPathToBackdropShape = fromFilePath;
        }

        public override void DoTask()
        {
            BackdropManager.main.CreateNewBackdrop(localPathToBackdropShape);
        }

        public override void UndoTask()
        {
            BackdropManager.main.RemoveBackdrop(BackdropManager.main.GetBackdropFromPath(localPathToBackdropShape));
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 0;
        }

        public override string GetTaskName()
        {
            return "Create new backdrop";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}