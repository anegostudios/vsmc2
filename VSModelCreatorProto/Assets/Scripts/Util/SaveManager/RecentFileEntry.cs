using System;
using System.IO;
using UnityEditor;
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

    public Texture2D GetPreviewImage()
    {
        if (File.Exists(GetTexturePath()))
        {
            try
            {
                Texture2D loaded = new Texture2D(256, 256, TextureFormat.ARGB32, false);
                loaded.LoadImage(File.ReadAllBytes(GetTexturePath()));
                return loaded;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
        return null;
    }

    public void SavePreviewImage(Texture2D tex)
    {
        if (!File.Exists(GetTexturePath()))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GetTexturePath()));
            using (var r = File.CreateText(GetTexturePath()))
            {
                r.Write(tex.EncodeToPNG());
            } 
            return;
        }
        File.WriteAllBytes(GetTexturePath(), tex.EncodeToPNG());
    }

    string GetTexturePath()
    {
        return Application.persistentDataPath + Path.DirectorySeparatorChar + "recents" + Path.DirectorySeparatorChar + uniqueFileName + ".png";
    }

}
