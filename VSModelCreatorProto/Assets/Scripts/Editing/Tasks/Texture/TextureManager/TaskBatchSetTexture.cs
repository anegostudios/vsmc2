using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace VSMC
{
    /// <summary>
    /// An undoable task to set the texture code (string) of all selected faces.
    /// </summary>
    public class TaskBatchSetTexture : IEditTask
    {

        public Dictionary<int, string[]> elemUIDToOldResolvedTextures;
        public string newTextureCode;

        public TaskBatchSetTexture(string newTexture, ShapeElement onlyDoElement = null, bool andChildren = true)
        {
            elemUIDToOldResolvedTextures = new Dictionary<int, string[]>();
            if (onlyDoElement == null)
            {
                foreach (ShapeElement e in ShapeElementRegistry.main.GetAllShapeElements())
                {
                    string[] tex = new string[6];
                    for (int i = 0; i < 6; i++)
                    {
                        if (e.FacesResolved[i] != null)
                        {
                            tex[i] = e.FacesResolved[i].ResolvedTexture;
                        }
                    }
                    elemUIDToOldResolvedTextures.Add(e.elementUID, tex);
                }
            }
            else //Only children and subchildren of selected
            {
                List<ShapeElement> toScan = new List<ShapeElement> { onlyDoElement };
                while (toScan.Count > 0)
                {
                    ShapeElement se = toScan[0];
                    toScan.RemoveAt(0);
                    if (se.Children != null && andChildren)
                    {
                        toScan.AddRange(se.Children);
                    }

                    string[] tex = new string[6];
                    for (int i = 0; i < 6; i++)
                    {
                        if (se.FacesResolved[i] != null)
                        {
                            tex[i] = se.FacesResolved[i].ResolvedTexture;
                        }
                    }
                    elemUIDToOldResolvedTextures.Add(se.elementUID, tex);
                }
            }
            this.newTextureCode = newTexture;
        }

        public override void DoTask()
        {
            foreach (KeyValuePair<int, string[]> pair in elemUIDToOldResolvedTextures)
            {
                ShapeElement e = ShapeElementRegistry.main.GetShapeElementByUID(pair.Key);

                for (int i = 0; i < 6; i++)
                {
                    if (e.FacesResolved[i] != null)
                    {
                        e.FacesResolved[i].Texture = "#" + newTextureCode;
                        e.FacesResolved[i].ResolveTexture(TextureManager.main.loadedTextures);
                    }
                }
                e.RecreateObjectMesh();
            }
            UVLayoutManager.main.RefreshAllUVSpaces(true);
        }

        public override void UndoTask()
        {
            foreach (KeyValuePair<int, string[]> pair in elemUIDToOldResolvedTextures)
            {
                ShapeElement e = ShapeElementRegistry.main.GetShapeElementByUID(pair.Key);

                for (int i = 0; i < 6; i++)
                {
                    if (e.FacesResolved[i] != null)
                    {
                        e.FacesResolved[i].Texture = "#" + pair.Value[i];
                        e.FacesResolved[i].ResolveTexture(TextureManager.main.loadedTextures);
                    }
                }
                e.RecreateObjectMesh();
            }
            UVLayoutManager.main.RefreshAllUVSpaces(true);
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 0;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Texture for group of elements";
        }
    }
}