using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VSMC {
    /// <summary>
    /// ShapeLoader is responsible for loading and saving a shape through the <see cref="ShapeAccessor"/>.
    /// It is accessed through the UI and passes the data into <see cref="ShapeHolder"/>.
    /// </summary>
    public class ShapeLoader : MonoBehaviour
    {
        public static ShapeLoader main;

        [Header("Unity References")]
        public ShapeHolder shapeHolder;
        public ElementHierarchyManager hierarchy;
        public UnityEvent<Shape, LoadingContext> onShapeLoadedEvent;
        public UnityEvent<Shape> beforeShapeSaveEvent;
        public TMP_Text currentlyLoaded;

        string storedSaveLocation;

        public string storedSaveLocationForFile
        {
            get { return storedSaveLocation; }
            set { storedSaveLocation = value; SetCurrentLoadedText(); }
        }
        string persistentDataPath;

        bool hasTaskOccuredForAutosaveBlock;
        int autosaveSuccessCode = 0;
        DateTime autosaveStartTime;

        private void Awake()
        {
            main = this;
            onShapeLoadedEvent = new UnityEvent<Shape, LoadingContext>();
            beforeShapeSaveEvent = new UnityEvent<Shape>();
            //InvokeRepeating("AutosaveShapeIfLoaded", 5, 30);
        }

        void Start()
        {
            UndoManager.RegisterForAnyActionDoneOrUndone(OnAnyActionDone);
        }

        void Update()
        {
            if (autosaveSuccessCode != 0)
            {
                if (autosaveSuccessCode == 1)
                {
                    InfoLogger.main.LogText("File restore point created. Took " + (DateTime.Now - autosaveStartTime).TotalMilliseconds + "ms.");
                }
                else if (autosaveSuccessCode == 2)
                {
                    InfoLogger.main.LogText("Tried to make a file restore point but failed due to user task."); 
                }
                else
                {
                    InfoLogger.main.LogText("File restore point failed for other reason.");
                }
                autosaveSuccessCode = 0;
            } 
        }

        void OnAnyActionDone()
        {
            hasTaskOccuredForAutosaveBlock = true;
        }

        public void BeginAutosaveThread()
        {
            autosaveSuccessCode = 0;
            autosaveStartTime = DateTime.Now;
            persistentDataPath = Application.persistentDataPath;
            Thread autosaveThread = new Thread(AutosaveShapeIfLoaded);
            autosaveThread.Start();
        }

        /// <summary>
        /// This is run on a seperate thread.
        /// </summary>
        public void AutosaveShapeIfLoaded()
        {
            if (ShapeHolder.CurrentLoadedShape == null) return;
            hasTaskOccuredForAutosaveBlock = false;
            string autosavePath = persistentDataPath + Path.DirectorySeparatorChar + "restore-points" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(storedSaveLocationForFile) + DateTime.Now.ToString("s").Replace(':', '-') + ".json";
            if (!Directory.Exists(Path.GetDirectoryName(autosavePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(autosavePath));
            }
            try
            {
                string serializedShape = ShapeAccessor.SerializeShapeToString(ShapeHolder.CurrentLoadedShape, beforeShapeSaveEvent, true);
                if (hasTaskOccuredForAutosaveBlock)
                {
                    autosaveSuccessCode = 2;
                    return;
                }
                File.WriteAllText(autosavePath, serializedShape);
                SaveManager.main.OnModelSave(autosavePath, true);
            }
            catch
            {
                autosaveSuccessCode = 2;
                return;
            }

            //Clear early autosaves.
            string[] files = Directory.GetFiles(persistentDataPath + Path.DirectorySeparatorChar + "restore-points");
            if (files.Length > 32)
            {
                //Start of array is one to remove.
                string[] ordered = files.OrderBy(f => File.GetLastWriteTime(f)).ToArray();
                File.Delete(ordered[0]);
            }
            autosaveSuccessCode = 1;
        }

        public static void RegisterForOnShapeLoadEvent(UnityAction<Shape, LoadingContext> shape)
        {
            main.onShapeLoadedEvent.AddListener(shape);
        }

        public static void RegisterForOnShapeSaveEvent(UnityAction<Shape> shape)
        {
            main.beforeShapeSaveEvent.AddListener(shape);
        }

        public void OnCreateNewShapeButton()
        {
            SaveOverlayManager.main.OpenSaveOverlayWithFunctions(null, CreateNewShape, "Create new shape", "You are creating a new shape.");
        }

        public void OnLoadShapeButton()
        {
            SaveOverlayManager.main.OpenSaveOverlayWithFunctions(null, DoLoadShape, "Load Shape", "You are loading a shape.");
        }

        public void DoLoadShape()
        {
            string[] selectedFiles = StandaloneFileBrowser.OpenFilePanel("Open Shape Files", "", "json", true);
            if (selectedFiles == null || selectedFiles.Length == 0 || selectedFiles[0].Trim().Length == 0) { return; }
            LoadShape(selectedFiles[0]);
        }

        public void CreateNewShape()
        {
            UnloadCurrentShape();
            storedSaveLocationForFile = null;
            Shape newShape = new Shape();
            newShape.Elements = new ShapeElement[0];

            ShapeElement initialElem = new ShapeElement();
            newShape.AddRootShapeElement(initialElem);

            newShape.Textures = new System.Collections.Generic.Dictionary<string, string>();
            newShape.Textures.Add("texture", "");

            newShape.ResolveFacesAndTextures(new StreamingContext(StreamingContextStates.File, true));


            shapeHolder.OnShapeLoaded(newShape, true);
            hierarchy.StartCreatingElementPrefabs(newShape);
            EditModeManager.main.SelectMode(VSEditMode.Model);
            onShapeLoadedEvent.Invoke(newShape, null);
            InfoLogger.main.LogText("New shape created.");
        }

        public void LoadShape(string filePath, LoadingContext context = null)
        {
            UnloadCurrentShape();
            storedSaveLocationForFile = filePath;
            //Load shape and then enter model mode.
            SaveManager.main.BeforeFileLoad();
            Shape loadedShape = ShapeAccessor.DeserializeShapeFromFile(filePath);
            //Check for stepparents in the root elements.
            foreach (ShapeElement e in loadedShape.Elements)
            {
                e.SearchForStepParentInShape(loadedShape);
            }
            shapeHolder.OnShapeLoaded(loadedShape);
            hierarchy.StartCreatingElementPrefabs(loadedShape);
            EditModeManager.main.SelectMode(VSEditMode.Model);
            onShapeLoadedEvent.Invoke(loadedShape, context);
            InfoLogger.main.LogText("Loaded shape successfully.");
        }

        public void SaveShapeToStoredPath()
        {
            if (ShapeHolder.CurrentLoadedShape == null)
            {
                Debug.Log("Trying to save nothing...");
                return;
            }
            if (storedSaveLocationForFile == "" || storedSaveLocationForFile == null)
            {
                SaveShapeWithFileSelect();
                return;
            }
            ShapeAccessor.SerializeShapeToFile(ShapeHolder.CurrentLoadedShape, storedSaveLocationForFile, beforeShapeSaveEvent);
            InfoLogger.main.LogText("Successfully saved shape.");
        }


        public void SaveShapeWithFileSelect()
        {
            if (ShapeHolder.CurrentLoadedShape == null)
            {
                Debug.Log("Trying to save nothing...");
                return;
            }
            string saveTo = StandaloneFileBrowser.SaveFilePanel("Save file path", Path.GetDirectoryName(storedSaveLocationForFile), Path.GetFileName(storedSaveLocationForFile), "json");
            if (saveTo == null || saveTo.Length < 1)
            {
                return;
            }
            if (!saveTo.EndsWith(".json"))
            {
                saveTo += ".json";
            }
            storedSaveLocationForFile = saveTo;
            ShapeAccessor.SerializeShapeToFile(ShapeHolder.CurrentLoadedShape, saveTo, beforeShapeSaveEvent);
            InfoLogger.main.LogText("Successfully saved shape.");
        }

        public void UnloadCurrentShape()
        {
            shapeHolder.UnloadCurrentShape();

            //Reset shape IDs.
            EditModeManager.main.SelectMode(VSEditMode.None);
            ShapeElementRegistry.main.ClearForNewModel();
            UndoManager.main.ReInit();
        }

        void SetCurrentLoadedText()
        {
            if (storedSaveLocation == null || storedSaveLocation == "")
            {
                currentlyLoaded.text = "";
                return;
            }
            currentlyLoaded.text = "Editing " + Path.GetFileName(storedSaveLocation);
        }

    }
}