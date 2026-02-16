using System.Collections.Generic;

namespace VSMC
{
    /// <summary>
    /// An interface for all undoable edit tasks.
    /// Any operation performed on a shape should inherit from this, which also provides a lovely way of splitting functionality.
    /// The smaller these tasks are in memory usage, the more effective the undo/redo system can be.
    /// <br/>
    /// For undo/redo to work correctly, it is important that undo-ing results in the exact same post-constructor state.
    /// Therefore, if any preperation needs doing for the task, it must be done in the constructor, before 'DoTask' is called.
    /// </summary>
    public abstract class IEditTask
    {

        /// <summary>
        /// Performs or 'redo' the task.
        /// </summary>
        public abstract void DoTask();

        /// <summary>
        /// Undo a task.
        /// </summary>
        public abstract void UndoTask();
    
        /// <summary>
        /// If possible, merges two (usually identical-type) consecutive tasks. This task is always the one that happened first. 
        /// For example, a movement of (0, 0, 1) followed immediately by a movement of (1, 0, 1) may be merged into (1, 0, 2).
        /// Likely used with the 3D editor to group live-movement tasks into one undoable task.
        /// </summary>
        public abstract bool MergeTasksIfPossible(IEditTask nextTask);

        /// <summary>
        /// Mainly for debugging purposes. 
        /// Returns, roughly, the size in bytes of the task. C# black magic means this will usually not be perfectly accurate.
        /// </summary>
        public abstract long GetSizeOfTaskInBytes();

        /// <summary>
        /// Returns the edit mode that must be active to perform or undo this task.
        /// When a task is undone or redone, it will automatically switch to this edit mode.
        /// Use 'None' to prevent mode switching.
        /// </summary>
        public abstract VSEditMode GetRequiredEditMode();

        /// <summary>
        /// Returns a readable task name, for use within the UI.
        /// </summary>
        public abstract string GetTaskName();

    }
}