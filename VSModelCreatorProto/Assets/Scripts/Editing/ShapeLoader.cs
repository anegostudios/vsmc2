using System;
using System.IO;
using System.Runtime.Serialization;
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
        public UnityEvent<Shape> onShapeLoadedEvent;
        int lastAutoSaveLoc = 0;

        private void Awake()
        {
            main = this;
            onShapeLoadedEvent = new UnityEvent<Shape>();
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

        public static void RegisterForOnShapeLoadEvent(UnityAction<Shape> shape)
        {
            main.onShapeLoadedEvent.AddListener(shape);
        }

        public void CreateNewShape()
        {
            UnloadCurrentShape();
            Shape newShape = new Shape();
            newShape.Elements = new ShapeElement[0];

            ShapeElement initialElem = new ShapeElement();
            newShape.AddRootShapeElement(initialElem);

            newShape.Textures = new System.Collections.Generic.Dictionary<string, string>();
            newShape.Textures.Add("texture", "");

            newShape.ResolveFacesAndTextures(new StreamingContext());
            shapeHolder.OnShapeLoaded(newShape, true);
            hierarchy.StartCreatingElementPrefabs(newShape);
            EditModeManager.main.SelectMode(VSEditMode.Model);
            onShapeLoadedEvent.Invoke(newShape);
        }

        public void LoadShape(string filePath)
        {
            UnloadCurrentShape();

            //Load shape and then enter model mode.
            SaveManager.main.BeforeFileLoad();
            Shape loadedShape = ShapeAccessor.DeserializeShapeFromFile(filePath);
            shapeHolder.OnShapeLoaded(loadedShape);
            hierarchy.StartCreatingElementPrefabs(loadedShape);
            EditModeManager.main.SelectMode(VSEditMode.Model);
            onShapeLoadedEvent.Invoke(loadedShape);
        }

        public void SaveShape()
        {

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