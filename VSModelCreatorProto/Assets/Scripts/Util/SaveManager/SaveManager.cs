using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class SaveManager : MonoBehaviour
{

    public static SaveManager main;
    public GameObject recentElemPrefab;
    public Transform recentFilesContentList;
    public GameObject startSectionContent;
    public GameObject modellingSectionContent;

    RenderTexture storedRT;
    RecentFileEntry storedrec;
    public Texture2D mostRecentTex;

    [System.Serializable]
    public class RecentFiles
    {
        public RecentFileEntry[] recents;

        public RecentFiles()
        {
            recents = new RecentFileEntry[0];
        }

        public void AddToRecents(RecentFileEntry recent)
        {
            List<RecentFileEntry> toRemove = new List<RecentFileEntry>();
            foreach (RecentFileEntry e in recents)
            {
                //First, remove all recents that are the same filepath as the one we are adding.
                if (Path.GetFullPath(e.completeFilePath).Equals(Path.GetFullPath(recent.completeFilePath), StringComparison.CurrentCultureIgnoreCase))
                {
                    toRemove.Add(e);
                    if (e.markedAsFavourite) recent.markedAsFavourite = true;
                }
                else
                {
                    if (e.uniqueFileName.Equals(recent.uniqueFileName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        recent.uniqueFileName += "_1";
                    }
                }
            }
            foreach (RecentFileEntry e in toRemove) 
            {
                recents = recents.Remove(e);
            }
            recents = recents.Append(recent);
        }

    }

    public RecentFiles recentFiles;

    void Awake()
    {
        main = this;
        LoadRecents();
        startSectionContent.SetActive(true);
        modellingSectionContent.SetActive(true);
    }

    void Start()
    {
        startSectionContent.SetActive(true);
        modellingSectionContent.SetActive(false);
    }

    public void OnModelSave(string path)
    {
        RecentFileEntry rec = new RecentFileEntry();
        rec.completeFilePath = path;
        rec.uniqueFileName = Path.GetFileNameWithoutExtension(path);
        Camera scene = Camera.main;
        storedRT = scene.targetTexture;
        RenderTexture nt = new RenderTexture(256, 256, 32, RenderTextureFormat.ARGB32, 0);
        scene.targetTexture = nt;
        scene.Render();
        recentFiles.AddToRecents(rec);
        storedrec = rec;
        StartCoroutine("SaveModelScreenshot");
        SaveRecents();
    }


    IEnumerator SaveModelScreenshot()
    {
        //I am 90% sure that most of this is unnecessary, but oh well. It works.
        yield return new WaitForEndOfFrame();
        RenderTexture nt = Camera.main.targetTexture;
        Camera.main.targetTexture = storedRT;
        storedRT = null;
        mostRecentTex = new Texture2D(nt.width, nt.height, TextureFormat.ARGB32, false);
        Graphics.CopyTexture(nt, mostRecentTex);
        RenderTexture.active = nt;
        mostRecentTex.ReadPixels(new Rect(0, 0, nt.width, nt.height), 0, 0);
        RenderTexture.active = Camera.main.targetTexture;
        storedrec.SavePreviewImage(mostRecentTex);
        Debug.Log("Saved post render.");
        yield return null;
    }

    public void LoadRecents()
    {
        if (!File.Exists(GetRecentsPath()))
        {
            recentFiles = new RecentFiles();
            return;
        }
        recentFiles = JsonUtility.FromJson<RecentFiles>(File.ReadAllText(GetRecentsPath()));
        if (recentFiles == null || recentFiles.recents == null) recentFiles = new RecentFiles();

        //Order the recents based on last write time for the file, then by favourite. (effect is other way around)
        recentFiles.recents = recentFiles.recents.OrderBy(x => File.GetLastWriteTime(x.completeFilePath))
        .OrderBy(x => x.markedAsFavourite).Reverse().ToArray();

        for (int i = 0; i < recentFiles.recents.Length; i++)
        {
            Instantiate(recentElemPrefab, recentFilesContentList).
            GetComponentInChildren<RecentFileUIEntry>().Initialize(recentFiles.recents[i]);
        }

    }

    public void BeforeFileLoad()
    {
        startSectionContent.SetActive(false);
        modellingSectionContent.SetActive(true);
    }

    public void SaveRecents()
    {
        if (!File.Exists(GetRecentsPath()))
        {
            using (var r = File.CreateText(GetRecentsPath()))
            {
                r.Write(JsonUtility.ToJson(recentFiles));
            }
            return;
        }
        File.WriteAllText(GetRecentsPath(), JsonUtility.ToJson(recentFiles));
    }

    string GetRecentsPath()
    {
        return Application.persistentDataPath + Path.DirectorySeparatorChar + "recents.json";
    }

}
