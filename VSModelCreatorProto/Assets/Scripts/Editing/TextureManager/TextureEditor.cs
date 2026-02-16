using UnityEngine;

namespace VSMC {
    /// <summary>
    /// This is the main class for editing any texture or UV related features.
    /// Most editing functionality is done inside of <see cref="TextureEditorUIElements"/>.
    /// See <see cref="TextureManager"/> for the loading and maagement of actual texture files.
    /// </summary>
    public class TextureEditor : MonoBehaviour
    {

        [Header("Unity References")]
        public TextureEditorUIElements uiElements;
        public ObjectSelector objectSelector;

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

    }
}
