using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// Deletes a shape element.
    /// </summary>
    public class TaskDeleteElement : IEditTask
    {
        /// <summary>
        /// Best way of storing the location and data for an animation entry.
        /// </summary>
        public struct AnimationEntryAndLocation
        {
            public string elemName;
            public AnimationKeyFrameElement keyFrameElement;
            public int animIndex;
            public int keyFrameIndex;
        }

        //This needs to store a fair bit of data to enable undoing.
        public ShapeElement elemToDelete;
        public List<AnimationEntryAndLocation> animationEntries;

        public TaskDeleteElement(ShapeElement elem)
        {
            elemToDelete = elem;
            List<string> elemNames = elem.GetNamesOfThisAndAllChildren();

            //Need to calculate the animation entries that this deletion removes.
            animationEntries = CalculateAnimationEntries(elemNames);
        }

        List<AnimationEntryAndLocation> CalculateAnimationEntries(List<string> elemNames)
        {
            List<AnimationEntryAndLocation> animationEntries = new List<AnimationEntryAndLocation>();
            if (ShapeLoader.main.shapeHolder.cLoadedShape.Animations != null)
            {
                for (int i = 0; i < ShapeLoader.main.shapeHolder.cLoadedShape.Animations.Length; i++)
                {
                    Animation anim = ShapeLoader.main.shapeHolder.cLoadedShape.Animations[i];
                    for (int j = 0; j < anim.KeyFrames.Length; j++)
                    {
                        foreach (string s in elemNames)
                        {
                            if (anim.KeyFrames[j].Elements.ContainsKey(s))
                            {
                                animationEntries.Add(new AnimationEntryAndLocation()
                                {
                                    elemName = s,
                                    animIndex = i,
                                    keyFrameIndex = j,
                                    keyFrameElement = anim.KeyFrames[j].Elements[s]
                                });
                            }
                        }
                    }
                }
            }
            return animationEntries;
        }


        public override void DoTask()
        {
            ObjectSelector.main.DeselectAll();
            List<ShapeElement> elemToDeleteAndChildren = elemToDelete.GetThisAndAllChildrenRecursively();
            foreach (ShapeElement elem in elemToDeleteAndChildren)
            {
                //'Deleted' elements' gameobjects get moved to a hidden parent object which they can then be restored from.
                ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(elem);

                //Unregister the shape element so it won't be counted in anything.
                ShapeElementRegistry.main.UnregisterShapeElement(elem);
                
                //Delete the element hierarchy for each element.
                GameObject.Destroy(ElementHierarchyManager.ElementHierarchy.GetElementHierarchyItem(elem).gameObject);
            }

            //Remove the animation entries.
            foreach (AnimationEntryAndLocation animEntry in animationEntries)
            {
                ShapeLoader.main.shapeHolder.cLoadedShape
                    .Animations[animEntry.animIndex]
                    .KeyFrames[animEntry.keyFrameIndex]
                    .Elements.Remove(animEntry.elemName);
            }

            //Remove the child from its parent.
            //Note that we do not use the 'RemoveParent' function since that'll lose the reference to the existing parent. 
            if (elemToDelete.ParentElement != null)
            {
                elemToDelete.ParentElement.Children = elemToDelete.ParentElement.Children.Remove(elemToDelete);
            }
            else
            {
                ShapeLoader.main.shapeHolder.cLoadedShape.RemoveRootShapeElement(elemToDelete);
            }
        }

        public override void UndoTask()
        {
            ObjectSelector.main.DeselectAll();

            //Re-Add element as child.
            //The position in the child array doesn't matter, so we are free to add it to the end.
            if (elemToDelete.ParentElement != null)
            {
                elemToDelete.ParentElement.Children = elemToDelete.ParentElement.Children.Append(elemToDelete);
            }
            else
            {
                ShapeLoader.main.shapeHolder.cLoadedShape.AddRootShapeElement(elemToDelete);
            }


            //Re-Add the animation entries.
            foreach (AnimationEntryAndLocation animEntry in animationEntries)
            {
                ShapeLoader.main.shapeHolder.cLoadedShape
                    .Animations[animEntry.animIndex]
                    .KeyFrames[animEntry.keyFrameIndex]
                    .Elements.Add(animEntry.elemName, animEntry.keyFrameElement);
            }

            List<ShapeElement> elemToDeleteAndChildren = elemToDelete.GetThisAndAllChildrenRecursively();
            foreach (ShapeElement elem in elemToDeleteAndChildren)
            {
                //Move the element game objects out of limbo.
                // We will force edit mode for this task, so they are placed in the no joint parent.
                elem.gameObject.transform.SetParent(ShapeLoader.main.shapeHolder.noJointParent, true);

                // And reregister the element.
                ShapeElementRegistry.main.ReregisterShapeElement(elem);
            }

            //For now, recreate the whole element hierarchy.
            // This may need to be changed if the hierarchy list needs to retain a consistent order.
            ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeLoader.main.shapeHolder.cLoadedShape);
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            //Deleting an element should never be merged.
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            //Inefficient but this is an uncommon and difficult task to compute.
            //Marshalling doesn't work, since these are not structs.
            long tot = 0;
            foreach (AnimationEntryAndLocation animEntry in animationEntries)
            {
                tot += System.Text.ASCIIEncoding.ASCII.GetByteCount(animEntry.elemName);
                tot += sizeof(int) * 2;
                tot += 8; //Pointer to keyframe elem.
                tot += 8; //Pointer to this entry in the list, I think?
            }
            tot += 8; //Pointer to element.
            return tot;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            //We very much need model mode for delete operations.
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Delete Element";
        }
    }
}