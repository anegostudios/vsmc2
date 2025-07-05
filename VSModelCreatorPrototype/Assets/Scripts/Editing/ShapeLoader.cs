using UnityEngine;

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

        private void Awake()
        {
            main = this;
        }

        public void LoadShape(string filePath)
        {
            UnloadCurrentShape();

            //Load shape and then enter model mode.
            Shape loadedShape = ShapeAccessor.DeserializeShapeFromFile(filePath);
            shapeHolder.OnShapeLoaded(loadedShape);
            hierarchy.StartCreatingElementPrefabs(loadedShape);
            EditModeManager.main.SelectMode(VSEditMode.Model);
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
        }

    }
}