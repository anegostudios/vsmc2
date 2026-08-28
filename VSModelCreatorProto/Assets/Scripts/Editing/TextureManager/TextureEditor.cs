using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UI;

namespace VSMC {
    /// <summary>
    /// This is the main class for editing any texture or UV related features.
    /// Most editing functionality is done inside of <see cref="TextureEditorUIElements"/>.
    /// See <see cref="TextureManager"/> for the loading and maagement of actual texture files.
    /// </summary>
    public class TextureEditor : MonoBehaviour
    {

        public struct ElementTextureDataForTasks
        {
            public bool autoUnwrap;
            public double[] entityTextureUV;
            public int entityTextureUnwrapMode;
            public int entityTextureUnwrapRotationIndex;
            public float[][] uvs;
            public float[] rotations;
            public bool[] autoResolutions;
            public bool[] snapUVs;

            public ElementTextureDataForTasks(ShapeElement e)
            {
                autoUnwrap = e.autoUnwrap;
                entityTextureUV = (double[])e.entityTextureUV.Clone();
                entityTextureUnwrapMode = e.entityTextureUnwrapMode;
                entityTextureUnwrapRotationIndex = e.entityTextureUnwrapRotationIndex;

                uvs = new float[6][];
                rotations = new float[6];
                autoResolutions = new bool[6];
                snapUVs = new bool[6];

                for (int i = 0; i < 6; i++)
                {
                    ShapeElementFace f = e.FacesResolved[i];
                    uvs[i] = (float[])f.Uv.Clone();
                    rotations[i] = f.Rotation;
                    autoResolutions[i] = f.autoResolutionForUV;
                    snapUVs[i] = f.snapUV;
                }
            }

            public void ApplyTo(ShapeElement e)
            {
                e.autoUnwrap = autoUnwrap;
                e.entityTextureUV = (double[])entityTextureUV.Clone();
                e.entityTextureUnwrapMode = entityTextureUnwrapMode;
                e.entityTextureUnwrapRotationIndex = entityTextureUnwrapRotationIndex;

                for (int i = 0; i < 6; i++)
                {
                    ShapeElementFace f = e.FacesResolved[i];
                    f.Uv = (float[])uvs[i].Clone();
                    f.Rotation = rotations[i];
                    f.autoResolutionForUV = autoResolutions[i];
                    f.snapUV = snapUVs[i];
                }
            }
        }

        public static TextureEditor main;

        [Header("Unity References")]
        public TextureEditorUIElements uiElements;
        public ObjectSelector objectSelector;

        [Header("Entity Texturing")]
        public GameObject entityTextureModeButtonToggleIcon;
        public GameObject entityTextureEnableOverlay;
        public GameObject roundUVsButtonToggleIcon;

        [Header("Auto Unwrap UV")]
        public GameObject autoUnwrapOverlay;
        public TMP_Dropdown autoUnwrapUnwrapMode;
        public TMP_InputField autoUnwrapMaxDimX;
        public TMP_InputField autoUnwrapMaxDimY;
        public TMP_InputField autoUnwrapPadding;
        public TMP_InputField autoUnwrapTotalAttempts;
        public TMP_Text autoUnwrapSuccess;
        public string autoUnwrapSuccessString;
        public GameObject autoUnwrapFailure;

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
            roundUVsButtonToggleIcon.SetActive(shape.editor.roundAutoUVs);
        }

        public void OnModeSelect(VSEditMode editMode)
        {
            foreach (var v in onlyActiveOnTextureModeInteractables)
            {
                v.interactable = editMode == VSEditMode.Texture;
            }
            if (editMode == VSEditMode.Texture)
            {
                UVLayoutManager.main.RefreshAllUVSpaces(true);
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

        public void ToggleRoundingForAutoUVs()
        {
            TaskSetRoundUVMode roundUVTask = new TaskSetRoundUVMode(!ShapeHolder.CurrentLoadedShape.editor.roundAutoUVs);
            roundUVTask.DoTask();
            UndoManager.main.CommitTask(roundUVTask);
            roundUVsButtonToggleIcon.SetActive(ShapeHolder.CurrentLoadedShape.editor.roundAutoUVs);
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

        public void OpenAutoUnwrapOverlay()
        {
            if (ShapeHolder.CurrentLoadedShape == null) return;
            if (EditModeManager.main.cEditMode != VSEditMode.Texture) return;
            LoadedTexture t = TextureManager.main.loadedTextures.Count > 0 ? TextureManager.main.loadedTextures[0] : TextureManager.main.emptyTexture;
            autoUnwrapMaxDimX.text = t.storedWidth.ToString();
            autoUnwrapMaxDimY.text = t.storedHeight.ToString();
            autoUnwrapSuccess.gameObject.SetActive(false);
            autoUnwrapFailure.SetActive(false);
            autoUnwrapOverlay.SetActive(true);
        }

        public void AttemptToUnwrapAllUVs(int option)
        {
            if (ShapeHolder.CurrentLoadedShape == null) return;
            if (EditModeManager.main.cEditMode != VSEditMode.Texture) return;
            int w = int.Parse(autoUnwrapMaxDimX.text);
            int h = int.Parse(autoUnwrapMaxDimY.text);
            int unwrapMode = autoUnwrapUnwrapMode.value;
            int padding = int.Parse(autoUnwrapPadding.text);
            int attempts = int.Parse(autoUnwrapTotalAttempts.text);

            if (ObjectSelector.main.IsAnySelected() && option == 0)
            {
                TaskAutoUVElements autoUVTask = new TaskAutoUVElements(ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element, unwrapMode, w, h, padding, attempts);
                autoUVTask.DoTask();

                if (autoUVTask.mostRecentDidSucceed)
                {
                    UndoManager.main.CommitTask(autoUVTask);
                    autoUnwrapSuccess.gameObject.SetActive(true);
                    autoUnwrapSuccess.text = String.Format(autoUnwrapSuccessString, autoUVTask.successUVDimensions.x, autoUVTask.successUVDimensions.y);
                    autoUnwrapFailure.SetActive(false);
                }
                else
                {
                    autoUnwrapSuccess.gameObject.SetActive(false);
                    autoUnwrapFailure.SetActive(true);
                }
            }
            else if (option == 1)
            {
                TaskAutoUVElements autoUVTask = new TaskAutoUVElements(null, unwrapMode, w, h, padding, attempts);
                autoUVTask.DoTask();
                if (autoUVTask.mostRecentDidSucceed)
                {
                    UndoManager.main.CommitTask(autoUVTask);
                    autoUnwrapSuccess.gameObject.SetActive(true);
                    autoUnwrapSuccess.text = String.Format(autoUnwrapSuccessString, autoUVTask.successUVDimensions.x, autoUVTask.successUVDimensions.y);
                    autoUnwrapFailure.SetActive(false);
                }
                else
                {
                    autoUnwrapSuccess.gameObject.SetActive(false);
                    autoUnwrapFailure.SetActive(true);
                }
            }
        }

        public void ExportUVMap()
        {
            UVMapExporter.CalculateAndExportUVMap();
        }

    }
}
