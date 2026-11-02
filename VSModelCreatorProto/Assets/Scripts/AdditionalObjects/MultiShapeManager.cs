using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using VSMC;

public class MultiShapeManager : MonoBehaviour
{

    public static MultiShapeManager main;

    [Header("Unity Refs")]
    public MultiShapeUIElements UI;
    public GameObject additionalShapePrefab;
    public Color[] shapeColors;

    [Header("Runtime")]
    public List<ShapeElement> storedTessellationOrder;
    public List<LoadedAdditionalShape> registeredAdditionalShapes = new List<LoadedAdditionalShape>();
    public List<LoadedAdditionalShape> removedAdditionalShapes = new List<LoadedAdditionalShape>();

    void Awake()
    {
        main = this;
    }

    void Start()
    {
        //Sign up for events.
        ShapeLoader.RegisterForOnShapeLoadEvent(OnEditingShapeLoaded);
        ShapeLoader.RegisterForOnShapeSaveEvent(BeforeShapeSave);
        EditModeManager.RegisterForOnModeSelect(OnEditModeSelected);
    }

    void OnEditingShapeLoaded(Shape shape, LoadingContext context)
    {

        if (context != null && context is LoadWithAdditionalShapesContext addContext)
        {
            OnEditingShapeLoadedWithContext(shape, addContext);
        }

        AttachmentPointsManager.main.EnsureAPsAreLoaded();
        foreach (LoadedAdditionalShape toDel in registeredAdditionalShapes)
        {
            GameObject.Destroy(toDel.attachmentHolder.gameObject);
        }
        foreach (LoadedAdditionalShape toDel in removedAdditionalShapes)
        {
            GameObject.Destroy(toDel.attachmentHolder.gameObject);
        }
        registeredAdditionalShapes = new List<LoadedAdditionalShape>();
        removedAdditionalShapes = new List<LoadedAdditionalShape>();
        shape.shapeLoadedFromPath = "Current Shape";

        //Load additional shapes.
        if (shape.editor.additionalShapes != null)
        {
            foreach (AdditionalShapeData d in shape.editor.additionalShapes)
            {
                Debug.Log("Loading shape " + d.rootShapeElementCode + " in " + d.shapeFilepath);
                LoadAdditionalShapeFromData(d, false, false); //Do not find origins yet.
            }
        }
        
#pragma warning disable CS0618 // Type or member is obsolete
        if (shape.editor.backDropShape != null)
        {
            LoadAdditionalShapesFromBackdropShapeData(shape.editor.backDropShape);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        //Resolve origins for additional shapes
        //This cannot be done immediately after loading, as that may result in some origin points not being found.
        foreach (LoadedAdditionalShape s in registeredAdditionalShapes)
        {
            AttemptToAutoAssignOriginPointsForRootElement(s.loadedAdditionalShape.Elements[0]);
        }

        //Resolve origins for shape root elements.
        for (int i = 0; i < shape.Elements.Length; i++)
        {
            ShapeElement root = shape.Elements[i];
            if (shape.editor.rootElementOrigins != null && shape.editor.rootElementOrigins[i] != null)
            {
                root.OriginPoint = shape.editor.rootElementOrigins[i].ConvertToOriginPoint();
            }
            AttemptToAutoAssignOriginPointsForRootElement(root);
        }

        UI.RecreateHierarchy();
        RecalculateAndApplyTransformsForAllObjects();
    }
    
    void OnEditingShapeLoadedWithContext(Shape shape, LoadWithAdditionalShapesContext context)
    {
        shape.editor.additionalShapes = context.newAdditionalShapes.ToArray();
#pragma warning disable CS0618 // Type or member is obsolete
        shape.editor.backDropShape = null;
#pragma warning restore CS0618 // Type or member is obsolete
        OriginPointData[] rootOrigins = new OriginPointData[shape.Elements.Length];
        for (int i = 0; i < shape.Elements.Length; i++)
        {
            rootOrigins[i] = context.rootElementOriginPoints[shape.Elements[i].Name];
        }
        shape.editor.rootElementOrigins = rootOrigins;
    }

    void LoadAdditionalShapesFromBackdropShapeData(string backdropShapePath)
    {
        string resolved = AssetPathManager.main.FindBackdropOrAttachmentShapeFilePath(backdropShapePath+".json");
        Shape s = ShapeAccessor.DeserializeShapeFromFile(resolved, JSONStreamingContexts.backdropOrAttachment);
        List<AdditionalShapeData> adds = new List<AdditionalShapeData>();
        foreach (ShapeElement se in s.Elements)
        {
            adds.Add(new AdditionalShapeData()
            {
                shapeFilepath = backdropShapePath,
                rootShapeElementCode = se.Name,
                enabled = true,
                opacity = 1
            });
        }
        foreach (AdditionalShapeData add in adds)
        {
            LoadAdditionalShapeFromData(add, false, false);
        }
    }

    void BeforeShapeSave(Shape shape)
    {
        shape.editor.rootElementOrigins = new OriginPointData[shape.Elements.Length];
        for (int i = 0; i < shape.Elements.Length; i++)
        {
            if (shape.Elements[i].OriginPoint != null)
            {
                shape.editor.rootElementOrigins[i] = shape.Elements[i].OriginPoint.SaveToOriginData();
            }
        }

        if (registeredAdditionalShapes.Count == 0)
        {
            shape.editor.additionalShapes = null;
            return;
        }
        List<AdditionalShapeData> addShapes = new List<AdditionalShapeData>();
        foreach (LoadedAdditionalShape s in registeredAdditionalShapes)
        {
            s.data.ResolveBeforeSerialization(ShapeHolder.CurrentLoadedShape, s.loadedAdditionalShape);
            addShapes.Add(s.data);
        }
        shape.editor.additionalShapes = addShapes.ToArray();

    }

    void OnEditModeSelected(VSEditMode editMode)
    {
        //Animation Mode doesn't need to be recalculated as it does it in the editor manager.
        if (editMode != VSEditMode.Animation)
        {
            RecalculateAndApplyTransformsForAllObjects();
        }
    }

    #region Editing Management

    public LoadedAdditionalShape LoadAdditionalShapeFromData(AdditionalShapeData data, bool findOrigins = true, bool doUI = true)
    {
        LoadedAdditionalShape shape = new LoadedAdditionalShape(data, Instantiate(additionalShapePrefab, transform).GetComponent<AdditionalShapeHolder>());
        registeredAdditionalShapes.Add(shape);
        AttachmentPointsManager.main.OnAdditionalShapeLoaded(shape.loadedAdditionalShape);
        if (findOrigins)
        {
            AttemptToAutoAssignOriginPointsForRootElement(shape.loadedAdditionalShape.Elements[0], true);
        }
        if (doUI)
        {
            UI.RecreateHierarchy();
        }
        return shape;
    }

    public void OnElementVisibilityRecalculated(ShapeElement elem)
    { 
        if (elem.GetRootElement() == elem)
        {
            //If root element, check the hierarchy.
            MultiShapeUIListEntry entry = UI.GetEntryFromRootShapeElement(elem);
            if (entry != null)
            {
                entry.ResolveEnabledState(!elem.ShouldHideFromView());
            }
        }
        if (EditModeManager.main.cEditMode == VSEditMode.Model)
        {
            RecalculateAndApplyTransformsForAllObjects();
        }
    }

    /// <summary>
    /// Checks if one root element can be connected to another. 
    /// If otherElem is a descendant of (or equal to) rootElem, then this is false.
    /// </summary>
    public bool CanRootElementBeAttachedToOther(ShapeElement rootElem, ShapeElement otherElem)
    {
        ShapeElement otherRoot = otherElem.GetRootElement();
        while (otherRoot.OriginPoint != null && otherRoot.OriginPoint.DependsOnOtherShapeElement())
        {
            if (otherRoot == rootElem) return false;
            otherRoot = otherRoot.OriginPoint.GetShapeElementThisDependsOn().GetRootElement();
        }
        if (otherRoot == rootElem) return false;
        return true;
    }

    /// <summary>
    /// Removes a loaded shape, but does not delete the data. Returns the index it was removed from. Does not trigger any further events.
    /// </summary>
    public int RemoveLoadedAdditionalShapeAndGetIndex(LoadedAdditionalShape shape)
    {
        shape.attachmentHolder.HideOutlines();
        shape.attachmentHolder.gameObject.SetActive(false);
        int i = registeredAdditionalShapes.IndexOf(shape);
        registeredAdditionalShapes.RemoveAt(i);
        AttachmentPointsManager.main.OnAdditionalShapeRemoved(shape.loadedAdditionalShape);
        removedAdditionalShapes.Add(shape);
        return i;
    }

    /// <summary>
    /// Adds a previously-removed additional shape. Does not trigger any further events.
    /// </summary>
    public void ReAddRemovedAdditionalShape(LoadedAdditionalShape shape, int index)
    {
        removedAdditionalShapes.Remove(shape);
        shape.attachmentHolder.gameObject.SetActive(shape.data.enabled);
        AttachmentPointsManager.main.OnAdditionalShapeLoaded(shape.loadedAdditionalShape);
        registeredAdditionalShapes.Insert(index, shape);
    }

    /// <summary>
    /// Permanently deletes an additional shape from the file. Does not trigger any further events.
    /// </summary>
    public void DeleteAdditionalShapePermanently(LoadedAdditionalShape shape)
    {
        Destroy(shape.attachmentHolder.gameObject);
        registeredAdditionalShapes.Remove(shape);
    }

    public void SetOriginOfShapeElementToOtherShapeElement(ShapeElement toMove, ShapeElement newStepParent)
    {
        StepparentOriginPoint newOP = new StepparentOriginPoint()
        {
            elemCode = new ElementCodeInShape(newStepParent.parentShape.shapeLoadedFromPath, newStepParent.Name)
        };

        if (toMove.OriginPoint != null)
        {
            toMove.OriginPoint.CopyTransformValuesTo(newOP);
        }

        if (!newOP.AttemptToResolveOriginPoint())
        {
            Debug.LogError("Failed to set origin point? How odd.");
            return;
        }

        TaskSetOriginPoint setOPTask = new TaskSetOriginPoint(toMove, toMove.OriginPoint, newOP);
        setOPTask.DoTask();
        UndoManager.main.CommitTask(setOPTask);
    }

    public void SetOriginOfShapeElementToAttachmentPoint(ShapeElement toMove, LoadedAP ap)
    {
        AttachmentPointOriginPoint newOP = new AttachmentPointOriginPoint()
        {
            apCode = ap.code,
            rootElemCode = new ElementCodeInShape(ap.GetElement().parentShape.shapeLoadedFromPath, ap.GetElement().Name)
        };
        
        if (toMove.OriginPoint != null)
        {
            toMove.OriginPoint.CopyTransformValuesTo(newOP);
        }

        if (!newOP.AttemptToResolveOriginPoint())
        {
            Debug.LogError("Failed to set origin point? How odd.");
            return;
        }

        TaskSetOriginPoint setOPTask = new TaskSetOriginPoint(toMove, toMove.OriginPoint, newOP);
        setOPTask.DoTask();
        UndoManager.main.CommitTask(setOPTask);
    }

    public void RemoveOriginOfShapeElement(ShapeElement elem)
    {
        TransformOnlyOriginPoint newOP = new TransformOnlyOriginPoint();
        if (elem.OriginPoint != null)
        {
            elem.OriginPoint.CopyTransformValuesTo(newOP);
        }
        TaskSetOriginPoint setOPTask = new TaskSetOriginPoint(elem, elem.OriginPoint, newOP);
        setOPTask.DoTask();
        UndoManager.main.CommitTask(setOPTask);
    }

    #endregion

    public LoadedAdditionalShape GetLoadedAdditionalShapeFromRootElementCode(ElementCodeInShape code)
    {
        return registeredAdditionalShapes.FirstOrDefault(x => x.data.shapeFilepath == code.shapePath && x.data.rootShapeElementCode == code.element);
    }

    public IEnumerable<LoadedAdditionalShape> GetAllLoadedAdditionalShapesForShapePath(string path)
    {
        return registeredAdditionalShapes.Where(x => x.loadedAdditionalShape.shapeLoadedFromPath.Equals(path, System.StringComparison.OrdinalIgnoreCase));
    }

    public Shape GetShapeFromRootElementCodeInShape(ElementCodeInShape code)
    {
        List<ShapeElement> rootElems = GetAllRootShapeElements();
        return rootElems.FirstOrDefault(x => x.parentShape.shapeLoadedFromPath.Equals(code.shapePath, System.StringComparison.OrdinalIgnoreCase)).parentShape;
    }

    public ShapeElement GetShapeElementFromRootElementCodeInShape(ElementCodeInShape code)
    {
        List<ShapeElement> elems = GetAllRootShapeElements();
        while (elems.Count > 0)
        {
            ShapeElement e = elems[0];
            elems.RemoveAt(0);
            if (e.Name.Equals(code.element, System.StringComparison.OrdinalIgnoreCase) && e.parentShape.shapeLoadedFromPath.Equals(code.shapePath, System.StringComparison.OrdinalIgnoreCase))
            {
                return e;
            }
            if (e.Children != null)
            {
                elems.AddRange(e.Children);
            }
        }
        return null;
    }

    public ShapeElement GetFirstShapeElementFromElementCodeOnly(string code)
    {
        List<ShapeElement> elems = GetAllRootShapeElements();
        while (elems.Count > 0)
        {
            ShapeElement e = elems[0];
            elems.RemoveAt(0);
            if (e.Name.Equals(code, System.StringComparison.OrdinalIgnoreCase))
            {
                return e;
            }
            if (e.Children != null)
            {
                elems.AddRange(e.Children);
            }
        }
        return null;
    }

    public void AttemptToAutoAssignOriginPointsForRootElement(ShapeElement root, bool retesellateAfter = false)
    {
        if (!root.OriginPoint.AttemptToResolveOriginPoint())
        {
            //The loaded origin point failed to find. 
            TransformOnlyOriginPoint correctedOrigin = new TransformOnlyOriginPoint();
            root.OriginPoint.CopyTransformValuesTo(correctedOrigin);
            root.OriginPoint = correctedOrigin;
            Debug.LogWarning("Failed to load the original origin point for root element " + root.Name+". Replacing with a transform-only origin point. Transform data is maintained.");
        }
        if (root.OriginPoint is TransformOnlyOriginPoint)
        {
            if (root.StepParentName != null && root.StepParentName != "")
            {
                Debug.Log("Attempting to find stepparent "+root.StepParentName+ " for element " + root.Name);
                ShapeElement step = GetFirstShapeElementFromElementCodeOnly(root.StepParentName);
                if (step != null)
                {
                    StepparentOriginPoint spOrigin = new StepparentOriginPoint() { elemCode = step.GetElementCodeInShape() };
                    root.OriginPoint.CopyTransformValuesTo(spOrigin);
                    if (spOrigin.AttemptToResolveOriginPoint())
                    {
                        root.OriginPoint = spOrigin;
                    }
                    else
                    {
                        throw new System.Exception("Something has gone very wrong while searching for a step-parent element. Please save your model in a new file, and reload the program. "
                        +"If you can reproduce this, please let Nat know.");
                    }
                }
                else
                {
                    Debug.Log("Oops! Couldn't find a matching step-parent. Keeping it as a transform-only origin point...");
                }
            }
        }

        if (retesellateAfter)
        {
            RecalculateAndApplyTransformsForAllObjects();
        }
    }

    public void RecalculateAndApplyTransformsForAllObjects(bool forAnimMode = false)
    {
        RecalculateTessellationOrderAndDependencies();
        foreach (ShapeElement e in storedTessellationOrder)
        {
            ShapeTesselator.ResolveAllLocalMatricesForRootShapeElement(e);
            e.gameObject.ReapplyTransformsFromMeshData(true, false, !forAnimMode);
        }
    }

    public void RecalculateTessellationOrderAndDependencies()
    {
        EnsureAllRootElementsHaveOriginPoints();
        List<ShapeElement> rootElems = GetAllRootShapeElements(true);

        //First need to clear the dependencies from all elements.
        List<ShapeElement> temp = new List<ShapeElement>();
        temp.AddRange(rootElems);
        while (temp.Count > 0)
        {
            ShapeElement elem = temp[0];
            temp.RemoveAt(0);
            elem.DependentRootElements = new List<ShapeElement>();
            if (elem.Children != null) temp.AddRange(elem.Children);
        }

        //Then add the new ones back.
        foreach (ShapeElement elem in rootElems)
        {
            if (elem.OriginPoint != null && elem.OriginPoint.DependsOnOtherShapeElement())
            {
                elem.OriginPoint.GetShapeElementThisDependsOn().DependentRootElements.Add(elem);
            }
        }

        //And calculate the tesselation order.
        storedTessellationOrder = GetTessellationOrderOfRootObjects(rootElems);
    }
    
    /// <summary>
    /// Creates a list of all root shape elements that need to be tesselated.
    /// </summary>
    /// <returns></returns>
    public List<ShapeElement> GetAllRootShapeElements(bool includeRemoved = false)
    {
        List<ShapeElement> rootElements = new List<ShapeElement>();
        rootElements.AddRange(ShapeHolder.CurrentLoadedShape.Elements);
        foreach (LoadedAdditionalShape additionalShape in registeredAdditionalShapes)
        {
            rootElements.AddRange(additionalShape.loadedAdditionalShape.Elements);
        }
        if (includeRemoved) //For undo purposes, we need to keep the root elements that have been deleted but may be restored.
        {
            foreach (LoadedAdditionalShape add in removedAdditionalShapes)
            {
                rootElements.AddRange(add.loadedAdditionalShape.Elements);
            }
        }
        return rootElements;
    }

    /// <summary>
    /// Returns the order in which root shape elements should be tessellated. 
    /// </summary>
    List<ShapeElement> GetTessellationOrderOfRootObjects(List<ShapeElement> rootObjects)
    {
        Dictionary<ShapeElement, int> priorities = new Dictionary<ShapeElement, int>();
        List<ShapeElement> nonDependentObjects = new List<ShapeElement>();
        nonDependentObjects.AddRange(rootObjects);

        foreach (ShapeElement elem in rootObjects)
        {
            if (elem.OriginPoint != null && elem.OriginPoint.DependsOnOtherShapeElement())
            {
                nonDependentObjects.Remove(elem);
            }
        }

        foreach (ShapeElement zero in nonDependentObjects)
        {
            priorities[zero] = 0;
        }

        int cycles = 0;
        int maxCycles = 16;
        while (rootObjects.Count > priorities.Count && cycles < maxCycles)
        {
            foreach (ShapeElement elem in rootObjects)
            {
                if (priorities.ContainsKey(elem)) continue;
                ShapeElement eRely = elem.OriginPoint.GetShapeElementThisDependsOn().GetRootElement();
                if (priorities.ContainsKey(eRely))
                {
                    priorities[elem] = priorities[eRely] + 1;
                }
            }
            cycles++;
        }

        //Ensure all elements are going to be tessellated, even if we overflow...
        if (rootObjects.Count > priorities.Count)
        {
            Debug.LogWarning("Overflow detected when deciding tessellation order. This may happen if using a large number of attachments.");
            foreach (ShapeElement e in rootObjects)
            {
                if (priorities.ContainsKey(e)) continue;
                priorities[e] = cycles;
            }
        }

        storedTessellationOrder = new List<ShapeElement>();
        foreach (var elem in priorities.OrderBy(x => x.Value))
        {
            storedTessellationOrder.Add(elem.Key);
        }

        return storedTessellationOrder;
    }

    public Color GetColorFromIndex(int colorIndex)
    {
        return shapeColors[colorIndex];
    }

    public void OpenShapeInContextButtonClicked(ShapeElement cSelected)
    {
        //The user must save before continuing. 
        SaveOverlayManager.main.OpenSaveOverlayWithFunctionsForMustSave(null,() => { OpenShapeInContext(cSelected); }, "Open Shape In Context",
        "This will open the selected shape as its own file, maintaining all existing shapes and origin points.\n\nYou must save to continue.",
         false, true);
    }

    private void OpenShapeInContext(ShapeElement selectedRoot)
    {
        //Here goes...
        //The user *must* save before continuing.

        string localSavePath = AssetPathManager.main.GetRelativePathForFile(ShapeLoader.main.storedSaveLocationForFile, "shapes");
        localSavePath = localSavePath.Replace(".json", "");

        List<AdditionalShapeData> currentAdditionalShapes = ShapeHolder.CurrentLoadedShape.editor.additionalShapes.ToList();
        OriginPointData[] currentEditingOriginPoints = ShapeHolder.CurrentLoadedShape.editor.rootElementOrigins;

        //Add all the root shapes into the additional shapes array.
        for (int i = 0; i < currentEditingOriginPoints.Length; i++)
        {
            OriginPointData origin = currentEditingOriginPoints[i];
            ShapeElement rootElem = ShapeHolder.CurrentLoadedShape.Elements[i];
            AdditionalShapeData newData = new AdditionalShapeData()
            {
                shapeFilepath = localSavePath,
                rootShapeElementCode = rootElem.Name,
                origin = origin,
                enabled = !rootElem.ShouldHideFromView()
            };
            currentAdditionalShapes.Add(newData);
        }

        //Resolve "Current Shape" to the local path and the new local path to "Current Shape". 
        foreach (AdditionalShapeData ad in currentAdditionalShapes)
        {
            if (ad.origin != null && ad.origin.rootElement != null)
            {
                if (ad.origin.rootElement.shapePath == "Current Shape")
                {
                    ad.origin.rootElement.shapePath = localSavePath;
                }
                else if (ad.origin.rootElement.shapePath == selectedRoot.parentShape.shapeLoadedFromPath)
                {
                    ad.origin.rootElement.shapePath = "Current Shape";
                }
            }
        }

        //Now create the local root origin points.
        //There's no gaurantee that the loaded elements are in the same order as the root elements in the file, so we need to map the name to the origin point.
        Dictionary<string, OriginPointData> newRootElemsToOriginPoints = new Dictionary<string, OriginPointData>();
        foreach (LoadedAdditionalShape toLoad in GetAllLoadedAdditionalShapesForShapePath(selectedRoot.parentShape.shapeLoadedFromPath))
        {
            newRootElemsToOriginPoints.Add(toLoad.loadedAdditionalShape.Elements[0].Name, toLoad.data.origin);
        }

        //Remove the additional shapes for the new editing shape.
        currentAdditionalShapes.RemoveAll(x => { return x.shapeFilepath == selectedRoot.parentShape.shapeLoadedFromPath; });

        string pathToShape = AssetPathManager.main.FindBackdropOrAttachmentShapeFilePath(selectedRoot.parentShape.shapeLoadedFromPath + ".json");
        LoadWithAdditionalShapesContext loadContext = new LoadWithAdditionalShapesContext(currentAdditionalShapes, newRootElemsToOriginPoints);
        ShapeLoader.main.LoadShape(pathToShape, loadContext);
    }
    
    /// <summary>
    /// Annoying that I have to make this, but after various issues of origin points now loading correctly, I want to verify it.
    /// </summary>
    public void EnsureAllRootElementsHaveOriginPoints()
    {
        foreach (var v in GetAllRootShapeElements(true))
        {
            if (v.OriginPoint == null)
            {
                throw new System.Exception("Root element " + v.GetElementCodeInShape() + " does not have an origin point attached to it. This should not happen. "
                    + "Please save your model in a new file, and reload the program. If you can reproduce this error, please let Nat know.");
            }
        }
    }


}
