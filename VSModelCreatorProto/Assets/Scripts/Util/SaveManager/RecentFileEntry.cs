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

    public bool DoesFileStillExist()
    {
        return File.Exists(completeFilePath);
    }

    public string GetModelName()
    {
        return Path.GetFileName(completeFilePath);
    }

}
