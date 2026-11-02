using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameWindowTitleSetterWindows : MonoBehaviour
{

#if UNITY_STANDALONE_WIN
    //Credit to https://discussions.unity.com/t/how-can-i-change-the-title-of-the-standalone-player-window/25635/1

    //Import the following.
    [DllImport("user32.dll", EntryPoint =  "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
    public static IntPtr launchedWindowPtr;

    void Awake()
    {
        launchedWindowPtr = FindWindow(null, "VSMC2");
        SetWindowText(launchedWindowPtr, "VSMC2 ("+Application.version+")");
        Debug.Log("Found window pointer.");
    }
#endif

    public static void SetWindowText(string forPath)
    {
#if UNITY_STANDALONE_WIN
        if (forPath == null) 
        {
            SetWindowText(launchedWindowPtr, "VSMC2 ("+Application.version+")");
            return;
        }
        SetWindowText(launchedWindowPtr, "VSMC2 (" + Application.version + ") - " + forPath);
#endif
    }

}
