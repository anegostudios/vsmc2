using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VSMC;

/// <summary>
/// Processes all undoable edit tasks and whatnot.
/// </summary>
public class UndoManager : MonoBehaviour
{

    public static UndoManager main;

    LinkedList<IEditTask> completedEditTasks;
    LinkedList<IEditTask> undoneEditTasks;

    UnityEvent OnAnyActionDoneOrUndone;

    private void Awake()
    {
        main = this;
        completedEditTasks = new LinkedList<IEditTask>();
        undoneEditTasks = new LinkedList<IEditTask>();
        OnAnyActionDoneOrUndone = new UnityEvent();
    }

    public static void RegisterForAnyActionDoneOrUndone(UnityAction func)
    {
        main.OnAnyActionDoneOrUndone.AddListener(func);
    }

    public void CommitTask(IEditTask newTask)
    {
        completedEditTasks.AddFirst(newTask);
        OnAnyActionDoneOrUndone.Invoke();
        undoneEditTasks.Clear();
        //uiElements.RefreshSelectionValues();
    }

    public long GetSizeOfTaskList()
    {
        long totRam = 0;
        foreach (IEditTask i in completedEditTasks)
        {
            totRam += i.GetSizeOfTaskInBytes();
        }
        return totRam;
    }

    public void UndoTopTask()
    {
        if (completedEditTasks.Count < 1) return;
        IEditTask toUndo = completedEditTasks.First.Value;
        toUndo.UndoTask();
        OnAnyActionDoneOrUndone.Invoke();
        undoneEditTasks.AddFirst(toUndo);
        completedEditTasks.RemoveFirst();
        //uiElements.RefreshSelectionValues();
    }

    public void RedoTopTask()
    {
        if (undoneEditTasks.Count < 1) return;
        IEditTask toRedo = undoneEditTasks.First.Value;
        toRedo.DoTask();
        OnAnyActionDoneOrUndone.Invoke();
        completedEditTasks.AddFirst(toRedo);
        undoneEditTasks.RemoveFirst();
        //uiElements.RefreshSelectionValues();
    }

    private void Update()
    {
        //Unity still reacts to Ctrl+Z updates 
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Z)) UndoTopTask();
        else if (Input.GetKeyDown(KeyCode.Y)) RedoTopTask();
        return;
#else
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Z)) UndoTopTask();
            else if (Input.GetKeyDown(KeyCode.Y)) RedoTopTask();
        }
#endif
    }

    /// <summary>
    /// Checks to see if top tasks can be merged and merges them.
    /// </summary>
    public void MergeTopTasks()
    {
        if (completedEditTasks.Count < 2) return;
        IEditTask recentTask = completedEditTasks.First.Value;
        IEditTask prevTask = completedEditTasks.First.Next.Value;
        while (prevTask.MergeTasksIfPossible(recentTask))
        {
            recentTask = prevTask;
            completedEditTasks.RemoveFirst();
            if (completedEditTasks.Count < 2) return;
            prevTask = completedEditTasks.First.Next.Value;
        }
    }


}
