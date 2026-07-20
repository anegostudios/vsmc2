using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using VSMC;

namespace VSMC
{
    /// <summary>
    /// An undoable task that changes the shape elements parent to another.
    /// </summary>
    public class TaskReparentElement : IEditTask
    {

        public int elemToReparentID;
        public int newParentID;
        public int oldParentID;

        public Vector3 oldFrom;
        public Vector3 oldRotOrigin;
        public Vector3 oldRotation;

        public Vector3 worldFrom;
        public Vector3 worldRotationOrigin;
        public Vector3 worldRotation;
        public double[] size;

        /// <summary>
        /// Reparents an element. CAUTION - This does NOT include a failsafe for reparenting an object to itself or its children.
        /// </summary>
        public TaskReparentElement(int elemToReparentID, int newParentID, bool keepGlobalTransform = true)
        {
            this.elemToReparentID = elemToReparentID;
            this.newParentID = newParentID;

            ShapeElement child = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);
            //worldFrom = child.GetWorldFrom();
            //worldRotationOrigin = child.GetWorldRotationOrigin();
            //worldRotation = child.GetWorldRotation();

            //Debug.Log("World from: " + worldFrom);
            //Debug.Log("World Rot Orig: " + worldRotationOrigin);
            //Debug.Log("World Rotation: " + worldRotation);

            size = new double[] { (child.To[0] - child.From[0]),
            (child.To[1] - child.From[1]),
            (child.To[2] - child.From[2]) };

            if (child.ParentElement == null)
            {
                oldParentID = -1;
            }
            else
            {
                oldParentID = child.ParentElement.elementUID;
            }

            oldFrom = new Vector3((float)child.From[0], (float)child.From[1], (float)child.From[2]);
            oldRotOrigin = new Vector3((float)child.RotationOrigin[0], (float)child.RotationOrigin[1], (float)child.RotationOrigin[2]);
            oldRotation = new Vector3((float)child.RotationX, (float)child.RotationY, (float)child.RotationZ);
        }

        public override void DoTask()
        {
            ShapeElement child = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);

            Matrix4x4 matrix2 = Matrix4x4.identity;
            matrix2 = child.ApplyTransform(matrix2);

            Debug.Log(matrix2);
            Debug.Log(ExtractEulerXYZ(matrix2));

            List<ShapeElement> oldParentPath = child.GetParentPath();
            
            //Remove old parent first.
            if (oldParentID == -1)
            {
                ShapeHolder.CurrentLoadedShape.RemoveRootShapeElement(child);
            }
            else
            {
                ShapeElement oldParent = ShapeElementRegistry.main.GetShapeElementByUID(oldParentID);
                oldParent.Children = oldParent.Children.Remove(child);
                oldParent.RecreateObjectMeshAndTransforms();
            }

            //Now add new parent.
            if (newParentID == -1)
            {
                ShapeHolder.CurrentLoadedShape.AddRootShapeElement(child);
                child.ParentElement = null;
            }
            else
            {
                ShapeElement newParent = ShapeElementRegistry.main.GetShapeElementByUID(newParentID);
                child.ParentElement = newParent;
                if (newParent.Children == null) newParent.Children = new ShapeElement[] { child };
                else
                {
                    newParent.Children = newParent.Children.Append(child);
                }
            }

            List<ShapeElement> newParentPath = child.GetParentPath();

            Matrix4x4 matrix = Matrix4x4.identity;

            foreach (ShapeElement e in newParentPath)
            {
                //Debug.Log("Analysing element " + e.Name + "in NEW parent path.");
                matrix = e.ApplyTransform(matrix);
            }
            matrix = matrix.inverse;
            Debug.Log("Got angle of new parent path of "+ ExtractEulerXYZ(matrix));
            foreach (ShapeElement e in oldParentPath)
            {
                //Debug.Log("Analysing element " + e.Name + "in OLD parent path.");
                matrix = e.ApplyTransform(matrix);
            }

