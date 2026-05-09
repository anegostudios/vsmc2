using System;
using UnityEngine;

namespace VSMC
{
    public class TaskSetAttachmentColorIndex : IEditTask
    {

        public string attachmentPath;
        public int newColorIndex;
        public int oldColorIndex;

        public TaskSetAttachmentColorIndex(LoadedAttachment attachment, int newColorIndex)
        {
            this.attachmentPath = attachment.data.shapeFilepath;
            this.newColorIndex = newColorIndex;
            this.oldColorIndex = attachment.data.flatColorIndex;
        }

        public override void DoTask()
        {
            AttachmentManager.main.GetAttachmentFromPath(attachmentPath).SetColorIndex(newColorIndex);
            BackdropAndAttachmentMenuManager.main.RefreshUIElements();
        }

        public override void UndoTask()
        {
            AttachmentManager.main.GetAttachmentFromPath(attachmentPath).SetColorIndex(oldColorIndex);
            BackdropAndAttachmentMenuManager.main.RefreshUIElements();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 16;
        }

        public override string GetTaskName()
        {
            return "Set Attachment Color Index";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}