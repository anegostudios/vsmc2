using UnityEngine;

namespace VSMC
{
    public class TaskRemoveBackdrop : IEditTask
    {

        public BackdropOrAttachmentData storedData;
        public int foundIndex;

        public TaskRemoveBackdrop(LoadedBackdrop backdrop)
        {
            storedData = backdrop.data;
            foundIndex = BackdropManager.main.allBackdrops.IndexOf(backdrop);
        }

        public override void DoTask()
        {
            BackdropManager.main.RemoveBackdrop(BackdropManager.main.GetBackdropFromPath(storedData.shapeFilepath));
        }

        public override void UndoTask()
        {
            BackdropManager.main.CreateAndInitializeBackdropFromData(storedData, foundIndex);
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
            return "Remove backdrop";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}