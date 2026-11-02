using UnityEngine;
using VSMC;

public class TaskDeleteAttachmentPoint : IEditTask
{

    public LoadedAP ap;

    public TaskDeleteAttachmentPoint(LoadedAP loadedAP)
    {
        this.ap = loadedAP;
    }

    public override void DoTask()
    {
        AttachmentPointsManager.main.RemoveAP(ap);
        AttachmentPointsManager.main.OnAnyAPPropertyChanged();
    }

    public override void UndoTask()
    {
        AttachmentPointsManager.main.AddNewAP(ap);
        AttachmentPointsManager.main.OnAnyAPPropertyChanged();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Delete AP";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
