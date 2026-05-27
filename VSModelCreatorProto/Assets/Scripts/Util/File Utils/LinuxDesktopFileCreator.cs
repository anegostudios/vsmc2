using System;
using System.IO;
using UnityEngine;

public class LinuxDesktopFileCreator : MonoBehaviour
{
    string desktopFileText = @"[Desktop Entry]
Categories=Game;Graphics;3DGraphics;
Comment=Vintage Story Model Creator 2, for creating 3D models for the Vintage Story game
Exec=""{0}/VSMC2.x86_64"" %f
GenericName=Vintage Story Shape Editor
Icon={1}/VSMC2_Data/Resources/UnityPlayer.png
Name=VSMC2
Terminal=false
Type=Application
Version=1.0
StartupNotify=true";

    public GameObject desktopFileCreatorOverlay;

#if UNITY_STANDALONE_LINUX

    void Start()
    {
        //Don't allow in editor.    
        if (Application.isEditor) return;
        if (Application.version != ProgramPreferences.LastUsedVersion.GetValue() || Application.dataPath != ProgramPreferences.LastUsedLaunchPath.GetValue())
        {
            desktopFileCreatorOverlay.SetActive(true);
        }
    }

    public void CreateDesktopEntry()
    {
        string desktopFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "applications" + Path.DirectorySeparatorChar;
        if (!Directory.Exists(desktopFolderPath))
        {
            Directory.CreateDirectory(desktopFolderPath);
        }
        string dirPath = Directory.GetParent(Application.dataPath).FullName.Replace(" ", "\\s");
        string contents = string.Format(desktopFileText, dirPath, Directory.GetParent(Application.dataPath));
        File.WriteAllText(desktopFolderPath + "VSMC2.x86_64.desktop", contents);
        ProgramPreferences.LastUsedVersion.SetValue(Application.version);
        ProgramPreferences.LastUsedLaunchPath.SetValue(Application.dataPath);
    }

    public void SelectDoNotAskAgain()
    {
        ProgramPreferences.LastUsedVersion.SetValue(Application.version);
        ProgramPreferences.LastUsedLaunchPath.SetValue(Application.dataPath);
    }
#endif

}

