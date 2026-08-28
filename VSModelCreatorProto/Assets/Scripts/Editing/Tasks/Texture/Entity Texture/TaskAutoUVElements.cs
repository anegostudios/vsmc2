using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
namespace VSMC
{
    public class TaskAutoUVElements : IEditTask
    {
        Dictionary<ShapeElement, TextureEditor.ElementTextureDataForTasks> elementData;
        List<ShapeElement> toUnwrap;
        int unwrapMode;
        int preferredWidth;
        int preferredHeight;
        int uvPadding;
        int totalAttempts;
        public bool mostRecentDidSucceed = false;
        public Vector2Int successUVDimensions;

        public TaskAutoUVElements(ShapeElement unwrapParent, int unwrapMode, int prefWidth, int prefHeight, int uvPadding, int attempsWithGrowth)
        {
            this.unwrapMode = unwrapMode;
            this.preferredHeight = prefHeight;
            this.preferredWidth = prefWidth;
            this.uvPadding = uvPadding;
            this.totalAttempts = attempsWithGrowth;

            //Find elements to unwrap.
            toUnwrap = new List<ShapeElement>();
            if (unwrapParent != null)
            {
                List<ShapeElement> es = new List<ShapeElement>() { unwrapParent };
                while (es.Count > 0)
                {
                    ShapeElement e = es[0];
                    es.RemoveAt(0);
                    toUnwrap.Add(e);
                    if (e.Children != null) es.AddRange(e.Children);
                }
            }
            else
            {
                toUnwrap.AddRange(ShapeElementRegistry.main.GetAllShapeElements());
            }

            //Sort toUnwrap by size.
            toUnwrap = toUnwrap.OrderByDescending(x => x.Volume).ToList();

            //Get all the element data for undoing.
            elementData = new Dictionary<ShapeElement, TextureEditor.ElementTextureDataForTasks>();
            foreach (ShapeElement s in toUnwrap)
            {
                elementData.Add(s, new TextureEditor.ElementTextureDataForTasks(s));
            }
        }

