using UnityEngine;
using VSMC;

public class TaskSetRoundUVMode : IEditTask
{
    bool newRoundUVMode;

    public TaskSetRoundUVMode(bool newRoundUVMode)
    {
        this.newRoundUVMode = newRoundUVMode;
    }

    public override void DoTask()
    {
        ShapeHolder.CurrentLoadedShape.editor.roundAutoUVs = newRoundUVMode;
        foreach (ShapeElement e in ShapeElementRegistry.main.GetAllShapeElements())
        {
            e.ResolveUVForFaces();
        }
    }

    public override void UndoTask()
    {
        ShapeHolder.CurrentLoadedShape.editor.roundAutoUVs = !newRoundUVMode;
        foreach (ShapeElement e in ShapeElementRegistry.main.GetAllShapeElements()) //Resolving the UVs is a reversible process!
        {
            e.ResolveUVForFaces();
        }
    }

    public override VSEditMode GetRequiredEditMode()
    {
        //Despite being a texture change, we don't need to be in texture mode for this.
        return VSEditMode.None;
    }

    public override string GetTaskName()
    {
        return "Toggle Round UV Mode";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
