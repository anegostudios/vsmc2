using System.IO;
using UnityEngine;

[System.Serializable]
public class RecentFileEntry
{
    public string uniqueFileName;
    public string completeFilePath;
    public bool markedAsFavourite;

    Texture2D loadedPreviewImage;

    public bool DoesFileStillExist()
    {
        return File.Exists(completeFilePath);
    }

    public string GetModelName()
    {
        return Path.GetFileName(completeFilePath);
    }

    public Texture2D LoadPreviewImage()
    {
        if (File.Exists(GetTexturePath()))
        {
            try
            {
                Texture2D loaded = new Texture2D(0, 0);
                loaded.LoadImage(File.ReadAllBytes(GetTexturePath()));
                return loaded;
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    string GetTexturePath()
    {
        return "recents/" + uniqueFileName + ".png";
    }

}
