using System.Collections.Generic;
using UnityEngine;
using VSMC;

public class TaskRandomizeUVs : IEditTask
{

    public Dictionary<int, (Vector4[] faceUVsBefore, Vector4[] faceUVsAfter)> elementUIDsToUVCoords;

    /// <summary>
    /// Randomizes the UVs of a set of elements.
    /// </summary>
    /// <param name="randomizeOption"> 0 = selected only, 1 = selected + children, 2 = all</param>
    /// <param name="cSelected"></param>
    public TaskRandomizeUVs(int randomizeOption, ShapeElement cSelected)
    {
        elementUIDsToUVCoords = new Dictionary<int, (Vector4[] faceUVsBefore, Vector4[] faceUVsAfter)>();
        List<ShapeElement> elementsToRandomizeUVs = new List<ShapeElement>();
        if (randomizeOption == 0)
        {
            elementsToRandomizeUVs.Add(cSelected);
        }
        else if (randomizeOption == 1)
        {
            List<ShapeElement> toAdd = new List<ShapeElement>();
            toAdd.Add(cSelected);
            while (toAdd.Count > 0)
            {
                ShapeElement e = toAdd[0];
                toAdd.RemoveAt(0);
                elementsToRandomizeUVs.Add(e);
                if (e.Children != null) toAdd.AddRange(e.Children);
            }
        }
        else if (randomizeOption == 2)
        {
            elementsToRandomizeUVs.AddRange(ShapeElementRegistry.main.GetAllShapeElements());
        }

        foreach (ShapeElement elem in elementsToRandomizeUVs)
        {
            if (elem.autoUnwrap && ShapeHolder.CurrentLoadedShape.editor.entityTextureMode)
            {
                Vector4[] prevEntityTextureUVs = new Vector4[] { new Vector4((float)elem.entityTextureUV[0], (float)elem.entityTextureUV[1]) };
                double[] elemUvBounds = new double[] { double.MaxValue, double.MaxValue, double.MinValue, double.MinValue };

                for (int i = 0; i < 6; i++)
                {
                    if (elem.FacesResolved[i].Enabled)
                    {
                        if (elem.FacesResolved[i].Uv[0] < elemUvBounds[0]) elemUvBounds[0] = elem.FacesResolved[i].Uv[0];
                        if (elem.FacesResolved[i].Uv[1] < elemUvBounds[1]) elemUvBounds[1] = elem.FacesResolved[i].Uv[1];
                        if (elem.FacesResolved[i].Uv[2] > elemUvBounds[2]) elemUvBounds[2] = elem.FacesResolved[i].Uv[2];
                        if (elem.FacesResolved[i].Uv[3] > elemUvBounds[3]) elemUvBounds[3] = elem.FacesResolved[i].Uv[3];
                    }
                }
                if (elemUvBounds[0] == double.MaxValue)
                {
                    //The element has no enabled faces, do not randomize.
                    continue;
                }
                LoadedTexture t = elem.FacesResolved[0].GetLoadedTexture();
                Vector4[] randEntityTextureUVs = new Vector4[] { new Vector4(Random.Range(0, (int)(t.storedWidth - (float)(elemUvBounds[2] - elemUvBounds[0]))), Random.Range(0, (int)(t.storedHeight - (float)(elemUvBounds[3] - elemUvBounds[1])))) };
                elementUIDsToUVCoords.Add(elem.elementUID, (prevEntityTextureUVs, randEntityTextureUVs));
            }
            else
            {
                //Get current UVs
                Vector4[] faceUVs = new Vector4[6];
                Vector4[] randUVs = new Vector4[6];
                for (int i = 0; i < 6; i++)
                {
                    var f = elem.FacesResolved[i];
                    faceUVs[i] = new Vector4(f.Uv[0], f.Uv[1], f.Uv[2], f.Uv[3]);

                    //We can get the width and height from this.
                    Vector4 autoRes = f.GetAutoUVPositions(elem.GetFaceDimension(i), new Vector2(f.Uv[0], f.Uv[1]));

                    LoadedTexture t = f.GetLoadedTexture();
                    int sRandX = Random.Range(0, t.storedWidth - (int)(autoRes[2] - autoRes[0]));
                    int sRandY = Random.Range(0, t.storedHeight - (int)(autoRes[3] - autoRes[1]));
                    randUVs[i] = f.GetAutoUVPositions(elem.GetFaceDimension(i), new Vector2(sRandX, sRandY));
                }
                elementUIDsToUVCoords.Add(elem.elementUID, (faceUVs, randUVs));
            }
        }
    }



    public override void DoTask()
    {
        foreach (KeyValuePair<int, (Vector4[] faceUVsBefore, Vector4[] faceUVsAfter)> pair in elementUIDsToUVCoords)
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(pair.Key);
            if (elem.autoUnwrap && ShapeHolder.CurrentLoadedShape.editor.entityTextureMode)
            {
                elem.entityTextureUV = new double[] { pair.Value.faceUVsAfter[0].x, pair.Value.faceUVsAfter[0].y };
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    elem.FacesResolved[i].Uv = new float[] {
                    pair.Value.faceUVsAfter[i].x,
                    pair.Value.faceUVsAfter[i].y,
                    pair.Value.faceUVsAfter[i].z,
                    pair.Value.faceUVsAfter[i].w };
                }
            }
            elem.ResolveUVForFaces();
            elem.RecreateObjectMesh();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }
    }

    public override void UndoTask()
    {
        foreach (KeyValuePair<int, (Vector4[] faceUVsBefore, Vector4[] faceUVsAfter)> pair in elementUIDsToUVCoords)
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(pair.Key);
            if (elem.autoUnwrap && ShapeHolder.CurrentLoadedShape.editor.entityTextureMode)
            {
                elem.entityTextureUV = new double[] { pair.Value.faceUVsBefore[0].x, pair.Value.faceUVsBefore[0].y };
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    elem.FacesResolved[i].Uv = new float[] {
                    pair.Value.faceUVsBefore[i].x,
                    pair.Value.faceUVsBefore[i].y,
                    pair.Value.faceUVsBefore[i].z,
                    pair.Value.faceUVsBefore[i].w };
                }
            }
            elem.ResolveUVForFaces();
            elem.RecreateObjectMesh();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Texture;
    }

    public override string GetTaskName()
    {
        return "Randomize UVs";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
