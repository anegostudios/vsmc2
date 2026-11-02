using UnityEngine;
using VSMC;

public class TaskAdditionalShapeSetOpacity : IEditTask
{

    ElementCodeInShape rootElementCode;
    float newOpacity;
    float oldOpacity;

    public TaskAdditionalShapeSetOpacity(ShapeElement forRootElement, float newValue)
    {
        rootElementCode = forRootElement.GetElementCodeInShape();
        newOpacity = newValue;
        oldOpacity = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode).data.opacity;
    }

    public override void DoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode);
        las.SetOpacity(newOpacity);
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override void UndoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode);
        las.SetOpacity(oldOpacity);
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set Opacity of Additional Shape";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        if (nextTask is TaskAdditionalShapeSetOpacity t2)
        {
            if (t2.rootElementCode == rootElementCode && t2.oldOpacity == newOpacity)
            {
                newOpacity = t2.newOpacity;
                return true;
            }
        }
        return false;
    }
}
