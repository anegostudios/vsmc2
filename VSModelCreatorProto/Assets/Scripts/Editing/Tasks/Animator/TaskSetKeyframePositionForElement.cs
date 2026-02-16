namespace VSMC
{
    /// <summary>
    /// Sets the position of a keyframe element.
    /// </summary>
    public class TaskSetKeyframePositionForElement : IEditTask
    {

        AnimationKeyFrameElement kfElem;
        double posX;
        double posY;
        double posZ;
        double? iPosX;
        double? iPosY;
        double? iPosZ;

        public TaskSetKeyframePositionForElement(AnimationKeyFrameElement kfElem, double posX, double posY, double posZ)
        {
            this.kfElem = kfElem;
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
            this.iPosX = kfElem.OffsetX;
            this.iPosY = kfElem.OffsetY;
            this.iPosZ = kfElem.OffsetZ;
        }

        public override void DoTask()
        {
            kfElem.OffsetX = posX;
            kfElem.OffsetY = posY;
            kfElem.OffsetZ = posZ;
            AnimationEditorManager.main.OnAnyKeyframeChanged();
        }

        public override void UndoTask()
        {
            kfElem.OffsetX = iPosX;
            kfElem.OffsetY = iPosY;
            kfElem.OffsetZ = iPosZ;
            AnimationEditorManager.main.OnAnyKeyframeAddedOrRemoved();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Animation;
        }

        public override string GetTaskName()
        {
            return "Edit Keyframe Position";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(double) * 6;
        }
    }
}