using System;
using System.IO;
using System.Windows.Forms.VisualStyles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecentFileUIEntry : MonoBehaviour
{

    public TMP_Text fileName;
    public TMP_Text fileLastWrite;
    public Image fileFavourite;
    public RawImage filePreview;

    RecentFileEntry entry;

    public void Initialize(RecentFileEntry entry)
    {
        this.entry = entry;
        fileName.text = entry.GetModelName();
        fileLastWrite.text = "Last Saved: " + File.GetCreationTime(entry.completeFilePath).ToString("g");
        filePreview.texture = entry.GetPreviewImage();
        if (filePreview.texture == null) filePreview.enabled = false;
        Invoke("ResolveFavouriteButton", 0);
    }

    public void MarkAsFavouriteButton()
    {
        entry.markedAsFavourite = !entry.markedAsFavourite;
        SaveManager.main.SaveRecents();
        ResolveFavouriteButton();
    }

    void ResolveFavouriteButton()
    {
        if (entry.markedAsFavourite)
        {
            fileFavourite.CrossFadeColor(Color.red, 0.2f, false, false);
        }
        else
        {
            fileFavourite.CrossFadeColor(Color.white, 0.2f, false, false);
        }
    }

}
