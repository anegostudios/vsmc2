using UnityEngine;

namespace VSMC
{
    public class TaskCreateNewAttachment : IEditTask
    {

        public string localPathToAttachmentShape;
        public TaskCreateNewAttachment(string fromFilePath)
        {
            this.localPathToAttachmentShape = fromFilePath;
        }

        public override void DoTask()
        {
            AttachmentManager.main.CreateNewAttachment(localPathToAttachmentShape);
        }

        public override void UndoTask()
        {
            AttachmentManager.main.RemoveAttachment(AttachmentManager.main.GetAttachmentFromPath(localPathToAttachmentShape));
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
            return "Create new attachment";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}