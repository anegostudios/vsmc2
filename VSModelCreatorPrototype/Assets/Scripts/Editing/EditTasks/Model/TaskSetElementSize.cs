using Unity.VisualScripting;
namespace VSMC {

    /// <summary>
    /// An edit task that will set the size of one axis of a shape element.
    /// Can be merged with other tasks of the same type.
    /// </summary>
    public class TaskSetElementSize : IEditTask
    {
        public int elementUID;
        public EnumAxis axis;
        public double oldSize;
        public double newSize;

        /// <summary>
        /// Creates a new resize task for a ShapeElement.
        /// </summary>
        public TaskSetElementSize(ShapeElement elem, EnumAxis axis, double newSize)
        {
            this.elementUID = elem.elementUID;
            this.axis = axis;
            this.newSize = newSize;
            this.oldSize = elem.To[(int)axis] - elem.From[(int)axis];
        }

        public override void DoTask()
        {
            //Get element, update size, recreate mesh.
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            elem.To[(int)axis] = elem.From[(int)axis] + newSize;
            elem.RecreateObjectMeshAndTransforms();
        }

        public override void UndoTask()
        {
            //Get element, update size, recreate mesh.
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
            elem.To[(int)axis] = elem.From[(int)axis] + oldSize;
            elem.RecreateObjectMeshAndTransforms();
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            //Keep the original oldSize, but the next task's newSize.
            if (nextTask is TaskSetElementSize t2)
            {
                if (elementUID == t2.elementUID &&  axis == t2.axis)
                {
                    newSize = t2.newSize;
                    return true;    
                }
            }
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 
                sizeof(int) +
                sizeof(double) * 2 +
                sizeof(EnumAxis);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }
    }
}
