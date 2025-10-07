using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VSMC;

/// <summary>
/// Processes all undoable edit tasks and whatnot.
/// </summary>
public class UndoManager : MonoBehaviour
{

    public static UndoManager main;
    public GameObject[] objectsToDeactivateWhenUndoOrRedo;
    public TMP_Text undoText;
    public TMP_Text redoText;
    public Color defaultTextColor;
    public Color disabledTextColor;

    LinkedList<IEditTask> completedEditTasks;
    LinkedList<IEditTask> undoneEditTasks;

    UnityEvent OnAnyActionDoneOrUndone;

    private void Awake()
    {
        main = this;
        completedEditTasks = new LinkedList<IEditTask>();
        undoneEditTasks = new LinkedList<IEditTask>();
        OnAnyActionDoneOrUndone = new UnityEvent();
        OnAnyActionDoneOrUndone.AddListener(OnAnyAction);
        OnAnyAction();
    }

    public static void RegisterForAnyActionDoneOrUndone(UnityAction func)
    {
        main.OnAnyActionDoneOrUndone.AddListener(func);
    }

    /// <summary>
    /// Will just refresh the undo and redo buttons text and interactabiity.
    /// </summary>
    public void OnAnyAction()
    {
        if (completedEditTasks.Count <= 0)
        {
            undoText.GetComponentInParent<Toggle>(true).interactable = false;
            undoText.text = "Undo (Ctrl+Z)";
            undoText.color = disabledTextColor;
        }
        else
        {
            undoText.GetComponentInParent<Toggle>(true).interactable = true;
            undoText.text = "Undo " + completedEditTasks.First.Value.GetTaskName() + " (Ctrl+Z)";
            undoText.color = defaultTextColor;
        }

        if (undoneEditTasks.Count <= 0)
        {
            redoText.GetComponentInParent<Toggle>(true).interactable = false;
            redoText.text = "Redo (Ctrl+Y)";
            redoText.color = disabledTextColor;
        }
        else
        {
            redoText.GetComponentInParent<Toggle>(true).interactable = true;
            redoText.text = "Redo " + undoneEditTasks.First.Value.GetTaskName() + " (Ctrl+Y)";
            redoText.color = defaultTextColor;
        }
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

        //Need to set edit mode to correct mode for undoing.
        EditModeManager.main.SelectMode(toUndo.GetRequiredEditMode());

        toUndo.UndoTask();
        undoneEditTasks.AddFirst(toUndo);
        completedEditTasks.RemoveFirst();
        OnAnyActionDoneOrUndone.Invoke();
        //uiElements.RefreshSelectionValues();
        foreach (GameObject g in objectsToDeactivateWhenUndoOrRedo)
        {
            g.SetActive(false);
        }
    }

    public void RedoTopTask()
    {
        if (undoneEditTasks.Count < 1) return;
        IEditTask toRedo = undoneEditTasks.First.Value;

        //Need to set edit mode to correct mode for redoing.
        EditModeManager.main.SelectMode(toRedo.GetRequiredEditMode());

        toRedo.DoTask();
        completedEditTasks.AddFirst(toRedo);
        undoneEditTasks.RemoveFirst();
        OnAnyActionDoneOrUndone.Invoke();
        //uiElements.RefreshSelectionValues();
        foreach (GameObject g in objectsToDeactivateWhenUndoOrRedo)
        {
            g.SetActive(false);
        }
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
