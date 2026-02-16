using System.Text;

namespace VSMC
{
    /// <summary>
    /// An undoable task that renames an element.
    /// </summary>
    public class TaskRenameElement : IEditTask
    {
        public int elemUID;
        public string newName;
        public string oldName;

        /// <summary>
        /// Note that 'targetName' may end up changing. Check 'newName' variable for the actual new name after the task is complete.
        /// </summary>
        public TaskRenameElement(ShapeElement elem, string targetName)
        {
            elemUID = elem.elementUID;
            this.newName = targetName;
            this.oldName = elem.Name;

            ValidateNewName();
        }

        /// <summary>
        /// Validates the new name based on the length and uniqueness.
        /// </summary>
        private void ValidateNewName()
        {
            if (newName == oldName) return;
            while (ShapeElementRegistry.main.GetShapeElementByName(newName) != null)
            {
                //Append _1 to the name, followed by _2, etc etc.
                int underscoreIndex = newName.LastIndexOf('_');
                if (underscoreIndex < 0 || underscoreIndex == newName.Length - 1) //If no underscore, or nothing after the underscore...
                {
                    newName += "_1";
                }
                else
                {
                    int val;
                    if (int.TryParse(newName.Substring(underscoreIndex + 1), out val))
                    {
                        newName = newName.Substring(0, underscoreIndex) + "_" + (val + 1);
                    }
                    else
                    {
                        newName += "_1";
                    }
                }
            }
            if (newName.Length < 1) newName = oldName;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.Name = newName;
            elem.gameObject.name = newName;

            //Rename element in UI.
            ElementHierarchyManager.ElementHierarchy.GetElementHierarchyItem(elem).elementName.text = newName;

            //Animations rely on object names, so we need to rename each entry that has this name.
            foreach (Animation anim in ShapeHolder.CurrentLoadedShape.Animations)
            {
                foreach (AnimationKeyFrame keyFrame in anim.KeyFrames)
                {
                    if (keyFrame.Elements.ContainsKey(oldName))
                    {
                        keyFrame.Elements[newName] = keyFrame.Elements[oldName];
                        keyFrame.Elements.Remove(oldName);
                    }
                }
            }
        }

        public override void UndoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.Name = oldName;
            elem.gameObject.name = oldName;

            //Rename element in UI.
            ElementHierarchyManager.ElementHierarchy.GetElementHierarchyItem(elem).elementName.text = oldName;

            //Animations rely on object names, so we need to rename each entry that has this name.
            foreach (Animation anim in ShapeHolder.CurrentLoadedShape.Animations)
            {
                foreach (AnimationKeyFrame keyFrame in anim.KeyFrames)
                {
                    if (keyFrame.Elements.ContainsKey(newName))
                    {
                        keyFrame.Elements[oldName] = keyFrame.Elements[newName];
                        keyFrame.Elements.Remove(newName);
                    }
                }
            }
        } 

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            //Debateable. I think not.
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return ASCIIEncoding.ASCII.GetByteCount(newName) +
                ASCIIEncoding.ASCII.GetByteCount(oldName) +
                sizeof(int);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            //Definitely requires model mode for this one.
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Rename Element";
        }

    }
}
