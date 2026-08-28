using UnityEngine;
using VSMC;

public class TaskSetAttachmentPointRotation : IEditTask
{

    public string apCode;
    public Vector3 oldRot;
    public Vector3 newRot;

    public TaskSetAttachmentPointRotation(LoadedAP loadedAP, Vector3 newRot)
    {
        this.apCode = loadedAP.code;
        oldRot = loadedAP.rotation;
        this.newRot = newRot;
    }

    public override void DoTask()
    {
        AttachmentPointsManager.main.GetLoadedAPFromCode(apCode).rotation = newRot;
        AttachmentPointsManager.main.UpdateAPValues();
    }

    public override void UndoTask()
    {
        AttachmentPointsManager.main.GetLoadedAPFromCode(apCode).rotation = oldRot;
        AttachmentPointsManager.main.UpdateAPValues();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set AP Rotation";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
