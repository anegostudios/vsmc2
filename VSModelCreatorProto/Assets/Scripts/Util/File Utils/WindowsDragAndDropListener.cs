using UnityEngine;
using B83.Win32;
using System.Collections.Generic;
using System.Linq;

public class WindowsDragAndDropListener : MonoBehaviour
{
    public DragAndDropFileManager fileManager;

#if UNITY_STANDALONE_WIN
    
    void OnEnable()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
        Application.quitting += OnQuitting;
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        fileManager.OnReceivedFile(aFiles);
    }

    void OnQuitting()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    

#endif

}
