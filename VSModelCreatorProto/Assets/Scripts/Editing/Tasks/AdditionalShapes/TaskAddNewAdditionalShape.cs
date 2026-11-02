using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VSMC;

public class TaskAddNewAdditionalShape : IEditTask
{
    public string localPathToShape;
    public List<string> rootElements;

    public List<LoadedAdditionalShape> shapes;
    public List<int> indices = new List<int>();

    public TaskAddNewAdditionalShape(string localFilePath)
    {
        localFilePath = localFilePath.Replace(".json", "", System.StringComparison.OrdinalIgnoreCase);
        localPathToShape = localFilePath;
        string globalPath = AssetPathManager.main.FindBackdropOrAttachmentShapeFilePath(localFilePath + ".json");

        //Need to load the shape and access the root elements.
        Shape shape = ShapeAccessor.DeserializeShapeFromFile(globalPath, JSONStreamingContexts.backdropOrAttachment);
        rootElements = new List<string>();
        foreach (ShapeElement e in shape.Elements)
        {
            rootElements.Add(e.Name);
        }

        List<AdditionalShapeData> adds = new List<AdditionalShapeData>();
        foreach (string s in rootElements)
        {
            adds.Add(new AdditionalShapeData()
            {
                shapeFilepath = localPathToShape,
                rootShapeElementCode = s,
                enabled = true,
                opacity = 1
            });
        }
        
        shapes = new List<LoadedAdditionalShape>();
        foreach (AdditionalShapeData add in adds)
        {
            shapes.Add(MultiShapeManager.main.LoadAdditionalShapeFromData(add, false, false));
        }
        foreach (LoadedAdditionalShape add in shapes)
        {
            MultiShapeManager.main.AttemptToAutoAssignOriginPointsForRootElement(add.loadedAdditionalShape.Elements[0], true);
        }

        //Now remove them...
        indices = new List<int>();
        foreach (LoadedAdditionalShape s in shapes)
        {
            indices.Add(MultiShapeManager.main.RemoveLoadedAdditionalShapeAndGetIndex(s));
        }
    }

    public override void DoTask()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            MultiShapeManager.main.ReAddRemovedAdditionalShape(shapes[i], indices[i]);
        }
        MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects(false);
        MultiShapeManager.main.UI.RecreateHierarchy();
    }

    public override void UndoTask()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            MultiShapeManager.main.RemoveLoadedAdditionalShapeAndGetIndex(shapes[i]);
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
        return "Add Additional Shape";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
