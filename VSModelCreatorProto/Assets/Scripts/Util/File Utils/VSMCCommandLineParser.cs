using System;
using System.IO;
using UnityEngine;
using VSMC;

public class VSMCCommandLineArgsParser : MonoBehaviour
{
    void Start()
    {
        Invoke("AnalyseCommandLineArgsDelayed", 0.25f);
    }
    
    /// <summary>
    /// Analyses the command line args, invoked delayed to make sure everything is initialized.
    /// </summary>
    void AnalyseCommandLineArgsDelayed()
    {
        string[] args = Environment.GetCommandLineArgs();
        //If args[1] exists, load it as a JSON file.
        if (args != null && args.Length > 1)
        {
            if (File.Exists(args[1]))
            {
                ShapeLoader.main.LoadShape(args[1]);
            }
        }
    }
}
