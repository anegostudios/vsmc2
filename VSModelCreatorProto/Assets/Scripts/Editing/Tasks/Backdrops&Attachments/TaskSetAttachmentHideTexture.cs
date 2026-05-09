
using UnityEngine;
using VSMC;

public class TaskSetAttachmentHideTexture : IEditTask
{

    public string attachmentPath;
    public bool newHideTexture;
    public bool oldHideTexture;

    public TaskSetAttachmentHideTexture(LoadedAttachment attachment, bool newHideTexture)
    {
        this.attachmentPath = attachment.data.shapeFilepath;
        this.newHideTexture = newHideTexture;
        this.oldHideTexture = !attachment.data.hideTextures;
    }

    public override void DoTask()
    {
        AttachmentManager.main.GetAttachmentFromPath(attachmentPath).SetHideTextures(newHideTexture);
        BackdropAndAttachmentMenuManager.main.RefreshUIElements();
    }

    public override void UndoTask()
    {
        AttachmentManager.main.GetAttachmentFromPath(attachmentPath).SetHideTextures(oldHideTexture);
        BackdropAndAttachmentMenuManager.main.RefreshUIElements();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override long GetSizeOfTaskInBytes()
    {
        return 2;
    }

    public override string GetTaskName()
    {
        return "Set Attachment Hide Textures";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
