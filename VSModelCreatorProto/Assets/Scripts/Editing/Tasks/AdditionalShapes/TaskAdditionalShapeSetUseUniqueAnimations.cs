using UnityEngine;
using VSMC;

public class TaskAdditionalShapeSetUseUniqueAnimations : IEditTask
{

    ElementCodeInShape rootElementCode;
    bool newUseUniqueAnimations;
    bool oldUseUniqueAnimations;

    public TaskAdditionalShapeSetUseUniqueAnimations(ShapeElement forRootElement, bool newValue)
    {
        rootElementCode = forRootElement.GetElementCodeInShape();
        newUseUniqueAnimations = newValue;
        oldUseUniqueAnimations = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode).data.useUniqueAnimations;
    }

    public override void DoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode);
        las.data.useUniqueAnimations = newUseUniqueAnimations;
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override void UndoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode);
        las.data.useUniqueAnimations = oldUseUniqueAnimations;
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set 'Use Unique Animations' for Additional Shape";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
