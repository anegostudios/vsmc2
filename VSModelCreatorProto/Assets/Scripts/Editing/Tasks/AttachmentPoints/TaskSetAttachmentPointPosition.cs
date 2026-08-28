using UnityEngine;
using VSMC;

public class TaskSetAttachmentPointPosition : IEditTask
{

    public string apCode;
    public Vector3 oldPos;
    public Vector3 newPos;

    public TaskSetAttachmentPointPosition(LoadedAP loadedAP, Vector3 newPos)
    {
        this.apCode = loadedAP.code;
        oldPos = loadedAP.position;
        this.newPos = newPos;
    }

    public override void DoTask()
    {
        AttachmentPointsManager.main.GetLoadedAPFromCode(apCode).position = newPos;
        AttachmentPointsManager.main.UpdateAPValues();
    }

    public override void UndoTask()
    {
        AttachmentPointsManager.main.GetLoadedAPFromCode(apCode).position = oldPos;
        AttachmentPointsManager.main.UpdateAPValues();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set AP Position";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
