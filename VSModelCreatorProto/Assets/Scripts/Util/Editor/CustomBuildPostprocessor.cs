using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class CustomBuildPostprocessor
{
    [PostProcessBuild(0)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        foreach (string dir in Directory.GetDirectories(Path.GetDirectoryName(pathToBuiltProject)))
        {
            if (dir.EndsWith("DoNotShip"))
            {
                Directory.Delete(dir, true);
                Debug.Log("Deleted " + dir + " after build.");
            }
        }
    }
}
