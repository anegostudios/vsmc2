using System;
using System.Net.Mail;
using UnityEngine;

namespace VSMC
{
    public class TaskSetAttachmentOpacity : IEditTask
    {

        public string attachmentPath;
        public float oldOpacity;
        public float newOpacity;

        public DateTime taskTime;

        public TaskSetAttachmentOpacity(LoadedAttachment attachment, float oldOpacity, float newOpacity)
        {
            this.attachmentPath = attachment.data.shapeFilepath;
            this.oldOpacity = oldOpacity;
            this.newOpacity = newOpacity;
            taskTime = DateTime.Now;
        }

        public override void DoTask()
        {
            AttachmentManager.main.GetAttachmentFromPath(attachmentPath).SetOpacity(newOpacity);
            BackdropAndAttachmentMenuManager.main.RefreshUIElements();
        }

        public override void UndoTask()
        {
            AttachmentManager.main.GetAttachmentFromPath(attachmentPath).SetOpacity(oldOpacity);
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
            return "Set Attachment Opacity";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            if (nextTask is TaskSetAttachmentOpacity o2)
            {
                if (o2.attachmentPath == attachmentPath && (o2.taskTime - taskTime).TotalSeconds < 2)
                {
                    this.newOpacity = o2.newOpacity;
                    this.taskTime = o2.taskTime;
                    return true;
                }
            }
            return false;
        }

    }
}