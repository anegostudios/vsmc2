using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// An undoable task for setting an element position in a single axis.
    /// Used by the UI controls.
    /// </summary>
    public class TaskSetElementPosition : IEditTask
    {
        public int elementUID;
        public EnumAxis axis;
        public double oldPosition;
        public double newPosition;

        public TaskSetElementPosition(ShapeElement elem, EnumAxis axis, double newPos)
        {
            elementUID = elem.elementUID;
            this.axis = axis;
            oldPosition = elem.From[(int)axis];
            newPosition = newPos;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            //Need to edit both the from and to values, so we need a temp storage of the size.
            double size = elem.To[(int)axis] - elem.From[(int)axis];
            elem.From[(int)axis] = newPosition;
            elem.To[(int)axis] = newPosition + size;
            elem.RecreateTransforms();

            /* Old code - This has been moved to IncElemenetPosition.
            //Move pos & rot origin in local space. - 100% works.
            if (alsoMoveRotationOrigin && !moveInGlobalSpace)
            {
                double inc = newPosition - oldPosition;
                Vector3 movement = new Vector3();
                movement[(int)axis] = (float)inc;
                movement = elem.RotateFromWorldToLocalForThisObjectsRotation(movement);
                movement = movement.normalized * (float)inc;

                elem.From = new double[] { elem.From[0] + movement.x, elem.From[1] + movement.y, elem.From[2] + movement.z };
                elem.To = new double[] { elem.To[0] + movement.x, elem.To[1] + movement.y, elem.To[2] + movement.z };
                elem.RotationOrigin = new double[] { elem.RotationOrigin[0] + movement.x, elem.RotationOrigin[1] + movement.y, elem.RotationOrigin[2] + movement.z };
            } 

            //Move only pos in global space
            else if (!alsoMoveRotationOrigin && moveInGlobalSpace)
            {
                double inc = newPosition - oldPosition;
                Vector3 movement = new Vector3();
                movement[(int)axis] = (float)inc;
                movement = elem.RotateFromLocalToWorldForThisObjectsRotation(movement);

                elem.From = new double[] { elem.From[0] + movement.x, elem.From[1] + movement.y, elem.From[2] + movement.z };
                elem.To = new double[] { elem.To[0] + movement.x, elem.To[1] + movement.y, elem.To[2] + movement.z };
            }

            //Move only pos in local space.
            else if (!alsoMoveRotationOrigin && !moveInGlobalSpace)
            {
                double inc = newPosition - oldPosition;
                Vector3 movement = new Vector3();
                movement[(int)axis] = (float)inc;

                elem.From = new double[] { elem.From[0] + movement.x, elem.From[1] + movement.y, elem.From[2] + movement.z };
                elem.To = new double[] { elem.To[0] + movement.x, elem.To[1] + movement.y, elem.To[2] + movement.z };
            }
            */
        }

        public override void UndoTask()
        {
            //Same as Do but just reversed.
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            //Need to edit both the from and to values, so we need a temp storage of the size.
            double size = elem.To[(int)axis] - elem.From[(int)axis];            
            elem.From[(int)axis] = oldPosition;
            elem.To[(int)axis] = oldPosition + size;
            elem.RecreateTransforms();

            /* Old code - This has been moved to IncElemenetPosition.
            if (alsoMoveRotationOrigin && moveInGlobalSpace)
            {
                double inc = newPosition - oldPosition;
                Vector3 movement = new Vector3();
                movement[(int)axis] = (float)inc;

                //Reverse the movement vector for undo.
                movement = elem.RotateFromWorldToLocalForThisObjectsRotation(movement) * -1;

                elem.From = new double[] { elem.From[0] + movement.x, elem.From[1] + movement.y, elem.From[2] + movement.z };
                elem.To = new double[] { elem.To[0] + movement.x, elem.To[1] + movement.y, elem.To[2] + movement.z };
                elem.RotationOrigin = new double[] { elem.RotationOrigin[0] + movement.x, elem.RotationOrigin[1] + movement.y, elem.RotationOrigin[2] + movement.z };
            }

            if (alsoMoveRotationOrigin && !moveInGlobalSpace)
            {
                double inc = newPosition - oldPosition;
                Vector3 movement = new Vector3();
                movement[(int)axis] = (float)-inc;

                elem.From = new double[] { elem.From[0] + movement.x, elem.From[1] + movement.y, elem.From[2] + movement.z };
                elem.To = new double[] { elem.To[0] + movement.x, elem.To[1] + movement.y, elem.To[2] + movement.z };
                elem.RotationOrigin = new double[] { elem.RotationOrigin[0] + movement.x, elem.RotationOrigin[1] + movement.y, elem.RotationOrigin[2] + movement.z };
            }
            */

        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
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
            return "Set Element Position";
        }
    }
}