using TMPro;
using UnityEngine;

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
        public TMP_Text entityTextureModeButtonText;
        public GameObject entityTextureEnableOverlay;

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
        }

        public void OnModeSelect(VSEditMode editMode)
        {
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
            entityTextureModeButtonText.text = GetEntityTextureMode() ? "Disable Entity Texture Mode" : "Enable Entity Texture Mode";
            UVLayoutManager.main.OnEntityTextureModeChange();
        }

        public static bool GetEntityTextureMode()
        {
            if (ShapeHolder.CurrentLoadedShape == null) return false;
            return ShapeHolder.CurrentLoadedShape.editor.entityTextureMode;
        }

    }
}
