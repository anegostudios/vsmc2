using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// This will manage all attachments on the current shape.
    /// Any number of attachments can be loaded.
    /// Attachments are nice because they don't actually interfere with the currently loaded shape.
    /// </summary>
    public class AttachmentManager : MonoBehaviour
    {

        public static AttachmentManager main;

        public GameObject attachmentHolderPrefab;
        public List<LoadedAdditionalShape> allAttachments;

        void Awake()
        {
            main = this;
        }

        void Start()
        {
            ShapeLoader.RegisterForOnShapeLoadEvent(LoadAttachmentsFromShapeData);
            ShapeLoader.RegisterForOnShapeSaveEvent(AddAttachmentDataIntoShape);
        }

        /// <summary>
        /// Called when a shape is loaded. Loads the attachment data from the editor values.
        /// </summary>
        void LoadAttachmentsFromShapeData(Shape shape, LoadingContext context)
        {
            allAttachments = new List<LoadedAdditionalShape>();
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }

            if (shape?.editor?.attachments != null)
            {
                foreach (AdditionalShapeData data in shape.editor.attachments)
                {
                    LoadedAdditionalShape a = CreateAndInitializeAttachmentFromData(data);
                    a.SetAttachmentEnabled(a.data.enabled);
                }
            }

            if (context is LoadIntoBackdropContext bdContext)
            {
                if (bdContext.fullPathToShapeLoadedFrom == "" || bdContext.fullPathToShapeLoadedFrom == null)
                {
                    Debug.Log("Trying to open in context, but the original shape has no filepath.");
                }
                else
                {
                    //Check if attachment is loaded.
                    string localPath = AssetPathManager.main.GetRelativePathForFile(bdContext.fullPathToShapeLoadedFrom, "shapes").Replace(".json", "");
                    LoadedAdditionalShape att = GetAttachmentFromPath(localPath);
                    if (att != null)
                    {
                        SetEnabledAttachment(att, true);
                    }
                    else
                    {
                        SetEnabledAttachment(CreateNewAttachment(localPath), true);                    
                    }
                }
            }
            BackdropAndAttachmentMenuManager.main.RecreateAttachmentsList(allAttachments);
        }

        /// <summary>
        /// Creates an attachment gameobject from the loaded or created data. 
        /// This will not enable the object.
        /// </summary>
        public LoadedAdditionalShape CreateAndInitializeAttachmentFromData(AdditionalShapeData data, int specificIndex = -1)
        {
            LoadedAdditionalShape attachment = new LoadedAdditionalShape(data,
            Instantiate(attachmentHolderPrefab, transform).GetComponentInChildren<AdditionalObjectHolder>());
            if (specificIndex == -1)
            {
                allAttachments.Add(attachment);
            }
            else
            {
                allAttachments.Insert(specificIndex, attachment);
            }
            SetEnabledAttachment(attachment, data.enabled);
            BackdropAndAttachmentMenuManager.main.RecreateAttachmentsList(allAttachments);
            return attachment;
        }

        /// <summary>
        /// Creates an attachment from a shape filepath.
        /// </summary>
        public LoadedAdditionalShape CreateNewAttachment(string fromPath)
        {
            if (GetAttachmentFromPath(fromPath) != null)
            {
                Debug.Log("Cannot create attachment from this shape as it already exists.");
                return null;
            }
            AdditionalShapeData data = new AdditionalShapeData(fromPath);
            return CreateAndInitializeAttachmentFromData(data);
        }

        public void RemoveAttachment(LoadedAdditionalShape attachment)
        {
            attachment.OnAttachmentDisabled();
            allAttachments.Remove(attachment);
            Destroy(attachment.attachmentHolder.gameObject);
            BackdropAndAttachmentMenuManager.main.RecreateAttachmentsList(allAttachments);
        }

        public void OnAddAttachmentButtonClicked()
        {
            try
            {
                string[] selectedFiles = StandaloneFileBrowser.OpenFilePanel("Select Attachment Shape Files", AssetPathManager.main.GetFirstPreferredAssetPath(), "json", true);
                if (selectedFiles.Length < 1 || selectedFiles[0] == "") return;
                foreach (string s in selectedFiles)
                {
                    string s2 = s;
                    while (char.IsControl(s2[0]) && s2.Length > 1)
                    {
                        s2 = s2.Substring(1);
                    }
                    string sRel = AssetPathManager.main.GetRelativePathForFile(s2, "shapes").Replace(".json", "");
                    if (GetAttachmentFromPath(sRel) != null) continue; //Cannot duplicate attachments.
                    TaskCreateNewAttachment addAttachment = new TaskCreateNewAttachment(sRel);
                    addAttachment.DoTask();
                    UndoManager.main.CommitTask(addAttachment);
                    SetEnabledAttachment(allAttachments[allAttachments.Count - 1], true);
                }
                //BackdropAndAttachmentMenuManager.main.SelectAttachment(allAttachments[allAttachments.Count - 1]);
            }
            catch { return; }
        }

        public LoadedAdditionalShape GetAttachmentFromPath(string path)
        {
            return allAttachments.FirstOrDefault(x => x.data.shapeFilepath == path);
        }

        public void AddAttachmentFromFileDrop(string path)
        {
            string sRel = AssetPathManager.main.GetRelativePathForFile(path, "shapes").Replace(".json", "");
            if (GetAttachmentFromPath(sRel) != null) return; //Cannot duplicate attachments.
            TaskCreateNewAttachment addAttachment = new TaskCreateNewAttachment(sRel);
            addAttachment.DoTask();
            UndoManager.main.CommitTask(addAttachment);
        }


        public void SetEnabledAttachment(string attachment, bool enabled)
        {
            SetEnabledAttachment(GetAttachmentFromPath(attachment), enabled);
        }

        public void SetEnabledAttachment(LoadedAdditionalShape attachment, bool enabled)
        {
            attachment.data.enabled = enabled;
            if (enabled) attachment.OnAttachmentEnabled();
            else attachment.OnAttachmentDisabled();
            BackdropAndAttachmentMenuManager.main.RecreateAttachmentsList(allAttachments);
        }

        void AddAttachmentDataIntoShape(Shape shape)
        {
            List<AdditionalShapeData> attachmentData = new List<AdditionalShapeData>();
            foreach (LoadedAdditionalShape attachment in allAttachments)
            {
                attachment.data.ResolveBeforeSerialization(shape, attachment.loadedAdditionalShape);
                attachmentData.Add(attachment.data);
            }
            shape.editor.attachments = attachmentData.ToArray();
        }

        public IEnumerable<LoadedAdditionalShape> GetEnabledAttachments()
        {
            return allAttachments.Where(x => x.attachmentHolder.gameObject.activeSelf);
        }

    }
}