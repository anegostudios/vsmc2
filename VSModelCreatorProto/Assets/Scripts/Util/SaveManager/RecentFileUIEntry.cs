using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class RecentFileUIEntry : MonoBehaviour
{

    public TMP_Text fileName;
    public TMP_Text fileLastWrite;
    public Image fileFavourite;

    RecentFileEntry entry;

    public void Initialize(RecentFileEntry entry)
    {
        this.entry = entry;
        fileName.text = entry.GetModelName();
        fileLastWrite.text = "Last Saved: " + File.GetCreationTime(entry.completeFilePath).ToString("g");
        Invoke("ResolveFavouriteButton", 0);
    }

    public void MarkAsFavouriteButton()
    {
        entry.markedAsFavourite = !entry.markedAsFavourite;
        SaveManager.main.SaveRecents();
        ResolveFavouriteButton();
    }

    public void OnRemoveButton()
    {
        SaveManager.main.DeleteRecent(entry);
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

    public void OpenRecent()
    {
        ShapeLoader.main.LoadShape(entry.completeFilePath);
    }

}
