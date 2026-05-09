using System;
using UnityEngine;

namespace VSMC
{
    public class TaskSetBackdropColorIndex : IEditTask
    {

        public string backdropPath;
        public int newColorIndex;
        public int oldColorIndex;

        public TaskSetBackdropColorIndex(LoadedBackdrop backdrop, int newColorIndex)
        {
            this.backdropPath = backdrop.data.shapeFilepath;
            this.newColorIndex = newColorIndex;
            this.oldColorIndex = backdrop.data.flatColorIndex;
        }

        public override void DoTask()
        {
            BackdropManager.main.GetBackdropFromPath(backdropPath).SetColorIndex(newColorIndex);
            BackdropAndAttachmentMenuManager.main.RefreshUIElements();
        }

        public override void UndoTask()
        {
            BackdropManager.main.GetBackdropFromPath(backdropPath).SetColorIndex(oldColorIndex);
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
            return "Set Backdrop Color Index";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}