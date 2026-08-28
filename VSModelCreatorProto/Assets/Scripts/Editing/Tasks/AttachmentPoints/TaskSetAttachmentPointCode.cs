using UnityEngine;
using VSMC;

public class TaskSetAttachmentPointCode : IEditTask
{

    public string oldCode;
    public string newCode;

    public TaskSetAttachmentPointCode(LoadedAP loadedAP, string code)
    {
        this.oldCode = loadedAP.code;
        this.newCode = code;
    }

    public override void DoTask()
    {
        AttachmentPointsManager.main.GetLoadedAPFromCode(oldCode).code = newCode;
        AttachmentPointsManager.main.RecreateAPList();
    }

    public override void UndoTask()
    {
        AttachmentPointsManager.main.GetLoadedAPFromCode(newCode).code = oldCode;
        AttachmentPointsManager.main.RecreateAPList();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set AP Code";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
