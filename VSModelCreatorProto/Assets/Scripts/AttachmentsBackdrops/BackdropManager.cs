using System.Collections.Generic;
using System.Linq;
using SFB;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// This manages all backdrops loaded by the object.
    /// Only one backdrop is enabled at any one time.
    /// </summary>
    public class BackdropManager : MonoBehaviour
    {

        public static BackdropManager main;

        public GameObject backdropHolderPrefab;
        public List<LoadedBackdrop> allBackdrops;
        public LoadedBackdrop cActiveBackdrop;


        [Header("Misc")]
        public Color[] colorsForUnTexturedObjects;

        public string CurrentActiveBackdropPath
        {
            get
            {
                return cActiveBackdrop.data.shapeFilepath;
            }
        }

        public Color GetColorFromIndex(int colorIndex)
        {
            if (colorIndex < 0) return colorsForUnTexturedObjects[0];
            return colorsForUnTexturedObjects[colorIndex % colorsForUnTexturedObjects.Length];
        }

        void Awake()
        {
            main = this;
        }

        void Start()
        {
            ShapeLoader.RegisterForOnShapeLoadEvent(LoadBackdropsFromShapeData);
            ShapeLoader.RegisterForOnShapeSaveEvent(AddBackdropDataIntoShape);
        }

        /// <summary>
        /// Called when a shape is loaded. Will access the backdrop data and create them.
        /// </summary>
        void LoadBackdropsFromShapeData(Shape shape, LoadingContext context)
        {
            Debug.Log ("Loading backdrop data from shape.");
            cActiveBackdrop = null;
            allBackdrops = new List<LoadedBackdrop>();
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }

            if (shape?.editor?.backdrops != null)
            {
                foreach (BackdropOrAttachmentData data in shape.editor.backdrops)
                {
                    CreateAndInitializeBackdropFromData(data);
                }
            }

            if (context is LoadIntoAttachmentContext attContext)
            {
                //Check if attachment is loaded.
                string localPath = AssetPathManager.main.GetRelativePathForFile(attContext.fullPathToShapeLoadedFrom, "shapes").Replace(".json", "");
                LoadedBackdrop bd = GetBackdropFromPath(localPath);
                if (bd != null)
                {
                    foreach (LoadedBackdrop b in allBackdrops)
                    {
                        b.SetBackdropEnabled(false);
                    }
                    SetEnabledBackdrop(bd);
                    BackdropAndAttachmentMenuManager.main.RecreateBackdropList(allBackdrops);
                    return;
                }
                else
                {
                    SetEnabledBackdrop(CreateNewBackdrop(localPath));
                    BackdropAndAttachmentMenuManager.main.RecreateBackdropList(allBackdrops);
                    return;
                }
            }

            if (allBackdrops.Count == 0)
            {
                //Create a backdrop from the VSMC1 data. This will be initially active.
#pragma warning disable CS0618 //Ignore obsolete warning.
                if (shape?.editor?.backDropShape != null && shape?.editor?.backDropShape != "")
                {
                    Debug.LogWarning("Loading old backdrop path.");
                    SetEnabledBackdrop(CreateNewBackdrop(shape.editor.backDropShape));
                }
                shape.editor.backDropShape = null;
#pragma warning restore CS0618 //Ignore obsolete warning.
            }

            FindCurrentActiveBackdrop();
            if (cActiveBackdrop != null) cActiveBackdrop.OnBackdropEnabled();
            BackdropAndAttachmentMenuManager.main.RecreateBackdropList(allBackdrops);

        }

        /// <summary>
        /// Creates a backdrop gameobject from the loaded or created data. 
        /// This will not enable the object.
        /// </summary>
        public LoadedBackdrop CreateAndInitializeBackdropFromData(BackdropOrAttachmentData data, int specificIndex = -1)
        {
            Debug.LogWarning("Creating new prefab for backdrop.");
            LoadedBackdrop backdrop = new LoadedBackdrop(data,
                    Instantiate(backdropHolderPrefab, transform).GetComponentInChildren<BackdropHolder>());
            if (specificIndex == -1)
            {
                allBackdrops.Add(backdrop);
            }
            else
            {
                allBackdrops.Insert(specificIndex, backdrop);
            }
            BackdropAndAttachmentMenuManager.main.RecreateBackdropList(allBackdrops);
            return backdrop;
        }

        /// <summary>
        /// Creates a backdrop from a shape filepath.
        /// </summary>
        public LoadedBackdrop CreateNewBackdrop(string fromPath)
        {
            if (GetBackdropFromPath(fromPath) != null)
            {
                Debug.Log("Cannot create backdrop from this shape as it already exists.");
                return null;
            }
            BackdropOrAttachmentData data = new BackdropOrAttachmentData(fromPath);
            return CreateAndInitializeBackdropFromData(data);
        }

        public void RemoveBackdrop(LoadedBackdrop backdrop)
        {
            if (cActiveBackdrop == backdrop) DisableCurrentBackdrop();
            allBackdrops.Remove(backdrop);
            Destroy(backdrop.backdropHolder.gameObject);
            BackdropAndAttachmentMenuManager.main.RecreateBackdropList(allBackdrops);
        }

        public void OnAddBackdropButtonClicked()
        {
            try
            {
                string[] selectedFiles = StandaloneFileBrowser.OpenFilePanel("Select Backdrop Shape Files", AssetPathManager.main.GetFirstPreferredAssetPath(), "json", true);
                if (selectedFiles.Length < 1 || selectedFiles[0] == "") return;
                foreach (string s in selectedFiles)
                {
                    string sRel = AssetPathManager.main.GetRelativePathForFile(s, "shapes").Replace(".json", "");
                    if (GetBackdropFromPath(sRel) != null) continue; //Cannot duplicate backdrops.
                    TaskCreateNewBackdrop addBackdrop = new TaskCreateNewBackdrop(sRel);
                    addBackdrop.DoTask();
                    UndoManager.main.CommitTask(addBackdrop);
                }
                BackdropAndAttachmentMenuManager.main.SelectBackdrop(allBackdrops[allBackdrops.Count - 1]);
                SetEnabledBackdrop(allBackdrops[allBackdrops.Count - 1]);
            }
            catch { return; }
        }


        public void AddBackdropFromFileDrop(string path)
        {
            string sRel = AssetPathManager.main.GetRelativePathForFile(path, "shapes").Replace(".json", "");
            if (GetBackdropFromPath(sRel) != null) return; //Cannot duplicate backdrops.
            TaskCreateNewBackdrop addBackdrop = new TaskCreateNewBackdrop(sRel);
            addBackdrop.DoTask();
            UndoManager.main.CommitTask(addBackdrop);
            BackdropAndAttachmentMenuManager.main.SelectBackdrop(allBackdrops[allBackdrops.Count - 1]);
            SetEnabledBackdrop(allBackdrops[allBackdrops.Count - 1]);
        }

        /// <summary>
        /// Finds the currently enabled backdrop based on the files, as well as verifying there is only one active.
        /// </summary>
        void FindCurrentActiveBackdrop()
        {
            cActiveBackdrop = null;
            bool foundActive = false;
            for (int i = 0; i < allBackdrops.Count; i++)
            {
                if (allBackdrops[i].data.enabled)
                {
                    if (foundActive) allBackdrops[i].data.enabled = false;
                    else
                    {
                        foundActive = true;
                        SetEnabledBackdrop(allBackdrops[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a backdrop's gameobjects and removes unregisters its data.
        /// </summary>
        /// <param name="toDelete"></param>
        public void DeleteBackdropAndGameObjects(LoadedBackdrop toDelete)
        {
            GameObject.Destroy(toDelete.backdropHolder);
            allBackdrops.Remove(toDelete);
            if (cActiveBackdrop == toDelete) cActiveBackdrop = null;
        }


        public void SetEnabledBackdrop(LoadedBackdrop backdrop)
        {
            if (cActiveBackdrop == backdrop) return;
            if (cActiveBackdrop != null) DisableCurrentBackdrop();
            backdrop.data.enabled = true;
            backdrop.OnBackdropEnabled();
            cActiveBackdrop = backdrop;
            BackdropAndAttachmentMenuManager.main.RecreateBackdropList(allBackdrops);
        }

        public void SetEnabledBackdrop(string backdrop)
        {
            SetEnabledBackdrop(GetBackdropFromPath(backdrop));
        }

        public void DisableCurrentBackdrop()
        {
            if (cActiveBackdrop == null) return;
            cActiveBackdrop.data.enabled = false;
            cActiveBackdrop.OnBackdropDisabled();
            BackdropAndAttachmentMenuManager.main.RecreateBackdropList(allBackdrops);
            cActiveBackdrop = null;
        }

        public void RefreshCurrentStepparents()
        {
            if (cActiveBackdrop != null)
            {
                cActiveBackdrop.OnBackdropDisabled();
                cActiveBackdrop.OnBackdropEnabled();
            }
        }

        public LoadedBackdrop GetBackdropFromPath(string path)
        {
            return allBackdrops.FirstOrDefault(x => x.data.shapeFilepath == path);
        }

        void AddBackdropDataIntoShape(Shape shape)
        {
            List<BackdropOrAttachmentData> backdropData = new List<BackdropOrAttachmentData>();
            foreach (LoadedBackdrop backdrop in allBackdrops)
            {
                backdropData.Add(backdrop.data);
            }
            shape.editor.backdrops = backdropData.ToArray();
        }

    }
}