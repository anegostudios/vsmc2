using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;

namespace VSMC
{
    public class TaskPasteSingleKeyframeData : IEditTask
    {

        Animation animationToStore;
        int kfIndex;
        string oldKeyframeAsJSON;
        string newKeyframeAsJSON;

        public TaskPasteSingleKeyframeData(Animation intoAnim, int kfIndex, string clipboardData)
        {
            this.kfIndex = kfIndex;
            AnimationKeyFrame kf = intoAnim.KeyFrames.FirstOrDefault(x => x.Frame == kfIndex);
            if (kf != null)
            {
                oldKeyframeAsJSON = JsonConvert.SerializeObject(kf, Formatting.Indented, ShapeAccessor.MainSettings);
            }
            else
            {
                oldKeyframeAsJSON = null;
            }
            this.animationToStore = intoAnim;
            this.newKeyframeAsJSON = clipboardData;
        }

        public override void DoTask()
        {
            AnimationKeyFrame load;
            try
            {
                load = JsonConvert.DeserializeObject<AnimationKeyFrame>(newKeyframeAsJSON);
            }
            catch
            {
                InfoLogger.main.LogText("The clipboard does not contain a valid keyframe!");
                return;
            }
            load.Frame = kfIndex;
            if (oldKeyframeAsJSON != null)
            {
                animationToStore.KeyFrames = animationToStore.KeyFrames.Remove(animationToStore.KeyFrames.FirstOrDefault(x => x.Frame == kfIndex));
            } 
            animationToStore.KeyFrames = animationToStore.KeyFrames.Append(load);
            AnimationEditorManager.main.OnAnimationDataChanged();
            InfoLogger.main.LogText("Successfully pasted frame data into selected frame.");

        }

        public override void UndoTask()
        {
            AnimationKeyFrame load;
            animationToStore.KeyFrames = animationToStore.KeyFrames.Remove(animationToStore.KeyFrames.FirstOrDefault(x => x.Frame == kfIndex));

            if (oldKeyframeAsJSON != null)
            {
                try
                {
                    load = JsonConvert.DeserializeObject<AnimationKeyFrame>(oldKeyframeAsJSON);
                }
                catch
                {
                    InfoLogger.main.LogText("The clipboard does not contain a valid keyframe!");
                    return;
                }
                load.Frame = kfIndex;
                animationToStore.KeyFrames = animationToStore.KeyFrames.Append(load);
            }
            AnimationEditorManager.main.OnAnimationDataChanged();
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