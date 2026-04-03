using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace VSMC
{
    /// <summary>
    /// Sets the entity texture mode for the current shape.
    /// </summary>
    public class TaskSetEntityTextureMode : IEditTask
    {
        bool newEntityTexMode;
        bool oldEntityTexMode;

        /// <summary>
        /// If true, set auto unwrap on all elements to true.
        /// If false, set auto unwrap on all elements to false.
        /// If null, do not change auto unwrap.
        /// </summary>
        bool? forceSetAutoUnwrapValues;

        //For undoing... No need to use or set if newEntityTexMode is false.
        Dictionary<int, (Vector4[] uvs, int[] rotIndices, bool autoUnwrap)> oldElementUIDToFaceUVsRotationsAndAutoUnwrap; 
        

        public TaskSetEntityTextureMode(bool entityTextureMode, bool? forceSetAutoUnwrapValues)
        {
            newEntityTexMode = entityTextureMode;
            oldEntityTexMode = ShapeHolder.CurrentLoadedShape.editor.entityTextureMode;
            this.forceSetAutoUnwrapValues = forceSetAutoUnwrapValues;

            //Set old values...
            if (newEntityTexMode)
            {
                oldElementUIDToFaceUVsRotationsAndAutoUnwrap = new Dictionary<int, (Vector4[] uvs, int[] rotIndices, bool autoUnwrap)>();
                foreach (ShapeElement elem in ShapeElementRegistry.main.GetAllShapeElements())
                {
                    Vector4[] uvs = new Vector4[6];
                    int[] rotIndices = new int[6];
                    for (int i = 0; i < 6; i++)
                    {
                        ShapeElementFace f = elem.FacesResolved[i];
                        uvs[i] = new Vector4(f.Uv[0], f.Uv[1], f.Uv[2], f.Uv[3]);
                        rotIndices[i] = f.RotationIndex;
                    }
                    Debug.Log("Name: " + elem.Name +" with ID "+elem.elementUID);
                    oldElementUIDToFaceUVsRotationsAndAutoUnwrap.Add(elem.elementUID, (uvs, rotIndices, elem.autoUnwrap));
                }
            }
        }

        public override void DoTask()
        {
            ShapeHolder.CurrentLoadedShape.editor.entityTextureMode = newEntityTexMode;

            if (newEntityTexMode)
            {
                foreach (ShapeElement elem in ShapeElementRegistry.main.GetAllShapeElements())
                {
                    if (forceSetAutoUnwrapValues == true)
                    {
                        elem.autoUnwrap = true;
                    }
                    else if (forceSetAutoUnwrapValues == false)
                    {
                        elem.autoUnwrap = false;
                    }
                    else //null
                    {

                    }
                    elem.ResolveUVForFaces();
                }
            }
            TextureEditor.main.OnEntityTextureModeChange();
        }

        public override void UndoTask()
        {
            ShapeHolder.CurrentLoadedShape.editor.entityTextureMode = oldEntityTexMode;

            //Revert old values if the old value was false.
            if (newEntityTexMode)
            {
                foreach (ShapeElement elem in ShapeElementRegistry.main.GetAllShapeElements())
                {
                    (Vector4[] uvs, int[] rotIndices, bool autoUnwrap) ev = oldElementUIDToFaceUVsRotationsAndAutoUnwrap[elem.elementUID];
                    for (int i = 0; i < 6; i++)
                    {
                        ShapeElementFace f = elem.FacesResolved[i];
                        f.Uv = new float[] { ev.uvs[i].x, ev.uvs[i].y, ev.uvs[i].z, ev.uvs[i].w };
                        f.RotationIndex = ev.rotIndices[i];
                    }
                    elem.autoUnwrap = ev.autoUnwrap;
                }
            }

            TextureEditor.main.OnEntityTextureModeChange();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.None;
        }

        public override string GetTaskName()
        {
            return "Set Entity Texture Mode";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(bool) * 2 + 2048;
        }

    }
}