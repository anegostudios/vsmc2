using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    /// <summary>
    /// This pretty much holds all the UI element references for the texture editor. I expect there to be quite a few here...
    /// </summary>
    public class TextureEditorUIElements : MonoBehaviour
    {

        public TextureEditor textureEditor;

        [Header("UI Elements")]
        public GameObject entireTextureModeObjectGroup;
        [Tooltip("North, East, South, West, Up, Down")]
        public GameObject[] faceButtons;
        public Color unselectedFaceButtonColor;
        public Color selectedFaceButtonColor;
        public GameObject allFacesButton;
        public GameObject entireObjectGroupThatRequiresFaceSelection;
        public TMP_Dropdown textureSelectionDropdown;
        //UV input fields.
        public TMP_InputField uvX1;
        public TMP_InputField uvY1;
        public TMP_InputField uvX2;
        public TMP_InputField uvY2;
        public GameObject uvMixedWarningText;
        public Slider uvRotationSlider;
        public MixedElementToggle enabledToggle;
        public TMP_InputField elemName;


        [Header("Usage Data")]
        public bool[] cSelectedFaces;
        public ShapeElement cSelected;


        private void Start()
        {
            cSelectedFaces = new bool[6];
            RegisterUIEvents();
            OnFaceSelectionChange();
        }

        private void RegisterUIEvents()
        {
            for (int i = 0; i < faceButtons.Length; i++)
            {
                //Events go weird if you don't copy for the loop var.
                int iCopy = i;
                faceButtons[i].GetComponent<Button>().onClick.AddListener(() => { ToggleSelectFace(iCopy); });
            }
            allFacesButton.GetComponent<Button>().onClick.AddListener(() => { ToggleAllFaces(); });
            textureSelectionDropdown.onValueChanged.AddListener(x => { OnTextureSelectionChanged(x); });
            uvX1.onEndEdit.AddListener(x => { OnAnyUVChanged(x, 0); });
            uvY1.onEndEdit.AddListener(x => { OnAnyUVChanged(x, 1); });
            uvX2.onEndEdit.AddListener(x => { OnAnyUVChanged(x, 2); });
            uvY2.onEndEdit.AddListener(x => { OnAnyUVChanged(x, 3); });
            enabledToggle.toggle.onValueChanged.AddListener(x => { OnEnabledChanged(x); });
            uvRotationSlider.onValueChanged.AddListener(x => { OnRotationSliderChanged(x); });
        }

        public void OnElementSelected(ShapeElementGameObject element)
        {
            cSelected = element.element;
            elemName.SetTextWithoutNotify(element.element.Name);
            entireTextureModeObjectGroup.SetActive(true);
            OnFaceSelectionChange();
            
        }

        public void OnElementDeselected()
        {
            cSelected = null;
        }

        public void RefreshSelectionValues()
        {
            if (!ObjectSelector.main.IsAnySelected()) return;
            OnElementSelected(ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>());
        }

        public void HideAllUIElements()
        {
            UVLayoutManager.main.OnSelectedFacesChanged(cSelectedFaces);
            entireTextureModeObjectGroup.SetActive(false);
        }

        public void ShowAllUIElements()
        {
            entireTextureModeObjectGroup.SetActive(true);
        }

        public void ToggleSelectFace(int index)
        {
            cSelectedFaces[index] = !cSelectedFaces[index];
            OnFaceSelectionChange();
        }

        public void ToggleAllFaces()
        {
            bool didFindAnyOff = false;
            for (int i = 0; i < faceButtons.Length; i++)
            {
                if (!cSelectedFaces[i]) didFindAnyOff = true;
                cSelectedFaces[i] = true;
            }

            //If no faces were selected, then they were all already active, so deselect all.
            if (!didFindAnyOff)
            {
                for (int i = 0; i < faceButtons.Length; i++)
                {
                    cSelectedFaces[i] = false;
                }
            }
            OnFaceSelectionChange();
        }

        public void OnFaceSelectionChange()
        {
            bool isAnyFaceSelected = false;
            for (int i = 0; i < faceButtons.Length; i++)
            {
                if (cSelectedFaces[i]) isAnyFaceSelected = true;
                faceButtons[i].GetComponent<Outline>().enabled = cSelectedFaces[i];
                faceButtons[i].GetComponent<Image>().color = cSelectedFaces[i] ? selectedFaceButtonColor : unselectedFaceButtonColor;
            }

            UVLayoutManager.main.OnSelectedFacesChanged(cSelectedFaces);
            if (!isAnyFaceSelected)
            {
                entireObjectGroupThatRequiresFaceSelection.SetActive(false); 
                return;
            }
            entireObjectGroupThatRequiresFaceSelection.SetActive(true);
            RefreshDetailsForSelectedFaces();
        }

        /// <summary>
        /// This'll refresh the UI elements depending on what element is currently selected.
        /// </summary>
        private void RefreshDetailsForSelectedFaces()
        {
            List<ShapeElementFace> selFaces = GetSelectedFaces();

            //Texture selection... First get the textures that exist.
            List<string> loadedTextures = new List<string>();
            foreach (LoadedTexture tex in TextureManager.main.loadedTextures)
            {
                loadedTextures.Add(tex.code);
            }
            textureSelectionDropdown.ClearOptions();
            textureSelectionDropdown.AddOptions(loadedTextures);

            /*
             * This is convoluted, but is the best way of hacking the dropdown to work how I need it to.
             * Details are essentially:
             *  - I set the text of the drop down directly, which allows me to have text appear in the dropdown but not as a selectable value.
             *  - However, the dropdown *must* have a value.
             *  - If the user selected the index of the value, e.g. 0, then the callback even for the dropdown changing would not be called (since the index value has not changed).
             *  - By enabling multiselect, it allows me to have an unclamped value.
             *  - Then disable multiselect because we don't want the dropdown to actually be a multiselect list.
             *  - And voila, it works as intended.
             */
            textureSelectionDropdown.MultiSelect = true;
            textureSelectionDropdown.SetValueWithoutNotify(int.MaxValue);
            textureSelectionDropdown.MultiSelect = false;
            
            List<string> faceTextures = new List<string>();
            foreach (ShapeElementFace face in selFaces)
            {
                if (!faceTextures.Contains(face.GetReadableTextureName()))
                {
                    faceTextures.Add(face.GetReadableTextureName());
                }
            }

            if (faceTextures.Count > 1)
            {
                textureSelectionDropdown.GetComponentInChildren<TMP_Text>().text = "Mixed";
            }
            else
            {
                textureSelectionDropdown.GetComponentInChildren<TMP_Text>().text = faceTextures[0];
            }

            //UV Stuff.
            bool anyMixedUVs = false;
            float[] uvs = new float[4] { int.MinValue, int.MinValue, int.MinValue, int.MinValue };

            foreach (ShapeElementFace face in selFaces)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (uvs[i] == face.Uv[i]) { } //This is fine.
                    else if (uvs[i] < 0) uvs[i] = face.Uv[i]; //This is fine.
                    else
                    {
                        uvs[i] = int.MaxValue; //This is not fine!
                        anyMixedUVs = true;
                    }
                }
            }

            uvX1.SetTextWithoutNotify(uvs[0] == int.MaxValue ? "~" : uvs[0].ToString());
            uvY1.SetTextWithoutNotify(uvs[1] == int.MaxValue ? "~" : uvs[1].ToString());
            uvX2.SetTextWithoutNotify(uvs[2] == int.MaxValue ? "~" : uvs[2].ToString());
            uvY2.SetTextWithoutNotify(uvs[3] == int.MaxValue ? "~" : uvs[3].ToString());

            //Set the warning to appear if any UVs are mixed.
            uvMixedWarningText.gameObject.SetActive(anyMixedUVs);

            bool areAllSame = true;
            foreach (ShapeElementFace face in selFaces)
            {
                if (face.Enabled != selFaces[0].Enabled)
                {
                    areAllSame = false;
                    break;
                }
            }
            if (!areAllSame) enabledToggle.SetToggleValue(false, true);
            else enabledToggle.SetToggleValue(selFaces[0].Enabled, false);
            uvRotationSlider.SetValueWithoutNotify(Mathf.RoundToInt(selFaces[0].Rotation / 90f));

        }

        public void OnTextureSelectionChanged(int selIndex)
        {
            TaskSetFaceTexture setFaceTextureTask = new TaskSetFaceTexture(cSelected, cSelectedFaces, TextureManager.main.loadedTextures[selIndex].code);
            setFaceTextureTask.DoTask();
            UndoManager.main.CommitTask(setFaceTextureTask);
        }

        public void OnAnyUVChanged(string value, int uvIndex)
        {
            if (!float.TryParse(value, out float setVal)) return;
            TaskSetFaceUV setFaceUVTask = new TaskSetFaceUV(cSelected, cSelectedFaces, uvIndex, setVal);
            setFaceUVTask.DoTask();
            UndoManager.main.CommitTask(setFaceUVTask);
        }
        
        public void OnRotationSliderChanged(float val)
        {
            float preciseVal = ((int)val) * 90;
            TaskSetFaceUVRotation setFaceUvRot = new TaskSetFaceUVRotation(cSelected, cSelectedFaces, preciseVal);
            setFaceUvRot.DoTask();
            UndoManager.main.CommitTask(setFaceUvRot);
        }

        public void OnEnabledChanged(bool enabled)
        {
            TaskSetFaceEnabled setFaceEnabledTask = new TaskSetFaceEnabled(cSelected, cSelectedFaces, enabled);
            setFaceEnabledTask.DoTask();
            UndoManager.main.CommitTask(setFaceEnabledTask);
        }

        private List<ShapeElementFace> GetSelectedFaces()
        {
            List<ShapeElementFace> faces = new List<ShapeElementFace>();
            for (int i = 0; i < 6; i++)
            {
                if (cSelectedFaces[i]) faces.Add(GetFaceFromIndex(i));
            }
            return faces;
        }

        private ShapeElementFace GetFaceFromIndex(int selIndex)
        {
            return cSelected.FacesResolved[selIndex];
        }
    }
}