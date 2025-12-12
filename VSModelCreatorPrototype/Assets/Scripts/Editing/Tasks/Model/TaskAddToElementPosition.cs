using UnityEngine;
namespace VSMC
{
    /// <summary>
    /// An undoable task to translates an element by a movement vector.
    /// This is mainly used by the 3D movement gizmos.
    /// </summary>
    public class TaskAddToElementPosition : IEditTask
    {
        public int elementUID;
        public double[] oldFromPosition;
        public double[] oldToPosition;
        public double[] oldRotOrigin;
        public double[] movement;
        public int randomTransformationUID;

        public bool alsoMoveRotationOrigin;

        public TaskAddToElementPosition(ShapeElement elem, double[] sFrom, double[] sTo, double[] sRotOrigin, double[] movement, int ranTranUID = 0, bool alsoMoveRotationOrigin = false)
        {
            elementUID = elem.elementUID;
            oldFromPosition = new double[] { sFrom[0], sFrom[1], sFrom[2] };
            oldToPosition = new double[] { sTo[0], sTo[1], sTo[2] };
            oldRotOrigin = new double[] { sRotOrigin[0], sRotOrigin[1], sRotOrigin[2] };
            this.movement = new double[] { movement[0], movement[1], movement[2] };
            randomTransformationUID = ranTranUID;
            this.alsoMoveRotationOrigin = alsoMoveRotationOrigin;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            //Need to edit both the from and to values, so we need a temp storage of the size.
            //double size = elem.To[(int)axis] - elem.From[(int)axis];
            //elem.From[(int)axis] = newPosition;
            //elem.To[(int)axis] = newPosition + size;

            
            elem.From = new double[] { oldFromPosition[0] + movement[0], oldFromPosition[1] + movement[1], oldFromPosition[2] + movement[2] };
            elem.To = new double[] { oldToPosition[0] + movement[0], oldToPosition[1] + movement[1], oldToPosition[2] + movement[2] };
            if (alsoMoveRotationOrigin)
            {
                elem.RotationOrigin = new double[] { oldRotOrigin[0] + movement[0], oldRotOrigin[1] + movement[1], oldRotOrigin[2] + movement[2] };
            }
            elem.RecreateTransforms();
        }

        public override void UndoTask()
        {
            //Same as Do but just reversed.
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            //Need to edit both the from and to values, so we need a temp storage of the size.
            //double size = elem.To[(int)axis] - elem.From[(int)axis];            
            //elem.From[(int)axis] = oldPosition;
            //elem.To[(int)axis] = oldPosition + size;

            elem.From = oldFromPosition;
            elem.To = oldToPosition;
            elem.RotationOrigin = oldRotOrigin;
            elem.RecreateTransforms();
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            //Keep the first tasks old position, and the newTasks new position.
            if (nextTask is TaskAddToElementPosition t2)
            {
                if (elementUID == t2.elementUID && randomTransformationUID == t2.randomTransformationUID)
                {
                    movement = t2.movement;
                    return true;
                }
            }
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return
                sizeof(int) * 2 +
                sizeof(double) * 12 +
                sizeof(int);
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