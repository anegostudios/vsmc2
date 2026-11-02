using UnityEngine;
using VSMC;
using System.Collections.Generic;

public class TaskSetOutOfBoundsUVsToZero : IEditTask
{
    public Dictionary<int, Vector4[]> elementUIDsToUVCoords;
    public int outOfBoundUvsCount;

    public TaskSetOutOfBoundsUVsToZero()
    {
        elementUIDsToUVCoords = new Dictionary<int, Vector4[]>();
        foreach (ShapeElement elem in ShapeElementRegistry.main.GetAllShapeElements())
        {
            if (elem.autoUnwrap && ShapeHolder.CurrentLoadedShape.editor.entityTextureMode)
            {
                Vector4[] prevEntityTextureUVs = new Vector4[] { new Vector4((float)elem.entityTextureUV[0], (float)elem.entityTextureUV[1]) };
                elementUIDsToUVCoords.Add(elem.elementUID, prevEntityTextureUVs);
            }
            else
            {
                Vector4[] faceUVs = new Vector4[6];
                for (int i = 0; i < 6; i++)
                {
                    var f = elem.FacesResolved[i];
                    faceUVs[i] = new Vector4(f.Uv[0], f.Uv[1], f.Uv[2], f.Uv[3]);
                }
                elementUIDsToUVCoords.Add(elem.elementUID, faceUVs);
            }
        }
    }

    public override void DoTask()
    {
        outOfBoundUvsCount = 0;
        foreach (ShapeElement elem in ShapeElementRegistry.main.GetAllShapeElements())
        {
            if (elem.autoUnwrap && ShapeHolder.CurrentLoadedShape.editor.entityTextureMode)
            {
                //If any faces are out, set the entity tex UV to max.
                for (int i = 0; i < 6; i++)
                {
                    var f = elem.FacesResolved[i];
                    if (!f.Enabled) continue;
                    float mX = f.GetLoadedTexture().storedWidth;
                    float mY = f.GetLoadedTexture().storedHeight;
                    bool isOk = true; //im so lazy I dont wanna do a break;
                    if (f.Uv[0] < 0 || f.Uv[0] >= mX) isOk = false;
                    if (isOk && (f.Uv[1] < 0 || f.Uv[1] >= mY)) isOk = false;
                    if (isOk && (f.Uv[2] < 0 || f.Uv[2] >= mX)) isOk = false;
                    if (isOk && (f.Uv[3] < 0 || f.Uv[3] >= mY)) isOk = false;
                    if (!isOk)
                    {
                        elem.entityTextureUV = new double[] { 0, 0 };
                        outOfBoundUvsCount += 6;
                        break; //Go onto next element as we just set all six faces to zero here.
                    }
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    var f = elem.FacesResolved[i];
                    if (!f.Enabled) continue;
                    float mX = f.GetLoadedTexture().storedWidth;
                    float mY = f.GetLoadedTexture().storedHeight;
                    bool isOk = true; //im so lazy I dont wanna do a break;
                    if (f.Uv[0] < 0 || f.Uv[0] >= mX) isOk = false;
                    if (isOk && (f.Uv[1] < 0 || f.Uv[1] >= mY)) isOk = false;
                    if (isOk && (f.Uv[2] < 0 || f.Uv[2] >= mX)) isOk = false;
                    if (isOk && (f.Uv[3] < 0 || f.Uv[3] >= mY)) isOk = false;
                    if (!isOk)
                    {
                        f.Uv = new float[] { 0, 0, f.Uv[2] - f.Uv[0], f.Uv[3] - f.Uv[1] };
                        if (f.Uv[2] < 0) f.Uv = new float[] { -f.Uv[0], f.Uv[1], 0, f.Uv[3] };
                        if (f.Uv[3] < 0) f.Uv = new float[] { f.Uv[0], -f.Uv[3], f.Uv[2], 0 }; //Edge cases for negative-sized UVs.
                        outOfBoundUvsCount++;
                    }
                }
            }
            elem.ResolveUVForFaces();
            elem.RecreateObjectMesh();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }
    }

    public override void UndoTask()
    {
        foreach (KeyValuePair<int, Vector4[]> pair in elementUIDsToUVCoords)
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(pair.Key);
            if (elem.autoUnwrap && ShapeHolder.CurrentLoadedShape.editor.entityTextureMode)
            {
                elem.entityTextureUV = new double[] { pair.Value[0].x, pair.Value[0].y };
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    elem.FacesResolved[i].Uv = new float[] {
                    pair.Value[i].x,
                    pair.Value[i].y,
                    pair.Value[i].z,
                    pair.Value[i].w };
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
        return "Zero All Out-Of-Bounds UVs";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
