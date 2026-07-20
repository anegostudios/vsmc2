using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    /// <summary>
    /// This supports loading various asset paths that will be consistent throughout the program.
    /// </summary>
    public class AssetPathManager : MonoBehaviour
    {
        public static AssetPathManager main;

        /// <summary>
        /// A list of asset paths in order of preference.
        /// </summary>
        public List<string> assetPaths;

        /// <summary>
        /// When finding an asset, should we prefer to use the local asset path?
        /// </summary>
        public bool useLocalAssetPathFirst = true;

        /// <summary>
        /// The asset path found from the currently loaded file, if valid.
        /// </summary>
        public string locallyFoundAssetPath = "";

        [Header("Unity Refs")]
        public GameObject pathManagerOverlay;
        public GameObject pathUIEntryPrefab;
        public Transform pathUIEntryParent;
        public Toggle useLocalAssetPathToggle;
        public TMP_Text currentlyFoundLocalPathText;

        AssetPathUIEntry cSelected = null;

        void Awake()
        {
            main = this;
            locallyFoundAssetPath = "";
            assetPaths = ProgramPreferences.PreferredAssetPaths.GetValue().Split(new[] { "!#!" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            useLocalAssetPathFirst = ProgramPreferences.UseLocalAssetPathFirst.GetValue();
        }

        public void OnChangeAnySettings()
        {
            ProgramPreferences.PreferredAssetPaths.SetValue(string.Join("!#!", assetPaths));
            ProgramPreferences.UseLocalAssetPathFirst.SetValue(useLocalAssetPathFirst);
            TextureManager.main.ChangeAnyBasePath();
        }

        /// <summary>
        /// Attempts to find the local asset path when a shape is loaded.
        /// </summary>
        public void OnShapeLoaded(string fullPathToShape)
        {
            string dir = Path.GetDirectoryName(fullPathToShape);
            while (dir != null && Path.GetFileName(dir) != "assets")
            {
                dir = Path.GetDirectoryName(dir);
                if (dir == null) break;
                //We're gonna check to see if the next directory contains the 'game', 'creative', and 'survival' folders.
                string[] furtherDirs = Directory.GetDirectories(dir).Select(x => Path.GetFileName(x)).ToArray();
                if (furtherDirs != null)
                {
                    if (furtherDirs.Contains("game") && furtherDirs.Contains("survival") && furtherDirs.Contains("creative"))
                    {
                        break;
                    }
                }
            }
            if (dir == null)
            {
                Debug.LogWarning("Could not find local assets path from loaded file.");
                locallyFoundAssetPath = "";
                return;
            }
            locallyFoundAssetPath = dir + Path.DirectorySeparatorChar;
            Debug.Log("Found local asset path at " + locallyFoundAssetPath);
        }

        public string FindTextureFilePath(string localPath)
        {
            return GetFullFilePathFromLocal(localPath, "textures");
        }

        public string FindBackdropOrAttachmentShapeFilePath(string localPath)
        {
            return GetFullFilePathFromLocal(localPath, "shapes");
        }

        public string GetFullFilePathFromLocal(string localPath, string folderNameToSearchIn)
        {
            if (Path.IsPathRooted(localPath)) return localPath;
            if (useLocalAssetPathFirst && locallyFoundAssetPath != "")
            {
                Debug.Log("Intermittent Step: " + locallyFoundAssetPath);
                foreach (string dirs in GetDefaultSearchOrder(Directory.GetDirectories(locallyFoundAssetPath, folderNameToSearchIn, SearchOption.AllDirectories)))
                {
                    Debug.Log(dirs);
                    if (File.Exists(Path.Combine(dirs, localPath)))
                    {
                        Debug.Log("Accessing file from " + Path.Combine(dirs, localPath));
                        return Path.Combine(dirs, localPath);
                    }
                }
            }
            foreach (string basePath in assetPaths)
            {
                foreach (string dirs in GetDefaultSearchOrder(Directory.GetDirectories(basePath, folderNameToSearchIn, SearchOption.AllDirectories)))
                {
                    Debug.Log(dirs);
                    if (File.Exists(Path.Combine(dirs, localPath)))
                    {
                        Debug.Log("Accessing file from " + Path.Combine(dirs, localPath));
                        return Path.Combine(dirs, localPath);
                    }
                }
            }
            if (!useLocalAssetPathFirst && locallyFoundAssetPath != "")
            {
                foreach (string dirs in GetDefaultSearchOrder(Directory.GetDirectories(locallyFoundAssetPath, folderNameToSearchIn, SearchOption.AllDirectories)))
                {
                    Debug.Log(dirs);
                    if (File.Exists(Path.Combine(dirs, localPath)))
                    {
                        Debug.Log("Accessing file from " + Path.Combine(dirs, localPath));
                        return Path.Combine(dirs, localPath);
                    }
                }
            }
            Debug.Log("Cannot find local file at " + localPath);
            return "";
        }

        /// <summary>
        /// When given a list of directories, this will preference custom domains, then survival, then creative, and then game.
        /// </summary>
        /// <param name="dirs"></param>
        /// <returns></returns>
        public IEnumerable<string> GetDefaultSearchOrder(IEnumerable<string> dirs)
        {
            return dirs.OrderBy(x => x.Contains(Path.DirectorySeparatorChar + "game" + Path.DirectorySeparatorChar)).
                         ThenBy(x => x.Contains(Path.DirectorySeparatorChar + "creative" + Path.DirectorySeparatorChar)).
                         ThenBy(x => x.Contains(Path.DirectorySeparatorChar + "survival" + Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Converts a complete file path into one local to the topmost base asset path.
        /// Returns complete file path if cannot find a local one.
        /// </summary>
        public string GetRelativePathForFile(string completeFilePath, string localFolderPrefix)
        {
            if (useLocalAssetPathFirst && locallyFoundAssetPath != "")
            {
                //Reversed the list to get survival folder first in a really hacky way.
                foreach (string dirs in GetDefaultSearchOrder(Directory.GetDirectories(locallyFoundAssetPath, localFolderPrefix, SearchOption.AllDirectories).Reverse()))
                {
                    string rel = Path.GetRelativePath(dirs, completeFilePath);
                    if (Path.GetFullPath(rel) == Path.GetFullPath(completeFilePath) || rel.StartsWith(".") || rel.StartsWith("..")) continue;
                    return rel;                    
                }
            }
            foreach (string basePath in assetPaths)
            {
                foreach (string dirs in GetDefaultSearchOrder(Directory.GetDirectories(basePath, localFolderPrefix, SearchOption.AllDirectories)))
                {
                    string rel = Path.GetRelativePath(dirs, completeFilePath);
                    if (Path.GetFullPath(rel) == Path.GetFullPath(completeFilePath) || rel.StartsWith(".") || rel.StartsWith("..")) continue;
                    return rel;
                }
            }
            if (!useLocalAssetPathFirst && locallyFoundAssetPath != "")
            {
                //Reversed the list to get survival folder first in a really hacky way.
                foreach (string dirs in GetDefaultSearchOrder(Directory.GetDirectories(locallyFoundAssetPath, localFolderPrefix, SearchOption.AllDirectories)))
                {
                    string rel = Path.GetRelativePath(dirs, completeFilePath);
                    if (Path.GetFullPath(rel) == Path.GetFullPath(completeFilePath) || rel.StartsWith(".") || rel.StartsWith("..")) continue;
                    return rel;
                }
            }
            Debug.Log("Cannot find local path to file at " + completeFilePath);
            return completeFilePath;
        }

        public string GetFirstPreferredAssetPath()
        {
            if ((useLocalAssetPathFirst && locallyFoundAssetPath != "") || assetPaths.Count == 0)
            {
                return locallyFoundAssetPath;
            }
            else
            {
                return assetPaths[0];
            }
            
        }

        public void OpenOverlay()
        {
            string cSel = "";
            if (cSelected) cSel = cSelected.path.text;
            cSelected = null;
            foreach (Transform t in pathUIEntryParent)
            {
                Destroy(t.gameObject);
            }
            int i = 1;
            foreach (string s in assetPaths)
            {
                AssetPathUIEntry entry = Instantiate(pathUIEntryPrefab, pathUIEntryParent).GetComponent<AssetPathUIEntry>();
                entry.Initialize(s, this, i++);
                if (cSel == s)
                {
                    SelectAssetPath(entry);
                }
            }

            if (locallyFoundAssetPath == null || locallyFoundAssetPath == "")
            {
                currentlyFoundLocalPathText.text = "Could not find local path";
            }
            else
            {
                currentlyFoundLocalPathText.text = locallyFoundAssetPath;
            }
            useLocalAssetPathToggle.SetIsOnWithoutNotify(useLocalAssetPathFirst);
            pathManagerOverlay.SetActive(true);
        }

        public void AddAssetPath()
        {
            try
            {
                string[] folders = StandaloneFileBrowser.OpenFolderPanel("Select base asset path", "", true);
                if (folders != null)
                {
                    foreach (string s in folders)
                    {
                        if (!assetPaths.Contains(s)) assetPaths.Add(s);
                    }
                    OnChangeAnySettings();
                    OpenOverlay();
                }
            }
            catch { }
        }

        public void RemoveAssetPath()
        {
            if (cSelected == null) return;
            assetPaths.Remove(cSelected.path.text);
            OnChangeAnySettings();
            OpenOverlay();
        }

        public void MoveUpPriority()
        {
            if (cSelected == null) return;
            int index = assetPaths.IndexOf(cSelected.path.text);
            if (index == 0) return;
            assetPaths.Remove(cSelected.path.text);
            assetPaths.Insert(index - 1, cSelected.path.text);
            OnChangeAnySettings();
            OpenOverlay();
        }
        
        public void MoveDownPriority()
        {
            if (cSelected == null) return;
            int index = assetPaths.IndexOf(cSelected.path.text);
            if (index >= assetPaths.Count - 1) return;
            assetPaths.Remove(cSelected.path.text);
            assetPaths.Insert(index + 1, cSelected.path.text);
            OnChangeAnySettings();
            OpenOverlay();
        }

        public void SelectAssetPath(AssetPathUIEntry uiEntry)
        {
            DeselectAssetPath();
            uiEntry.selectedHighlight.enabled = true;
            cSelected = uiEntry;
        }

        public void DeselectAssetPath()
        {
            if (cSelected != null)
            {
                cSelected.selectedHighlight.enabled = false;
            }
            cSelected = null;
        }

        public void ToggleUseLocalAssets(bool toggle)
        {
            useLocalAssetPathFirst = toggle;
            OnChangeAnySettings();
        }
            
    }
}