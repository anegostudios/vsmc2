using UnityEngine;

namespace VSMC
{
    public class TaskRemoveAttachment : IEditTask
    {

        public BackdropOrAttachmentData storedData;
        public int foundIndex;

        public TaskRemoveAttachment(LoadedAttachment attachment)
        {
            storedData = attachment.data;
            foundIndex = AttachmentManager.main.allAttachments.IndexOf(attachment);
        }

        public override void DoTask()
        {
            AttachmentManager.main.RemoveAttachment(AttachmentManager.main.GetAttachmentFromPath(storedData.shapeFilepath));
        }

        public override void UndoTask()
        {
            AttachmentManager.main.CreateAndInitializeAttachmentFromData(storedData, foundIndex);
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
            return "Remove attachment";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}