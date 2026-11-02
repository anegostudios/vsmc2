using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;

namespace VSMC
{
    public class TaskPasteKeyframeElementData : IEditTask
    {

        AnimationKeyFrameElement kfeToEdit;
        string oldKeyframeElemAsJSON;
        string newKeyframeElemAsJSON;
        int option;

        public TaskPasteKeyframeElementData(AnimationKeyFrameElement kfe, string newJson, int option)
        {
            this.kfeToEdit = kfe;
            this.option = option;
            this.newKeyframeElemAsJSON = newJson;
            oldKeyframeElemAsJSON = JsonConvert.SerializeObject(kfe, Formatting.Indented, ShapeAccessor.MainSettings);
        }

        public override void DoTask()
        {
            AnimationKeyFrameElement load;
            try
            {
                load = JsonConvert.DeserializeObject<AnimationKeyFrameElement>(newKeyframeElemAsJSON);
            }
            catch
            {
                InfoLogger.main.LogText("The clipboard does not contain a valid keyframe!");
                return;
            }

            CopyKFE(kfeToEdit, load, option);
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
            InfoLogger.main.LogText("Successfully pasted frame data into selected frame.");

        }

        public void CopyKFE(AnimationKeyFrameElement to, AnimationKeyFrameElement fr, int option)
        {
            if (option == 0)
            {
                to.OriginX = fr.OriginX;
                to.OriginY = fr.OriginY;
                to.OriginZ = fr.OriginZ;
            }
            if (option == 1 || option == 0)
            {
                to.OffsetX = fr.OffsetX;
                to.OffsetY = fr.OffsetY;
                to.OffsetZ = fr.OffsetZ;
            }
            if (option == 2 || option == 0)
            {
                to.RotationX = fr.RotationX;
                to.RotationY = fr.RotationY;
                to.RotationZ = fr.RotationZ;
                to.RotShortestDistanceX = fr.RotShortestDistanceX;
                to.RotShortestDistanceY = fr.RotShortestDistanceY;
                to.RotShortestDistanceZ = fr.RotShortestDistanceZ;
            }
            if (option == 3 || option == 0)
            {
                to.StretchX = fr.StretchX;
                to.StretchY = fr.StretchY;
                to.StretchZ = fr.StretchZ;
            }
        }

        public override void UndoTask()
        {
            AnimationKeyFrameElement load;
            try
            {
                load = JsonConvert.DeserializeObject<AnimationKeyFrameElement>(oldKeyframeElemAsJSON);
            }
            catch
            {
                InfoLogger.main.LogText("The clipboard does not contain a valid keyframe!");
                return;
            }
            CopyKFE(kfeToEdit, load, option);
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
            InfoLogger.main.LogText("Successfully pasted frame data into selected frame.");
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Paste Keyframe Data";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}