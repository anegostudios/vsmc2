using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VSMC;

public class MultiShapeManager : MonoBehaviour
{

    public static MultiShapeManager main;
    public List<ShapeElement> storedTessellationOrder;

    int prevUID = 0;

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
        shape.shapeUID = 0;
        RecalculateAndApplyTransformsForAllObjects();
    }

    void BeforeShapeSave(Shape shape)
    {

    }

    void OnEditModeSelected(VSEditMode editMode)
    {

    }

    public Shape GetShapeFromUID(int uid)
    {
        if (uid == 0) return ShapeHolder.CurrentLoadedShape;
    }

    public void RecalculateAndApplyTransformsForAllObjects()
    {
        RecalculateTessellationOrderAndDependencies();
        foreach (ShapeElement e in storedTessellationOrder)
        {
            ShapeTesselator.ResolveAllLocalMatricesForRootShapeElement(e);
            e.gameObject.ReapplyTransformsFromMeshData(true, false, true);
        }
    }

    public void RecalculateTessellationOrderAndDependencies()
    {
        List<ShapeElement> rootElems = GetAllRootShapeElements();

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
            if (elem.OriginPoint != null && elem.OriginPoint.DependsOnOtherRootShapeElement())
            {
                elem.OriginPoint.GetRootShapeElementThisDependsOn().DependentRootElements.Add(elem);
            }
        }

        //And calculate the tesselation order.
        storedTessellationOrder = GetTessellationOrderOfRootObjects(GetAllRootShapeElements());
    }
    
    /// <summary>
    /// Creates a list of all root shape elements that need to be tesselated.
    /// </summary>
    /// <returns></returns>
    public List<ShapeElement> GetAllRootShapeElements()
    {
        List<ShapeElement> rootElements = new List<ShapeElement>();
        rootElements.AddRange(ShapeHolder.CurrentLoadedShape.Elements);
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
            if (elem.OriginPoint != null && elem.OriginPoint.DependsOnOtherRootShapeElement())
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
                ShapeElement eRely = elem.OriginPoint.GetRootShapeElementThisDependsOn();
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


}
