using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using SFB;
using UnityEngine;
using UnityEngine.Events;

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
        int lastAutoSaveLoc = 0;

        public string storedSaveLocationForFile;

        private void Awake()
        {
            main = this;
            onShapeLoadedEvent = new UnityEvent<Shape, LoadingContext>();
            beforeShapeSaveEvent = new UnityEvent<Shape>();
            //InvokeRepeating("AutosaveShapeIfLoaded", 5, 30);
        }

        public void AutosaveShapeIfLoaded()
        {
            if (ShapeHolder.CurrentLoadedShape == null) return;
            DateTime t1 = DateTime.Now;
            if (!Directory.Exists("autosaves")) Directory.CreateDirectory("autosaves");
            //ShapeAccessor.SerializeShapeToFile(ShapeHolder.CurrentLoadedShape, "autosaves/" + lastAutoSaveLoc + ".json");
            lastAutoSaveLoc++;
            Debug.Log("Autosave took " + (DateTime.Now - t1).TotalMilliseconds + "ms.");
        }

        public static void RegisterForOnShapeLoadEvent(UnityAction<Shape, LoadingContext> shape)
        {
            main.onShapeLoadedEvent.AddListener(shape);
        }

        public static void RegisterForOnShapeSaveEvent(UnityAction<Shape> shape)
        {
            main.beforeShapeSaveEvent.AddListener(shape);
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

    }
}