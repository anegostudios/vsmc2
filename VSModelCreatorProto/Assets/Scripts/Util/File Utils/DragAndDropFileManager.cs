using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VSMC;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using System;

public class DragAndDropFileManager : MonoBehaviour
{
    public WindowsDragAndDropListener windowsListener;

    public string storedFilepath;
    public GameObject openShapeFileFromFileDropOverlay;
    public TMP_Text openShapeFileFromFileDropFilePathText;

    public GameObject openNewTextureFileFromFileDropOverlay;
    public TMP_Text openNewTextureFilePathText;
    public GameObject openLoadedTextureFileFromFileDropOverlay;
    public TMP_Text openLoadedTextureFilePathText;

    [Tooltip("If any of these GOs are  active, deny any drops.")]
    public GameObject[] allFileDropBlockingObjects;

    public Button[] onlyAllowIfAnElementIsSelected;

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

        bool isAnySelected = ObjectSelector.main.IsAnySelected();
        foreach (Button b in onlyAllowIfAnElementIsSelected)
        {
            b.interactable = isAnySelected;
        }

        storedFilepath = droppedFilePath[0];
        //Analyse based on path...
        if (Path.GetExtension(storedFilepath).ToLower() == ".png") //texture
        {
            if (ShapeHolder.CurrentLoadedShape == null)
            {
                InfoLogger.main.LogText("Received dropped file - Would load as texture, but please load a shape first!");
                return;
            }
            if (GetTextureFromFilepath() != null)
            {
                InfoLogger.main.LogText("Received dropped file - Loading as already-loaded texture file.");
                openLoadedTextureFilePathText.text = storedFilepath;
                openLoadedTextureFileFromFileDropOverlay.SetActive(true);
            }
            else
            {
                InfoLogger.main.LogText("Received dropped file - Loading as new texture file.");
                openNewTextureFilePathText.text = storedFilepath;
                openNewTextureFileFromFileDropOverlay.SetActive(true);
            }
        }
        else if (Path.GetExtension(storedFilepath).ToLower() == ".json") //shape... hopefully.
        {
            InfoLogger.main.LogText("Received dropped file - Loading as shape file.");
            //If no shape file, just load the shape immediately.
            if (ShapeHolder.CurrentLoadedShape == null)
            {
                OnOpenDroppedShapeAsNewFileAfterSaveDialog();
                return;
            }
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

        EditModeManager.main.SelectMode(VSEditMode.Model);
        BackdropManager.main.AddBackdropFromFileDrop(storedFilepath);
        openShapeFileFromFileDropOverlay.SetActive(false);
    }

    public void OnOpenDroppedShapeAsAttachment()
    {
        EditModeManager.main.SelectMode(VSEditMode.Model);
        AttachmentManager.main.AddAttachmentFromFileDrop(storedFilepath);
        openShapeFileFromFileDropOverlay.SetActive(false);
    }

    public void OnOpenDroppedShapeAsNewFile()
    {
        SaveOverlayManager.main.OpenSaveOverlayWithFunctions(null, OnOpenDroppedShapeAsNewFileAfterSaveDialog,
        "Open shape file", "This will open the dropped file as a new shape. Any unsaved changes will be lost.");
    }


    public void OnOpenDroppedShapeAsNewFileAfterSaveDialog()
    {
        ShapeLoader.main.LoadShape(storedFilepath);
        openShapeFileFromFileDropOverlay.SetActive(false);
    }

    LoadedTexture GetTextureFromFilepath()
    {
        string texPath = AssetPathManager.main.GetRelativePathForFile(storedFilepath, "textures").Replace(".png", "");
        texPath = Path.GetFullPath(AssetPathManager.main.FindTextureFilePath(texPath + ".png")); //May seem odd, but we want the files to resolve to the exact same path with the same format.
        foreach (LoadedTexture loadedTex in TextureManager.main.loadedTextures)
        {
            if (Path.GetFullPath(AssetPathManager.main.FindTextureFilePath(loadedTex.path + ".png")).Equals(texPath, StringComparison.CurrentCultureIgnoreCase))
            {
                return loadedTex;
            } 
        }
        return null;
    }

    public void OnLoadAsNewTexture()
    {
        EditModeManager.main.SelectMode(VSEditMode.Texture);
        TaskCreateNewTextureFromFile newTexTask = new TaskCreateNewTextureFromFile(storedFilepath);
        newTexTask.DoTask();
        UndoManager.main.CommitTask(newTexTask);
        InfoLogger.main.LogText("Texture loaded.");
        openNewTextureFileFromFileDropOverlay.SetActive(false);
        openLoadedTextureFileFromFileDropOverlay.SetActive(false);
    }

    public void OnApplyTextureToSelectedElement()
    {
        EditModeManager.main.SelectMode(VSEditMode.Texture);
        LoadedTexture tex = GetTextureFromFilepath();
        if (tex == null)
        {
            Debug.LogError("Cannot find texture to apply with. This shouldn't happen!");
            openNewTextureFileFromFileDropOverlay.SetActive(false);
            openLoadedTextureFileFromFileDropOverlay.SetActive(false);
            return;
        }
        TaskBatchSetTexture batchSetTextureTask = new TaskBatchSetTexture(tex.code, ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element, false);
        batchSetTextureTask.DoTask();
        UndoManager.main.CommitTask(batchSetTextureTask);
        InfoLogger.main.LogText("Texture successfully applied.");
        openNewTextureFileFromFileDropOverlay.SetActive(false);
        openLoadedTextureFileFromFileDropOverlay.SetActive(false);
    }

    public void OnApplyTextureToSelectedElementAndChildrenRecursive()
    {
        EditModeManager.main.SelectMode(VSEditMode.Texture);
        LoadedTexture tex = GetTextureFromFilepath();
        if (tex == null)
        {
            Debug.LogError("Cannot find texture to apply with. This shouldn't happen!");
            openNewTextureFileFromFileDropOverlay.SetActive(false);
            openLoadedTextureFileFromFileDropOverlay.SetActive(false);
            return;
        }
        TaskBatchSetTexture batchSetTextureTask = new TaskBatchSetTexture(tex.code, ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element);
        batchSetTextureTask.DoTask();
        UndoManager.main.CommitTask(batchSetTextureTask);
        InfoLogger.main.LogText("Texture successfully applied.");
        openNewTextureFileFromFileDropOverlay.SetActive(false);
        openLoadedTextureFileFromFileDropOverlay.SetActive(false);
    }

    public void OnApplyTextureToAllElements()
    {
        EditModeManager.main.SelectMode(VSEditMode.Texture);
        LoadedTexture tex = GetTextureFromFilepath();
        if (tex == null)
        {
            Debug.LogError("Cannot find texture to apply with. This shouldn't happen!");
            openNewTextureFileFromFileDropOverlay.SetActive(false);
            openLoadedTextureFileFromFileDropOverlay.SetActive(false);
            return;
        }
        TaskBatchSetTexture batchSetTextureTask = new TaskBatchSetTexture(tex.code);
        batchSetTextureTask.DoTask();
        UndoManager.main.CommitTask(batchSetTextureTask);
        InfoLogger.main.LogText("Texture successfully applied.");
        openNewTextureFileFromFileDropOverlay.SetActive(false);
        openLoadedTextureFileFromFileDropOverlay.SetActive(false);
    }


}
