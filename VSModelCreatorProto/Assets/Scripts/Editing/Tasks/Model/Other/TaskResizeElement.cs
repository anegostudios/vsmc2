using System.Collections.Generic;
using VSMC;

public class TaskResizeElement : IEditTask
{

    public Dictionary<int, bool[]> elementUIDsToChangeToAutoUVValues;
    public float scaleAmount;
    public bool scaleUVs;

    public TaskResizeElement(ShapeElement toScale, bool doChildren, float scaleAmount, bool scaleUVs)
    {
        this.scaleUVs = scaleUVs;
        this.scaleAmount = scaleAmount;
        elementUIDsToChangeToAutoUVValues = new Dictionary<int, bool[]>();
        List<ShapeElement> scaleElems = new List<ShapeElement>();
        if (toScale == null)
        {
            scaleElems.AddRange(ShapeElementRegistry.main.GetAllShapeElements());
        }
        else
        {
            if (!doChildren) scaleElems.Add(toScale);
            else
            {
                List<ShapeElement> a = new List<ShapeElement>();
                a.Add(toScale);
                while (a.Count > 0)
                {
                    ShapeElement e = a[0];
                    a.RemoveAt(0);
                    scaleElems.Add(e);
                    if (e.Children != null) a.AddRange(e.Children);
                }
            }
        }
        
        foreach (ShapeElement s in scaleElems)
        {
            bool[] autoUVs = new bool[6];
            for (int i = 0; i < 6; i++)
            {
                autoUVs[i] = s.FacesResolved[i].autoResolutionForUV;
            }
            elementUIDsToChangeToAutoUVValues.Add(s.elementUID, autoUVs);
        }
    }

    public override void DoTask()
    {

        foreach (Animation a in ShapeHolder.CurrentLoadedShape.Animations)
        {
            foreach (var kf in a.KeyFrames)
            {
                foreach (int i in elementUIDsToChangeToAutoUVValues.Keys)
                {
                    AnimationKeyFrameElement kfe = kf.GetKeyFrameElement(ShapeElementRegistry.main.GetShapeElementByUID(i));
                    if (kfe != null)
                    {
                        kfe.ScaleAll(scaleAmount);
                    }
                }
            }
        }

        foreach (int i in elementUIDsToChangeToAutoUVValues.Keys)
        {
            ShapeElement e = ShapeElementRegistry.main.GetShapeElementByUID(i);
            e.ScaleAll(scaleAmount, scaleUVs);
            e.RecreateObjectMeshAndTransforms();
        }
    }

    public override void UndoTask()
    {

        foreach (Animation a in ShapeHolder.CurrentLoadedShape.Animations)
        {
            foreach (var kf in a.KeyFrames)
            {
                foreach (int i in elementUIDsToChangeToAutoUVValues.Keys)
                {
                    AnimationKeyFrameElement kfe = kf.GetKeyFrameElement(ShapeElementRegistry.main.GetShapeElementByUID(i));
                    if (kfe != null)
                    {
                        kfe.ScaleAll(1f / scaleAmount);
                    }
                }
            }
        }

        foreach (var i in elementUIDsToChangeToAutoUVValues)
        {
            ShapeElement e = ShapeElementRegistry.main.GetShapeElementByUID(i.Key);
            e.ScaleAll(1f / scaleAmount, scaleUVs, i.Value);
            e.RecreateObjectMeshAndTransforms();
        }
        
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Resize Elements";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
