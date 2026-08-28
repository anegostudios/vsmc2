using UnityEngine;
using VSMC;

public class TaskSetAttachmentPointParent : IEditTask
{

    public string apCode;
    public int oldParentUID;
    public int newParentUID;

    public TaskSetAttachmentPointParent(LoadedAP loadedAP, ShapeElement newParent)
    {
        this.apCode = loadedAP.code;
        oldParentUID = loadedAP.shapeElementParentUID;
        newParentUID = newParent.elementUID;
    }

    public override void DoTask()
    {
        AttachmentPointsManager.main.GetLoadedAPFromCode(apCode).shapeElementParentUID = newParentUID;
        AttachmentPointsManager.main.RecreateAPList();
    }

    public override void UndoTask()
    {
        AttachmentPointsManager.main.GetLoadedAPFromCode(apCode).shapeElementParentUID = oldParentUID;
        AttachmentPointsManager.main.RecreateAPList();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set AP Parent Element";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