        public override void DoTask()
        {
            /* General box packing algorithm:
                - Map the UVs for all elements that are not to be unwrapped.
                - Sort by element size
                - Foreach elem:
                    - Calculate UV bounds.
                    - Find space for UVs. UVs are generally square so no need for complex per-pixel collision detection, just bounding box.
                    - If intersect with any already placed UVs, included in algorithm or not, move the element over.
                    - If there is no space, then increase the UV size by 8 pixels in each direction and restart. 
                    - When UV space is found, then mark its position and add the padding values.
                - 
            */

            List<RectInt> mappedUVBoundingBoxes = new List<RectInt>();
            //This should give all elements that are in the registry but are not being unwrapped.
            foreach (ShapeElement notMapping in ShapeElementRegistry.main.GetAllShapeElements().Where(x => !toUnwrap.Contains(x)))
            {
                for (int i = 0; i < 6; i++)
                {
                    ShapeElementFace f = notMapping.FacesResolved[i];
                    if (f.Enabled)
                    {
                        Vector2Int fs = new Vector2Int((int)(f.Uv[0] - uvPadding), (int)(f.Uv[1] - uvPadding));
                        Vector2Int fe = new Vector2Int(Mathf.CeilToInt(f.Uv[2] + uvPadding), Mathf.CeilToInt(f.Uv[3] + uvPadding));
                        mappedUVBoundingBoxes.Add(new RectInt(fs, fe - fs));
                    }
                }
            }

            int preAnalysedBoundingBoxCount = mappedUVBoundingBoxes.Count;
            int maxUvUnwrapAttempts = totalAttempts;
            int texRangeWidth = preferredWidth;
            int texRangeHeight = preferredHeight;
            for (int uvUnwrapAttempt = 0; uvUnwrapAttempt < maxUvUnwrapAttempts; uvUnwrapAttempt++)
            {
                InfoLogger.main.LogText("Attempting to unwrap all UVs into " + texRangeWidth + "x" + texRangeHeight + ". This is attempt " + (uvUnwrapAttempt + 1));
                bool hasAttemptFailed = false;
                //Reset the bounding boxes to only the non-modified ones
                mappedUVBoundingBoxes.RemoveRange(preAnalysedBoundingBoxCount, mappedUVBoundingBoxes.Count - preAnalysedBoundingBoxCount);
                foreach (ShapeElement e in toUnwrap)
                {
                    int cTX = 0;
                    int cTY = 0;
                    e.entityTextureUnwrapMode = unwrapMode;
                    e.entityTextureUnwrapRotationIndex = 0;
                    bool foundValidSpot = false;
                    while (!foundValidSpot)
                    {
                        e.entityTextureUV = new double[] { cTX, cTY };
                        UVUnwrapper.DoAutoUV(e);
                        bool isFaceValid = true;
                        RectInt[] faceRects = new RectInt[6];
                        for (int f = 0; f < 6; f++)
                        {
                            ShapeElementFace face = e.FacesResolved[f];
                            if (face.Enabled)
                            {
                                Vector2Int fs = new Vector2Int((int)(face.Uv[0]), (int)face.Uv[1]);
                                Vector2Int fe = new Vector2Int(Mathf.CeilToInt(face.Uv[2]), Mathf.CeilToInt(face.Uv[3]));
                                faceRects[f] = new RectInt(fs, fe - fs);
                                //Debug.Log(faceRects[f]);
                                //Check Y bounds first:
                                if (fe.y >= texRangeHeight)
                                {
                                    //If the Y axis is out of bounds then we didn't find anywhere to place the element.
                                    hasAttemptFailed = true;
                                    isFaceValid = false;
                                    break;
                                }
                                //Now check X bounds:
                                if (fe.x >= texRangeWidth) //I think this is >=. If the UV ends at 16 on a 16x16 texture, that's outside the range.
                                {
                                    cTX = 0;
                                    cTY += 1;
                                    isFaceValid = false;
                                    break;
                                }
                                //Now check for intersections
                                if (mappedUVBoundingBoxes.Any(x => x.Overlaps(faceRects[f])))
                                {
                                    cTX += 1;
                                    isFaceValid = false;
                                    break;
                                }
                            }
                            //Yay! This space is valid.
                        }
                        if (!isFaceValid) //We found an invalid face. The values have already been adjusted, so just continue.
                        {
                            if (hasAttemptFailed)
                            {
                                break;
                            }
                            continue;
                        }
                        else
                        {
                            foundValidSpot = true;
                            //Success for this element! Add it to the bounds...
                            for (int f = 0; f < 6; f++)
                            {
                                if (e.FacesResolved[f].Enabled)
                                {
                                    //Add the bounding box, but include the padding around it now.
                                    mappedUVBoundingBoxes.Add(new RectInt(
                                        new Vector2Int(faceRects[f].min.x - uvPadding, faceRects[f].min.y - uvPadding),
                                        new Vector2Int(faceRects[f].width + (uvPadding * 2), faceRects[f].height + (uvPadding * 2))
                                    ));
                                }
                            }
                        }
                    }
                    if (hasAttemptFailed)
                    {
                        texRangeWidth += 8;
                        texRangeHeight += 8; //Give some more space to work with and try again...
                        InfoLogger.main.LogText("Failed to unwrap all UVs...");
                        break;
                    }
                }
                if (!hasAttemptFailed)
                {
                    //Success!
                    InfoLogger.main.LogText("Successfully unwrapped UVs!");
                    mostRecentDidSucceed = true; 
                    foreach (ShapeElement s in toUnwrap)
                    {
                        s.ResolveUVForFaces();
                    }
                    UVLayoutManager.main.RefreshAllUVSpaces(true);
                    successUVDimensions = new Vector2Int(texRangeWidth, texRangeHeight);
                    return;
                }
            }

            //If here, then the process failed. Restore the undo data...
            mostRecentDidSucceed = false;
            foreach (ShapeElement s in toUnwrap)
            {
                elementData[s].ApplyTo(s);
                s.ResolveUVForFaces();
            }
        }

        public override void UndoTask()
        {
            //Restore from the stored element data.
            foreach (ShapeElement s in toUnwrap)
            {
                elementData[s].ApplyTo(s);
                s.ResolveUVForFaces();
            }
            UVLayoutManager.main.RefreshAllUVSpaces(true);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override string GetTaskName()
        {
            return "Auto Unwrap Elements";
        }

    }
}