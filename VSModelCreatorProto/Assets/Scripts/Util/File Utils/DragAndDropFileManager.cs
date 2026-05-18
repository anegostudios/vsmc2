using System.Collections.Generic;
using System.IO;
using Ookii.Dialogs;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class DragAndDropFileManager : MonoBehaviour
{
    public WindowsDragAndDropListener windowsListener;

    public string storedFilepath;
    public GameObject openShapeFileFromFileDropOverlay;
    public Text openShapeFileFromFileDropFilePathText;

    [Tooltip("If any of these GOs are  active, deny any drops.")]
    public GameObject[] allFileDropBlockingObjects;

    void Awake()
    {
#if UNITY_STANDALONE_WIN
        //Enable Windows listener.
        windowsListener.gameObject.SetActive(true);
#elif UNITY_STANDALONE_LINUX
        //Enable Linux listener.
#endif
    }

    public void OnReceivedFile(List<string> droppedFilePath)
    {
        //Only allow one file drop at a time.
        if (droppedFilePath.Count != 1)
        {
            InfoLogger.main.LogText("Received dropped files - But can only handle one at a time!");
            return;
        }

        //Check for blocking objects.
        foreach (GameObject go in allFileDropBlockingObjects)
        {
            if (go.activeInHierarchy)
            {
                InfoLogger.main.LogText("Received dropped file - But please finish current action! (Blocked by " + go.name + ")");
                return;
            }
        }

        storedFilepath = droppedFilePath[0];
        //Analyse based on path...
        if (Path.GetExtension(storedFilepath).ToLower() == ".png") //texture
        {
            InfoLogger.main.LogText("Received dropped file - Loading as texture file.");
        }
        else if (Path.GetExtension(storedFilepath).ToLower() == ".json") //shape... hopefully.
        {
            InfoLogger.main.LogText("Received dropped file - Loading as shape file.");
            openShapeFileFromFileDropFilePathText.text = storedFilepath;
            openShapeFileFromFileDropOverlay.SetActive(true);
        }
        else //invalid
        {
            InfoLogger.main.LogText("Received dropped file - But invalid type!");
        }
    }

    public void OnOpenDroppedShapeAsBackdrop()
    {
        BackdropManager.main.AddBackdropFromFileDrop(storedFilepath);
    }

    public void OnOpenDroppedShapeAsAttachment()
    {
        AttachmentManager.main.AddAttachmentFromFileDrop(storedFilepath);
    }

    public void OnOpenDroppedShapeAsNewFile()
    {
        SaveOverlayManager.main.OpenSaveOverlayWithFunctions(null, OnOpenDroppedShapeAsNewFileAfterSaveDialog,
        "Open shape file", "This will open the dropped file as a new shape. Any unsaved changes will be lost.");
    }
    
    public void OnOpenDroppedShapeAsNewFileAfterSaveDialog()
    {
        ShapeLoader.main.LoadShape(storedFilepath);
    }
    
}
