using UnityEngine;
using UnityEditor;
using System.IO;

public class FindNonNamespacedScripts : MonoBehaviour
{
    [MenuItem("Custom/Find Non-Namespaced Scripts")]
    public static void DoFindNonNamespacedScripts()
    {
        Debug.Log(Application.dataPath);
        foreach (string s in Directory.GetFiles(Application.dataPath + Path.DirectorySeparatorChar + "Scripts", "*.cs", SearchOption.AllDirectories))
        {
            if (!File.ReadAllText(s).Contains("namespace VSMC"))
            {
                string link = "<color=#40a0ff><link=\"href='" + s + "'\">" + s + "</link></color>";
                Debug.Log("File at " + link + " is not in VSMC namespace.");
            }
        }
    }
}
