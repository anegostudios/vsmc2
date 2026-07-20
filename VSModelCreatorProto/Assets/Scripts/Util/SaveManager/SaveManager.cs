using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
    public int autosaveFrequencySeconds = 120;

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
        ShapeLoader.RegisterForOnShapeLoadEvent(OnShapeLoad);
        InvokeRepeating("TriggerAutosave", autosaveFrequencySeconds, autosaveFrequencySeconds);
        startSectionContent.SetActive(false);
        Invoke("CreateNewShapeLateStart", 0.25f);
        return;
        /* Enable to enable the home screen.
        string[] args = Environment.GetCommandLineArgs();
        foreach (string s in args)
        {
            if (s.Contains("-nointroscreen", StringComparison.CurrentCultureIgnoreCase))
            {
                startSectionContent.SetActive(false);
                ShapeLoader.main.CreateNewShape();
                return;
            }
        }
        modellingSectionContent.SetActive(false);
        */
    }

    void CreateNewShapeLateStart()
    {
        //There's a chance that at this point, a CLI arg might have been used to open a shape.
        if (ShapeHolder.CurrentLoadedShape == null)
        {
            ShapeLoader.main.CreateNewShape();
        }
    }

    void Update()
    {
        CheckInputs();
    }

    void OnShapeLoad(Shape shape, LoadingContext context)
    {
        startSectionContent.SetActive(false);
        modellingSectionContent.SetActive(true);
    }

    void TriggerAutosave()
    {
        ShapeLoader.main.BeginAutosaveThread();
    }

    public void OnModelSave(string path, bool isAutosave)
    {
        if (isAutosave) return;
        RecentFileEntry rec = new RecentFileEntry();
        rec.completeFilePath = path;
        rec.uniqueFileName = Path.GetFileNameWithoutExtension(path);
        recentFiles.AddToRecents(rec);
        SaveRecents();
    }

    public void CopyFileContentsForBackup(string path, string contents)
    {
        try //backup.
        {
            //Perform an immediate backup.
            string backupPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "backups" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path) + DateTime.Now.ToString("s").Replace(':', '-') + ".json";

            if (!Directory.Exists(Path.GetDirectoryName(backupPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
            }

            File.WriteAllText(backupPath, contents);
            Debug.Log("Written backup file to " + backupPath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not write file backup. Exception:" + e.Message);
        }

        string[] files = Directory.GetFiles(Application.persistentDataPath + Path.DirectorySeparatorChar + "backups");
        if (files.Length > 32)
        {
            //Start of array is one to remove.
            string[] ordered = files.OrderBy(f => File.GetLastWriteTime(f)).ToArray();
            File.Delete(ordered[0]);
        }
    }

    public void OpenDataFolder()
    {
        Application.OpenURL(Application.persistentDataPath);
    }

    public void OpenWikiPage()
    {
        Application.OpenURL("https://wiki.vintagestory.at/Modding:VSMC2");
    }

    public void LoadRecents()
    {
        foreach (Transform t in recentFilesContentList.transform)
        {
            Destroy(t.gameObject);
        }

        if (!File.Exists(GetRecentsPath()))
        {
            recentFiles = new RecentFiles();
            return;
        }
        recentFiles = JsonUtility.FromJson<RecentFiles>(File.ReadAllText(GetRecentsPath()));
        if (recentFiles == null || recentFiles.recents == null) recentFiles = new RecentFiles();

        //Order the recents based on last write time for the file, then by favourite. (effect is other way around)
        recentFiles.recents = recentFiles.recents.OrderBy(x => x.markedAsFavourite)
        .ThenBy(x => File.GetLastWriteTime(x.completeFilePath)).Reverse().ToArray();

        for (int i = 0; i < recentFiles.recents.Length; i++)
        {
            Instantiate(recentElemPrefab, recentFilesContentList).
            GetComponentInChildren<RecentFileUIEntry>().Initialize(recentFiles.recents[i]);
        }

    }

    public void DeleteRecent(RecentFileEntry entry)
    {
        recentFiles.recents = recentFiles.recents.Remove(entry);
        SaveRecents();
        LoadRecents();
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

    void CheckInputs()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    ShapeLoader.main.SaveShapeWithFileSelect();
                }
                else
                {
                    ShapeLoader.main.SaveShapeToStoredPath();
                }
            }
        }
    }

}
