using UnityEngine;
using UnityEngine.AI;

namespace VSMC
{
    /// <summary>
    /// Enables or disables an attachment.
    /// </summary>
    public class TaskSetEnabledAttachment : IEditTask
    {

        public string attachment;
        public bool newEnabled;
        public bool previouslyEnabled;

        public TaskSetEnabledAttachment(LoadedAttachment attachment, bool newEnabled)
        {
            previouslyEnabled = attachment.data.enabled;
            this.newEnabled = newEnabled;
            this.attachment = attachment.data.shapeFilepath;
        }

        public override void DoTask()
        {
            AttachmentManager.main.SetEnabledAttachment(attachment, newEnabled);
        }

        public override void UndoTask()
        {
            AttachmentManager.main.SetEnabledAttachment(attachment, previouslyEnabled);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Enable/Disable attachment";
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