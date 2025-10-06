using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// An edit task for setting an element position in a single axis.
    /// Can be merged with other tasks of the same type.
    /// </summary>
    public class TaskSetElementPosition : IEditTask
    {
        public int elementUID;
        public EnumAxis axis;
        public double oldPosition;
        public double newPosition;
        public int randomTransformationUID;

        public bool alsoMoveRotationOrigin;
        public bool moveInGlobalSpace;

        public TaskSetElementPosition(ShapeElement elem, EnumAxis axis, double newPos, int ranTranUID = 0, bool alsoMoveRotationOrigin = false, bool moveInGlobalSpace = false)
        {
            elementUID = elem.elementUID;
            this.axis = axis;
            oldPosition = elem.From[(int)axis];
            newPosition = newPos;
            randomTransformationUID = ranTranUID;
            this.alsoMoveRotationOrigin = alsoMoveRotationOrigin;
            this.moveInGlobalSpace = moveInGlobalSpace;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            //Need to edit both the from and to values, so we need a temp storage of the size.
            //double size = elem.To[(int)axis] - elem.From[(int)axis];
            //elem.From[(int)axis] = newPosition;
            //elem.To[(int)axis] = newPosition + size;

            double inc = newPosition - oldPosition;
            Vector3 movement = new Vector3();
            movement[(int)axis] = (float)inc;
            movement = elem.RotateFromWorldToLocalForThisObjectsRotation(movement);
            //movement = Quaternion.Inverse(elem.gameObject.transform.rotation) * movement;

            elem.From = new double[] { elem.From[0] + movement.x, elem.From[1] + movement.y, elem.From[2] + movement.z };
            elem.To = new double[] { elem.To[0] + movement.x, elem.To[1] + movement.y, elem.To[2] + movement.z };
            elem.RotationOrigin = new double[] { elem.RotationOrigin[0] + movement.x, elem.RotationOrigin[1] + movement.y, elem.RotationOrigin[2] + movement.z };

            elem.RecreateTransforms();
        }

        public override void UndoTask()
        {
            //Same as Do but just reversed.
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);

            //Just do the reverse increment.
            double inc = oldPosition - newPosition;
            Vector3 movement = new Vector3();
            movement[(int)axis] = (float)inc;
            movement = elem.RotateFromWorldToLocalForThisObjectsRotation(movement);
            //movement = Quaternion.Inverse(elem.gameObject.transform.rotation) * movement;

            elem.From = new double[] { elem.From[0] + movement.x, elem.From[1] + movement.y, elem.From[2] + movement.z };
            elem.To = new double[] { elem.To[0] + movement.x, elem.To[1] + movement.y, elem.To[2] + movement.z };
            elem.RotationOrigin = new double[] { elem.RotationOrigin[0] + movement.x, elem.RotationOrigin[1] + movement.y, elem.RotationOrigin[2] + movement.z };

            elem.RecreateTransforms();
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            //Keep the first tasks old position, and the newTasks new position.
            if (nextTask is TaskSetElementPosition t2)
            {
                if (elementUID == t2.elementUID && axis == t2.axis && randomTransformationUID == t2.randomTransformationUID)
                {
                    newPosition = t2.newPosition;
                    return true;
                }
            }
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return
                sizeof(int) * 2 +
                sizeof(EnumAxis) +
                sizeof(double) * 2;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Move Element";
        }
    }
}