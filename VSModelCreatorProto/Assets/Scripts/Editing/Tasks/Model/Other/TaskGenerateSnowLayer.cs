using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using VSMC;

public class TaskGenerateSnowLayer : IEditTask
{

    Dictionary<int, ShapeElement> parentUIDToSnowElement;
    bool addedSnowTexture;

    public TaskGenerateSnowLayer()
    {
        List<ShapeElement> addSnowTo = new List<ShapeElement>();
        //Get elements to add snow to:
        foreach (ShapeElement e in ShapeElementRegistry.main.GetAllShapeElements())
        {
            if (Mathf.Abs((float)e.RotationX) < 15 && Mathf.Abs((float)e.RotationZ) < 15)
            {
                if (e.FacesResolved[4].Enabled && !e.Name.Contains("-snow"))
                {
                    if (e.Children == null || e.Children.FirstOrDefault(x => x.Name.Contains("-snow")) == null)
                    {
                        addSnowTo.Add(e);
                    }
                }
            }
        }

        parentUIDToSnowElement = new Dictionary<int, ShapeElement>();
        foreach (ShapeElement e1 in addSnowTo)
        {
            ShapeElement createdElement = new ShapeElement();
            createdElement.ResolveFacesAndTextures(TextureManager.main.loadedTextures);
            ShapeElementRegistry.main.UnregisterShapeElement(createdElement);
            createdElement.SetParent(e1);
            createdElement.From = new double[] { 0, e1.To[1] - e1.From[1] + 0.01, 0 };
            createdElement.To = new double[] { e1.To[0] - e1.From[0], e1.To[1] - e1.From[1] + 2, e1.To[2] - e1.From[2] };
            createdElement.Name = e1.Name + "-snow";
            createdElement.autoUnwrap = true;

            //The auto res and snap UV is likely to turn itself off...
            for (int i = 0; i < 6; i++)
            {
                createdElement.FacesResolved[i].Texture = "#snowcover";
                createdElement.FacesResolved[i].autoResolutionForUV = true;
                createdElement.FacesResolved[i].snapUV = true;
                createdElement.FacesResolved[i].ResolveTexture(TextureManager.main.loadedTextures);
            }


            //We will tesselate the object and create its game object, but will move it immediately to the deletion state.
            ShapeTesselator.TesselateShapeElements(new ShapeElement[] { createdElement });
            ShapeTesselator.ResolveMatricesForShapeElementAndChildren(createdElement);
            createdElement.ResolveUVForFaces();
            ShapeLoader.main.shapeHolder.CreateShapeElementGameObject(createdElement);
            ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(createdElement);
            createdElement.RemoveParent();
            parentUIDToSnowElement.Add(e1.elementUID, createdElement);
        }

        addedSnowTexture = TextureManager.main.loadedTextures.FirstOrDefault(x => x.code.Equals("snowcover", System.StringComparison.CurrentCultureIgnoreCase)) == null;
    }

    public override void DoTask()
    {
        if (addedSnowTexture)
        {
            LoadedTexture newTex = new LoadedTexture("snowcover", "block" + Path.DirectorySeparatorChar + "liquid" + Path.DirectorySeparatorChar + "snow" + Path.DirectorySeparatorChar + "normal1");
            newTex.LoadTextureFromCodeAndPath(ShapeHolder.CurrentLoadedShape);
            newTex.ResolveTextureSize(ShapeHolder.CurrentLoadedShape);
            TextureManager.main.loadedTextures.Add(newTex);
            TextureManager.main.RegenerateTextureArray();
        }

        foreach (var pair in parentUIDToSnowElement)
        {
            ShapeElement parent = ShapeElementRegistry.main.GetShapeElementByUID(pair.Key);
            pair.Value.SetParent(parent);
            ShapeElementRegistry.main.ReregisterShapeElement(pair.Value);
            ShapeLoader.main.shapeHolder.RestoreElementFromDeletionLimbo(pair.Value);
            //The auto res and snap UV is likely to turn itself off...
            for (int i = 0; i < 6; i++)
            {
                pair.Value.FacesResolved[i].Texture = "#snowcover";
                pair.Value.FacesResolved[i].autoResolutionForUV = true;
                pair.Value.FacesResolved[i].snapUV = true;
                pair.Value.FacesResolved[i].ResolveTexture(TextureManager.main.loadedTextures);
            }
            pair.Value.RecreateObjectMesh();
        }
        ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);

    }

    public override void UndoTask()
    {
        foreach (var pair in parentUIDToSnowElement)
        {
            pair.Value.RemoveParent();
            ShapeElementRegistry.main.UnregisterShapeElement(pair.Value);
            ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(pair.Value);
        }
        ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);

        if (addedSnowTexture)
        {
            TextureManager.main.loadedTextures.RemoveAll(x => x.code == "snowcover");
            TextureManager.main.RegenerateTextureArray();
        }

    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Generate Snow Layers";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
