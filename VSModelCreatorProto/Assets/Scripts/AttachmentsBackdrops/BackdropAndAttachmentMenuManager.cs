using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    public class BackdropAndAttachmentMenuManager : MonoBehaviour
    {

        public static BackdropAndAttachmentMenuManager main;

        [Header("Parent Game Objects")]
        public GameObject[] onBackdropsEnabledActive;

        [Header("List Elements")]
        public GameObject backdropListEntryPrefab;
        public Transform backdropListParent;
        public GameObject attachmentsListEntryPrefab;
        public Transform attachmentsListParent;
        public Color selectedColor;
        public Color deselectedColor;
        public Sprite activeUIImage;
        public Sprite inactiveUIImage;

        [Header("Modification Elements")]
        public GameObject backdropModificationParent;
        //Events for slider are done on slider gameobject.
        public Slider backdropOpacitySlider;
        public Toggle backdropDisableTexturesToggle;
        public GameObject backdropColorIndexParent;
        public TMP_InputField backdropColorIndexInput;
        public Button removeBackdropButton;

        public GameObject attachmentModificationParent;
        public Slider attachmentOpacitySlider;
        public Toggle attachmentDisableTexturesToggle;
        public GameObject attachmentColorIndexParent;
        public TMP_InputField attachmentColorIndexInput;
        public Button removeAttachmentButton;

        public BackdropOrAttachmentUIEntry cSelected;

        BackdropOrAttachmentUIEntry selectedOnLoadClick;

        void Awake()
        {
            main = this;
        }

        void Start()
        {
            EditModeManager.RegisterForOnModeSelect(OnEditModeSelected);
            EditModeManager.RegisterForOnModeDeselect(OnEditModeDeselected);
            ObjectSelector.main.RegisterForObjectSelectedEvent(OnObjectSelected);
            backdropDisableTexturesToggle.onValueChanged.AddListener(OnBackdropHideTexturesChanged);
            attachmentDisableTexturesToggle.onValueChanged.AddListener(OnAttachmentHideTexturesChanged);
        }

        public void RecreateBackdropList(List<LoadedBackdrop> allBackdrops)
        {
            LoadedBackdrop storedSelected = null;
            if (cSelected != null && cSelected.storedBackdrop != null)
            {
                storedSelected = cSelected.storedBackdrop;
                DeselectCurrentBackdropOrAttachment();                
            }
            foreach (Transform t in backdropListParent)
            {
                Destroy(t.gameObject);
            }

            foreach (LoadedBackdrop bd in allBackdrops)
            {
                BackdropOrAttachmentUIEntry entry = Instantiate(backdropListEntryPrefab, backdropListParent).GetComponent<BackdropOrAttachmentUIEntry>();
                entry.Initialize(bd);
                if (storedSelected == bd)
                {
                    SelectBackdropOrAttachment(entry);
                }
            }
        }

        public void RecreateAttachmentsList(List<LoadedAttachment> allAttachments)
        {
            LoadedAttachment storedSelected = null;
            if (cSelected != null && cSelected.storedAttachment != null)
            {
                storedSelected = cSelected.storedAttachment;
                DeselectCurrentBackdropOrAttachment();
            }
            foreach (Transform t in attachmentsListParent)
            {
                Destroy(t.gameObject);
            }

            foreach (LoadedAttachment ad in allAttachments)
            {
                BackdropOrAttachmentUIEntry entry = Instantiate(attachmentsListEntryPrefab, attachmentsListParent).GetComponent<BackdropOrAttachmentUIEntry>();
                entry.Initialize(ad);
                if (storedSelected == ad)
                {
                    SelectBackdropOrAttachment(entry);
                }
            }
        }

        public void SelectBackdrop(LoadedBackdrop backdrop)
        {
            foreach (Transform t in backdropListParent)
            {
                if (t.GetComponent<BackdropOrAttachmentUIEntry>().storedBackdrop == backdrop)
                {
                    SelectBackdropOrAttachment(t.GetComponent<BackdropOrAttachmentUIEntry>());
                    return;
                }
            }
            
        }

        public void SelectAttachment(LoadedAttachment attachment)
        {
            foreach (Transform t in attachmentsListParent)
            {
                if (t.GetComponent<BackdropOrAttachmentUIEntry>().storedAttachment == attachment)
                {
                    SelectBackdropOrAttachment(t.GetComponent<BackdropOrAttachmentUIEntry>());
                }
            }
        }

        public void OnRemoveBackdropButtonClicked()
        {
            if (cSelected != null && cSelected.storedBackdrop != null)
            {
                TaskRemoveBackdrop removeBackdrop = new TaskRemoveBackdrop(cSelected.storedBackdrop);
                removeBackdrop.DoTask();
                UndoManager.main.CommitTask(removeBackdrop);
            }
        }

        public void OnRemoveAttachmentButtonClicked()
        {
            if (cSelected != null && cSelected.storedAttachment != null)
            {
                TaskRemoveAttachment removeAttachment = new TaskRemoveAttachment(cSelected.storedAttachment);
                removeAttachment.DoTask();
                UndoManager.main.CommitTask(removeAttachment);
            }
        }

        public void SelectBackdropOrAttachment(BackdropOrAttachmentUIEntry selected)
        {
            if (cSelected == selected) return;
            DeselectCurrentBackdropOrAttachment();
            ObjectSelector.main.DeselectAll();
            cSelected = selected;
            selected.OnSelected();
            RefreshUIElements();
        }

        public void DeselectCurrentBackdropOrAttachment()
        {
            if (cSelected == null) return;
            cSelected.OnDeselected();
            cSelected = null;
            RefreshUIElements();
        }

        public void ToggleBackdropOrAttachmentEnabled(BackdropOrAttachmentUIEntry toToggle)
        {
            //backdrop
            if (toToggle.storedBackdrop != null)
            {
                TaskSetEnabledBackdrop enableTask = new TaskSetEnabledBackdrop(toToggle.storedBackdrop, !toToggle.storedBackdrop.data.enabled);
                enableTask.DoTask();
                UndoManager.main.CommitTask(enableTask);
            }

            //Attachment
            if (toToggle.storedAttachment != null)
            {
                //task...
                TaskSetEnabledAttachment enableAtTask = new TaskSetEnabledAttachment(toToggle.storedAttachment, !toToggle.storedAttachment.data.enabled);
                enableAtTask.DoTask();
                UndoManager.main.CommitTask(enableAtTask);
            }
        }

        public void OnObjectSelected(GameObject selected)
        {
            DeselectCurrentBackdropOrAttachment();
        }

        public void RefreshUIElements()
        {
            backdropModificationParent.SetActive(false);
            attachmentModificationParent.SetActive(false);
            if (cSelected == null)
            {
                removeAttachmentButton.interactable = false;
                removeBackdropButton.interactable = false;
            }
            else if (cSelected.storedBackdrop != null)
            {
                RefreshValuesForSelectedBackdrop();
                removeAttachmentButton.interactable = false;
            }
            else if (cSelected.storedAttachment != null)
            {
                RefreshValuesForSelectedAttachment();
                removeBackdropButton.interactable = false;
            }
        }

        public void RefreshValuesForSelectedBackdrop()
        {
            removeBackdropButton.interactable = true;
            backdropModificationParent.SetActive(true);
            backdropOpacitySlider.SetValueWithoutNotify(cSelected.storedBackdrop.data.opacity);
            backdropDisableTexturesToggle.SetIsOnWithoutNotify(cSelected.storedBackdrop.data.hideTextures);
            backdropColorIndexParent.SetActive(cSelected.storedBackdrop.data.hideTextures);
            backdropColorIndexInput.SetTextWithoutNotify(cSelected.storedBackdrop.data.flatColorIndex.ToString());
        }

        public void RefreshValuesForSelectedAttachment()
        {
            removeAttachmentButton.interactable = true;
            attachmentModificationParent.SetActive(true);
            attachmentOpacitySlider.SetValueWithoutNotify(cSelected.storedAttachment.data.opacity);
            attachmentDisableTexturesToggle.SetIsOnWithoutNotify(cSelected.storedAttachment.data.hideTextures);
            attachmentColorIndexParent.SetActive(cSelected.storedAttachment.data.hideTextures);
            attachmentColorIndexInput.SetTextWithoutNotify(cSelected.storedAttachment.data.flatColorIndex.ToString());
        }

        public void OnBackdropHideTexturesChanged(bool enabled)
        {
            TaskSetBackdropHideTexture taskSetHideTexture = new TaskSetBackdropHideTexture(cSelected.storedBackdrop, enabled);
            taskSetHideTexture.DoTask();
            UndoManager.main.CommitTask(taskSetHideTexture);
        }

        public void OnAttachmentHideTexturesChanged(bool enabled)
        {
            TaskSetAttachmentHideTexture taskSetHideTexture = new TaskSetAttachmentHideTexture(cSelected.storedAttachment, enabled);
            taskSetHideTexture.DoTask();
            UndoManager.main.CommitTask(taskSetHideTexture);
        }

        public void OnBackdropOpacityChanged(float opacity)
        {
            TaskSetBackdropOpacity setOpacityTask = new TaskSetBackdropOpacity(cSelected.storedBackdrop, cSelected.storedBackdrop.data.opacity, opacity);
            setOpacityTask.DoTask();
            UndoManager.main.CommitTask(setOpacityTask);
            UndoManager.main.MergeTopTasks();
        }

        public void OnAttachmentOpacityChanged(float opacity)
        {
            TaskSetAttachmentOpacity setOpacityTask = new TaskSetAttachmentOpacity(cSelected.storedAttachment, cSelected.storedAttachment.data.opacity, opacity);
            setOpacityTask.DoTask();
            UndoManager.main.CommitTask(setOpacityTask);
            UndoManager.main.MergeTopTasks();
        }

        public void OnBackdropColorIndexChange(string val)
        {
            if (cSelected == null || cSelected.storedBackdrop == null) return;
            if (int.TryParse(val, out int i))
            {
                if (i < 0)
                {
                    backdropColorIndexInput.text = (BackdropManager.main.colorsForUnTexturedObjects.Length - 1).ToString();
                    OnBackdropColorIndexChange(backdropColorIndexInput.text);
                    return;
                }
                else if (i >= BackdropManager.main.colorsForUnTexturedObjects.Length)
                {
                    backdropColorIndexInput.text = "0";
                    OnBackdropColorIndexChange(backdropColorIndexInput.text);
                    return;
                }
                TaskSetBackdropColorIndex taskSetColorIndex = new TaskSetBackdropColorIndex(cSelected.storedBackdrop, i);
                taskSetColorIndex.DoTask();
                UndoManager.main.CommitTask(taskSetColorIndex);
            }
        }

        public void OnAttachmentColorIndexChange(string val)
        {
            if (cSelected == null || cSelected.storedAttachment == null) return;
            if (int.TryParse(val, out int i))
            {
                if (i < 0)
                {
                    attachmentColorIndexInput.text = (BackdropManager.main.colorsForUnTexturedObjects.Length - 1).ToString();
                    OnAttachmentColorIndexChange(attachmentColorIndexInput.text);
                    return;
                }
                else if (i >= BackdropManager.main.colorsForUnTexturedObjects.Length)
                {
                    attachmentColorIndexInput.text = "0";
                    OnAttachmentColorIndexChange(attachmentColorIndexInput.text);
                    return;
                }
                TaskSetAttachmentColorIndex taskSetColorIndex = new TaskSetAttachmentColorIndex(cSelected.storedAttachment, i);
                taskSetColorIndex.DoTask();
                UndoManager.main.CommitTask(taskSetColorIndex);
            }
        }


        /// <summary>
        /// This will open the currently selected backdrop as the main file, saving the current file.
        /// The current file will be added as an attachment or backdrop depending on context. 
        /// </summary>
        public void OnOpenSelectedInContext()
        {
            if (cSelected != null)
            {
                selectedOnLoadClick = cSelected;
                SaveOverlayManager.main.OpenSaveOverlayWithFunctions(null, OpenSelectedInContext, "Open In Context",
                "This will open the selected attachment/backdrop as its own file. This will close the currently loaded file.");
            }
        }

        public void OpenSelectedInContext()
        {
            if (selectedOnLoadClick.storedAttachment != null)
            {
                LoadIntoAttachmentContext context = new LoadIntoAttachmentContext(ShapeLoader.main.storedSaveLocationForFile);
                selectedOnLoadClick.storedAttachment.LoadInContext(context);
            }
            else if (selectedOnLoadClick.storedBackdrop != null)
            {
                LoadIntoBackdropContext context = new LoadIntoBackdropContext(ShapeLoader.main.storedSaveLocationForFile);
                selectedOnLoadClick.storedBackdrop.LoadInContext(context);
            }
        }

        #region Backdrop Panel Open/Closing

        public void ToggleOpenClosePanel()
        {
            bool cOpen = !ProgramPreferences.ShowBackdropsAndAttachmentsPanel.GetValue();
            ProgramPreferences.ShowBackdropsAndAttachmentsPanel.SetValue(cOpen);
            if (cOpen)
            {
                OpenPanel();
            }
            else
            {
                ClosePanel();
            }
        }

        public void OpenPanel()
        {
            foreach (GameObject g in onBackdropsEnabledActive)
            {
                g.SetActive(true);
            }
        }
        
        public void ClosePanel()
        {
            foreach (GameObject g in onBackdropsEnabledActive)
            {
                g.SetActive(false);
            }
        }

        /// <summary>
        /// Used to open or close the edit mode manager based on the current preference setting.
        /// </summary>
        public void OnEditModeSelected(VSEditMode mode)
        {
            if (mode != VSEditMode.Model) return;
            if (ProgramPreferences.ShowBackdropsAndAttachmentsPanel.GetValue())
            {
                OpenPanel();
            }
        }

        public void OnEditModeDeselected(VSEditMode mode)
        {
            if (mode != VSEditMode.Model) return;
            ClosePanel();
            DeselectCurrentBackdropOrAttachment();
        }
        
        #endregion

    }
}