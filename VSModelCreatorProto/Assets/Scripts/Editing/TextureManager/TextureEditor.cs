using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC {
    /// <summary>
    /// This is the main class for editing any texture or UV related features.
    /// Most editing functionality is done inside of <see cref="TextureEditorUIElements"/>.
    /// See <see cref="TextureManager"/> for the loading and maagement of actual texture files.
    /// </summary>
    public class TextureEditor : MonoBehaviour
    {

        public static TextureEditor main;

        [Header("Unity References")]
        public TextureEditorUIElements uiElements;
        public ObjectSelector objectSelector;

        [Header("Entity Texturing")]
        public GameObject entityTextureModeButtonToggleIcon;
        public GameObject entityTextureEnableOverlay;

        public Selectable[] onlyActiveOnTextureModeInteractables;

        void Awake()
        {
            main = this;  
        }

        private void Start()
        {
            objectSelector.RegisterForObjectSelectedEvent(OnObjectSelected);
            objectSelector.RegisterForObjectDeselectedEvent(OnObjectDeselcted);
            EditModeManager.RegisterForOnModeSelect(OnModeSelect);
            uiElements.HideAllUIElements();
            UndoManager.RegisterForAnyActionDoneOrUndone(OnAnyAction);
            ShapeLoader.RegisterForOnShapeLoadEvent(OnShapeLoaded);
        }

        void OnShapeLoaded(Shape shape, LoadingContext context)
        {
            entityTextureModeButtonToggleIcon.SetActive(GetEntityTextureMode());
        }

        public void OnModeSelect(VSEditMode editMode)
        {
            foreach (var v in onlyActiveOnTextureModeInteractables)
            {
                v.interactable = editMode == VSEditMode.Texture;
            }
            if (editMode == VSEditMode.Texture)
            {
                if (objectSelector.IsAnySelected()) OnObjectSelected(objectSelector.GetCurrentlySelected());
                else OnObjectDeselcted(null);
                UVLayoutManager.main.RefreshAllUVSpaces(true);
            }
        }

        public void OnAnyAction()
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Texture) return;
            uiElements.RefreshSelectionValues();
        }

        private void OnObjectSelected(GameObject cSelected)
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Texture) return;
            uiElements.OnElementSelected(cSelected.GetComponent<ShapeElementGameObject>());
            UVLayoutManager.main.RefreshAllUVSpaces();
            uiElements.ShowAllUIElements();

        }

        private void OnObjectDeselcted(GameObject deSelected)
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Texture) return;
            uiElements.OnElementDeselected();
            uiElements.HideAllUIElements();
        }


        public void OnEntityTextureModeToggleSelect()
        {
            if (ShapeHolder.CurrentLoadedShape == null) return;
            if (GetEntityTextureMode())
            {
                TaskSetEntityTextureMode setET = new TaskSetEntityTextureMode(!GetEntityTextureMode(), false);
                setET.DoTask();
                UndoManager.main.CommitTask(setET);
            }
            else
            {
                entityTextureEnableOverlay.SetActive(true);
            }
        }

        public void ConfirmEntityTextureModeToggle(int confirmValue)
        {
            bool? t = null;
            if (confirmValue == 0) t = true;
            else if (confirmValue == 1) t = false;
            TaskSetEntityTextureMode setET = new TaskSetEntityTextureMode(true, t);
            setET.DoTask();
            UndoManager.main.CommitTask(setET);
            entityTextureEnableOverlay.SetActive(false);
        }


        public void OnEntityTextureModeChange()
        {
            entityTextureModeButtonToggleIcon.SetActive(GetEntityTextureMode());
            UVLayoutManager.main.OnEntityTextureModeChange();
            InfoLogger.main.LogText(GetEntityTextureMode() ? "Enabled entity texture mode" : "Disabled entity texture mode");
        }

        public static bool GetEntityTextureMode()
        {
            if (ShapeHolder.CurrentLoadedShape == null) return false;
            return ShapeHolder.CurrentLoadedShape.editor.entityTextureMode;
        }

        public void RandomizeUVs(int randOption)
        {
            if (ShapeHolder.CurrentLoadedShape == null) return;
            if (EditModeManager.main.cEditMode != VSEditMode.Texture) return;
            if (randOption <= 1 && ObjectSelector.main.IsAnySelected())
            {
                TaskRandomizeUVs randUVTask = new TaskRandomizeUVs(randOption, ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element);
                randUVTask.DoTask();
                UndoManager.main.CommitTask(randUVTask);
            }
            else if (randOption == 2) 
            {
                TaskRandomizeUVs randUVTask = new TaskRandomizeUVs(randOption, null);
                randUVTask.DoTask();
                UndoManager.main.CommitTask(randUVTask);
            }
        }

    }
}
