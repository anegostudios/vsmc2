using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VSMC;

namespace VSMC
{
    public class TextureManagerOverlay : MonoBehaviour
    {
        [Header("Unity References")]
        public GameObject selectableTexturePrefab;
        public Transform selectableTexturesHolder;
        public TMP_Text baseTexturePathText;
        public TMP_InputField textureCode;
        public TMP_InputField texturePath;
        public TMP_InputField textureWidth;
        public TMP_InputField textureHeight;

        [NonSerialized]
        public GameObject[] textureImages;
        [NonSerialized]
        public int cSelectedTextureIndex = -1;

        /// <summary>
        /// Shorthand to get the texture manager.
        /// </summary>
        public TextureManager TexMan
        {
            get
            {
                return TextureManager.main;
            }
        }

        private void Start()
        {
            cSelectedTextureIndex = -1;
        }

        public void Open()
        {
            //Remove current loaded textures.
            foreach (Transform child in selectableTexturesHolder)
            {
                Destroy(child.gameObject);
            }

            //Load new textures...
            textureImages = new GameObject[TexMan.loadedTextures.Count];
            for (int i = 0; i < TexMan.loadedTextures.Count; i++)
            {
                LoadedTexture tex = TexMan.loadedTextures[i];
                GameObject texUI = Instantiate(selectableTexturePrefab, selectableTexturesHolder);
                texUI.GetComponentInChildren<RawImage>().texture = tex.loadedTexture;
                texUI.GetComponentInChildren<TMP_Text>().gameObject.SetActive(tex.error != LoadedTexture.LoadedTextureError.Valid);
                texUI.GetComponentInChildren<Outline>(true).enabled = cSelectedTextureIndex == i;
                //We use the tex object name to store its index for selection.
                texUI.name = i.ToString();
                textureImages[i] = texUI;
                texUI.GetComponent<Button>().onClick.AddListener(() => OnTextureSelected(texUI));
            }

            //Set texture base path.
            if (TextureManager.main.textureBasePath.Length < 3)
            {
                baseTexturePathText.text = "Please select a base texture path.";
            }
            else
            {
                baseTexturePathText.text = TextureManager.main.textureBasePath;
            }
            
            gameObject.SetActive(true);

            if (cSelectedTextureIndex >= 0 && cSelectedTextureIndex < textureImages.Length)
            {
                OnTextureSelected(textureImages[cSelectedTextureIndex]);
            }

        }

        public void OnChangeTextureBasePathButton()
        {
            string[] selectedFolder = SFB.StandaloneFileBrowser.OpenFolderPanel("Select 'Textures' Folder", "", false);
            if (selectedFolder == null || selectedFolder.Length == 0) return;
            TaskChangeTextureBasePath changeBasePathTask = new TaskChangeTextureBasePath(selectedFolder[0]);
            changeBasePathTask.DoTask();
            UndoManager.main.CommitTask(changeBasePathTask);
        }

        public void OnTextureSelected(GameObject texObj)
        {
            if (!gameObject.activeSelf) return;
            //We use the tex object name to store its index, this is fine.
            int selID = int.Parse(texObj.name);
            LoadedTexture tex = TextureManager.main.loadedTextures[selID];

            //Deselect current.
            if (cSelectedTextureIndex != -1 && cSelectedTextureIndex < textureImages.Length) textureImages[cSelectedTextureIndex].GetComponentInChildren<Outline>(true).enabled = false;
            
            //Now select the next texture.
            cSelectedTextureIndex = selID;
            textureImages[selID].GetComponentInChildren<Outline>(true).enabled = true;
            textureCode.SetTextWithoutNotify(tex.code);
            texturePath.SetTextWithoutNotify(tex.path);
            textureWidth.SetTextWithoutNotify(tex.storedWidth.ToString());
            textureHeight.SetTextWithoutNotify(tex.storedHeight.ToString());
        }

        public void ApplyTextureCode()
        {
            if (cSelectedTextureIndex == -1) return;
            //Check new code validity.
            bool pass = true;
            if (textureCode.text.Length < 3) pass = false;
            else
            {
                foreach (LoadedTexture loadedTex in TextureManager.main.loadedTextures)
                {
                    if (loadedTex.code == textureCode.text) pass = false;
                }
            }
            if (!pass)
            {
                textureCode.text = TextureManager.main.loadedTextures[cSelectedTextureIndex].code;
                return;
            }
            TaskChangeTextureCode changeTexCodeTask = new TaskChangeTextureCode(cSelectedTextureIndex, textureCode.text);
            changeTexCodeTask.DoTask();
            UndoManager.main.CommitTask(changeTexCodeTask);
        }

        public void ApplyTexturePath()
        {
            if (cSelectedTextureIndex == -1) return;
            TaskChangeTexturePath changeTexPathTask = new TaskChangeTexturePath(cSelectedTextureIndex, texturePath.text);
            changeTexPathTask.DoTask();
            UndoManager.main.CommitTask(changeTexPathTask);
        }

        public void AddNewTexture()
        {
            TaskCreateNewTexture newTexTask = new TaskCreateNewTexture();
            newTexTask.DoTask();
            UndoManager.main.CommitTask(newTexTask);
        }

        public void DeleteTexture()
        {
            if (cSelectedTextureIndex == -1) return;
            TaskDeleteTexture delTexTask = new TaskDeleteTexture(TextureManager.main.loadedTextures[cSelectedTextureIndex]);
            cSelectedTextureIndex = -1;
            delTexTask.DoTask();
            UndoManager.main.CommitTask(delTexTask);
            
        }

        public void RefreshIfOpen()
        {
            if (gameObject.activeSelf) Open();
        }
    }
}   