            Vector3 originPos = matrix * new Vector4((float)child.RotationOrigin[0], (float)child.RotationOrigin[1], (float)child.RotationOrigin[2], 1);
            matrix = child.ApplyTransform(matrix);
            Vector3 angles = ExtractEulerXYZ(matrix);
            Debug.Log("Got full rotation of " + angles);

            child.RotationOrigin = new double[] { originPos.x, originPos.y, originPos.z };
            child.RotationX = -angles.x;
            child.RotationY = -angles.y;
            child.RotationZ = -angles.z;

            matrix = matrix.inverse;
            matrix = child.ApplyTransform(matrix);
            matrix = matrix.inverse;

            Vector3 startPos = matrix * new Vector4((float)child.From[0], (float)child.From[1], (float)child.From[2], 1);
            child.From = new double[] { startPos.x, startPos.y, startPos.z };
            child.To = new double[] { startPos.x + size[0], startPos.y + size[1], startPos.z + size[2] };

            child.RecreateObjectMeshAndTransforms();
            ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);
        }

        public override void UndoTask()
        {
            ShapeElement child = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);
            //ShapeElement newParent = ShapeElementRegistry.main.GetShapeElementByUID(newParentID);

            //Restore the transform
            child.From = new double[] { oldFrom[0], oldFrom[1], oldFrom[2] };
            child.RotationOrigin = new double[] { oldRotOrigin[0], oldRotOrigin[1], oldRotOrigin[2] };
            child.RotationX = oldRotation[0];
            child.RotationY = oldRotation[1];
            child.RotationZ = oldRotation[2];
            child.To = new double[] { child.From[0] + size[0], child.From[1] + size[1], child.From[2] + size[2] };

            //Do the opposite - So remove the new parent first.
            if (newParentID == -1)
            {
                ShapeHolder.CurrentLoadedShape.RemoveRootShapeElement(child);
            }
            else
            {
                ShapeElement newParent = ShapeElementRegistry.main.GetShapeElementByUID(newParentID);
                newParent.Children = newParent.Children.Remove(child);
                newParent.RecreateObjectMeshAndTransforms();
            }

            //Now add the old parent back.
            if (oldParentID == -1)
            {
                ShapeHolder.CurrentLoadedShape.AddRootShapeElement(child);
                child.ParentElement = null;
                child.RecreateObjectMeshAndTransforms();
            }
            else
            {
                ShapeElement oldParent = ShapeElementRegistry.main.GetShapeElementByUID(oldParentID);

                child.ParentElement = oldParent;
                if (oldParent.Children == null) oldParent.Children = new ShapeElement[] { child };
                else
                {
                    oldParent.Children = oldParent.Children.Append(child);
                }
                oldParent.RecreateObjectMeshAndTransforms();
            }
            ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);

        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) * 3 + sizeof(bool);
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override string GetTaskName()
        {
            return "Reparent Element";
        }

        Vector3 ExtractEulerXYZ(Matrix4x4 m)
        {
            Vector3 rot = new Vector3();
            Matrix4x4 n = m.transpose;
            Quaternion q = Quaternion.LookRotation(n.GetColumn(2), n.GetColumn(1));

            //Roll (x-axis)
            float sinr_cosp = 2.0f * (q.w * q.x + q.y * q.z);
            float cosr_cosp = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);
            rot.x = Mathf.Atan2(sinr_cosp, cosr_cosp);


            // pitch (y-axis)
            float sinp = 2.0f * (q.w * q.y - q.z * q.x);
            if (Mathf.Abs(sinp) >= 1)
            {
                rot.y = Mathf.PI / 2 * math.sign(sinp);
            }
            else
            {
                rot.y = Mathf.Asin(sinp);
            }

            // yaw (z-axis)
            float siny_cosp = 2.0f * (q.w * q.z + q.x * q.y);
            float cosy_cosp = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);
            rot.z = Mathf.Atan2(siny_cosp, cosy_cosp);

            return rot * Mathf.Rad2Deg;

        }

    }
}