using System;
using UnityEngine;

namespace VSMC
{
    public class TaskSetBackdropOpacity : IEditTask
    {

        public string backdropPath;
        public float oldOpacity;
        public float newOpacity;

        public DateTime taskTime;

        public TaskSetBackdropOpacity(LoadedBackdrop backdrop, float oldOpacity, float newOpacity)
        {
            this.backdropPath = backdrop.data.shapeFilepath;
            this.oldOpacity = oldOpacity;
            this.newOpacity = newOpacity;
            taskTime = DateTime.Now;
        }

        public override void DoTask()
        {
            BackdropManager.main.GetBackdropFromPath(backdropPath).SetOpacity(newOpacity);
            BackdropAndAttachmentMenuManager.main.RefreshUIElements();
        }

        public override void UndoTask()
        {
            BackdropManager.main.GetBackdropFromPath(backdropPath).SetOpacity(oldOpacity);
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
            return "Set Backdrop Opacity";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            if (nextTask is TaskSetBackdropOpacity o2)
            {
                if (o2.backdropPath == backdropPath && (o2.taskTime - taskTime).TotalSeconds < 2)
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