using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VSMC;

public class TaskRemoveAdditionalShape : IEditTask
{
    public string localPathToShape;
    public List<LoadedAdditionalShape> shapes;
    public List<int> indices = new List<int>();

    public TaskRemoveAdditionalShape(Shape shape)
    {
        localPathToShape = shape.shapeLoadedFromPath;
        shapes = MultiShapeManager.main.GetAllLoadedAdditionalShapesForShapePath(localPathToShape).ToList();
        foreach (LoadedAdditionalShape s in shapes)
        {
            indices.Add(MultiShapeManager.main.registeredAdditionalShapes.IndexOf(s));
        }
    }

    public override void DoTask()
    {
        foreach (LoadedAdditionalShape shape in shapes)
        {
            MultiShapeManager.main.RemoveLoadedAdditionalShapeAndGetIndex(shape);
        }
        MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects(false);
        MultiShapeManager.main.UI.RecreateHierarchy();
    } 

    public override void UndoTask()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            MultiShapeManager.main.ReAddRemovedAdditionalShape(shapes[i], indices[i]);
        }
        MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects(false);
        MultiShapeManager.main.UI.RecreateHierarchy();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Remove Additional Shape";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
