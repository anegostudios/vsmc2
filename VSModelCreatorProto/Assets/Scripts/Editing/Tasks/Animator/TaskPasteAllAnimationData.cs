using Newtonsoft.Json;

namespace VSMC
{
    public class TaskPasteAllAnimationData : IEditTask
    {

        Animation animationToStore;
        string oldAnimationAsJSON;
        string newAnimationAsJSON;

        public TaskPasteAllAnimationData(Animation intoAnim, string clipboardData)
        {
            oldAnimationAsJSON = JsonConvert.SerializeObject(intoAnim, Formatting.Indented, ShapeAccessor.MainSettings);
            this.animationToStore = intoAnim;
            this.newAnimationAsJSON = clipboardData;
        }

        public override void DoTask()
        {
            try
            {
                Animation copyFrom = JsonConvert.DeserializeObject<Animation>(newAnimationAsJSON);
                animationToStore.QuantityFrames = copyFrom.QuantityFrames;
                animationToStore.Version = copyFrom.Version;
                animationToStore.EaseAnimationSpeed = copyFrom.EaseAnimationSpeed;
                animationToStore.KeyFrames = copyFrom.KeyFrames;
                animationToStore.OnActivityStopped = copyFrom.OnActivityStopped;
                animationToStore.OnAnimationEnd = copyFrom.OnAnimationEnd;
                AnimationEditorManager.main.OnAnimationDataChanged();
                InfoLogger.main.LogText("Successfully pasted all frame data from clipboard.");
            }
            catch
            {
                InfoLogger.main.LogText("The clipboard does not contain a valid animation!");
                return;
            }
        }

        public override void UndoTask()
        {
            Animation copyFrom = JsonConvert.DeserializeObject<Animation>(oldAnimationAsJSON);
            animationToStore.QuantityFrames = copyFrom.QuantityFrames;
            animationToStore.Version = copyFrom.Version;
            animationToStore.EaseAnimationSpeed = copyFrom.EaseAnimationSpeed;
            animationToStore.KeyFrames = copyFrom.KeyFrames;
            animationToStore.OnActivityStopped = copyFrom.OnActivityStopped;
            animationToStore.OnAnimationEnd = copyFrom.OnAnimationEnd;
            AnimationEditorManager.main.OnAnimationDataChanged();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Paste Animation Data";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

    }
}