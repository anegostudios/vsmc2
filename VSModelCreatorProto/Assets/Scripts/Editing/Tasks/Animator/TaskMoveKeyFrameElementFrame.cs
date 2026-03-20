using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// Sets the position values to their unmodified values for a key frame element, effectively adding a position keyframe.
    /// </summary>
    public class TaskMoveMultiKeyFrameElementsFrames : IEditTask
    {

        /*
        Bit of a rant. This task is hard.
        We need to move the individual values of a keyframe element, so we cannot store the KFEs directly.
        We also need to store all edited or modified KFEs before the action takes place.
        Actually moving is fine, but to avoid overwriting issues we would either need to make another KFE list (which is a nightare), or 
            start it from the side that is being moved.
        I.e., if all keys are being moved to the right, we want to start from the right and work our way backwards.
        If all keys are being moved to the left, we want to start from the left and move to the right.

        Oh and to add on to that, the quantity of frames will need to change too.
        */

        Animation anim;
        string animKeyframesSerializedBeforeMove;
        string animKeyframesSerializedAfterMove;
        int newQuantity = 0;
        int oldQuantity = 0;


        [System.Serializable]
        class AnimationKeyFrameHolder
        {
            public AnimationKeyFrame[] kfs;
        }

        public TaskMoveMultiKeyFrameElementsFrames(Animation anim, List<TimelineKeyFrameElementMarker> kfeMarkers, int moveFramesAmount)
        {

            //This task is setup in a very different way. Rather than performing the task in DoTask, we're going to
            //  do the task in the constructor: When the task is done or redone, it simply sets the keyframe to a new
            //  calculated and serialized state. When undone, it restores the state beforehand.
            this.anim = anim;

            JsonSerializerSettings settings = JsonSettings;
            animKeyframesSerializedBeforeMove = JsonConvert.SerializeObject(new AnimationKeyFrameHolder() { kfs = anim.KeyFrames }, settings);
            IOrderedEnumerable<TimelineKeyFrameElementMarker> markers;
            if (moveFramesAmount > 0) markers = kfeMarkers.OrderByDescending(x => x.assosciatedKFE.Frame);
            else markers = kfeMarkers.OrderBy(x => x.assosciatedKFE.Frame);

            oldQuantity = anim.QuantityFrames;
            newQuantity = anim.QuantityFrames;
            foreach (TimelineKeyFrameElementMarker marker in markers)
            {
                MoveOneKeyFrame(anim, marker.assosciatedKFE, marker.assosciatedFlag, moveFramesAmount);
                newQuantity = Mathf.Max(newQuantity, marker.assosciatedKFE.Frame + moveFramesAmount + 1);
            }

            animKeyframesSerializedAfterMove = JsonConvert.SerializeObject(new AnimationKeyFrameHolder() { kfs = anim.KeyFrames }, settings);
            anim.KeyFrames = JsonConvert.DeserializeObject<AnimationKeyFrameHolder>(animKeyframesSerializedBeforeMove, settings).kfs;
        }

        public override void DoTask()
        {
            anim.QuantityFrames = newQuantity;
            anim.KeyFrames = JsonConvert.DeserializeObject<AnimationKeyFrameHolder>(animKeyframesSerializedAfterMove, JsonSettings).kfs;
            //Due to quantity of frames changing, need to update whole anim data.
            AnimationEditorManager.main.OnAnimationDataChanged();
        }

        public override void UndoTask()
        {
            anim.QuantityFrames = oldQuantity; 
            anim.KeyFrames = JsonConvert.DeserializeObject<AnimationKeyFrameHolder>(animKeyframesSerializedBeforeMove, JsonSettings).kfs;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }

        JsonSerializerSettings JsonSettings
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy(false, false) }
                };
            }
        }
        
        /// <summary>
        /// This will immediately apply the key frame to the anim array, so ensure its done in the correct order.
        /// </summary>
        private void MoveOneKeyFrame(Animation anim, AnimationKeyFrameElement kfe, int flagForKFType, int moveAmount)
        {
            //Find this animation key frame element...
            int newFrame = kfe.Frame + moveAmount;
            //Will create the KFE if it doesn't exist.
            AnimationKeyFrameElement beingEdited = anim.GetOrCreateKeyFrame(newFrame).GetOrCreateKeyFrameElement(kfe.ForElement.Name);
            if (flagForKFType == 0) //Position
            {
                beingEdited.OffsetX = kfe.OffsetX;
                beingEdited.OffsetY = kfe.OffsetY;
                beingEdited.OffsetZ = kfe.OffsetZ;
                kfe.OffsetX = null;
                kfe.OffsetY = null;
                kfe.OffsetZ = null;
            }
            else if (flagForKFType == 1) //Rotation
            {
                beingEdited.RotationX = kfe.RotationX;
                beingEdited.RotationY = kfe.RotationY;
                beingEdited.RotationZ = kfe.RotationZ;
                kfe.RotationX = null;
                kfe.RotationY = null;
                kfe.RotationZ = null;
                //also copy rotate around setting
                beingEdited.RotShortestDistanceX = kfe.RotShortestDistanceX;
                beingEdited.RotShortestDistanceY = kfe.RotShortestDistanceY;
                beingEdited.RotShortestDistanceZ = kfe.RotShortestDistanceZ;
            }
        }


        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Move Key(s) Frames";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 8;
        }
    }
}