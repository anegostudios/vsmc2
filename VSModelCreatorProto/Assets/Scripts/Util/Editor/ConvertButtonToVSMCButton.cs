using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

public class ConvertButtonToVSMCButton : MonoBehaviour
{
    [MenuItem("Custom/Convert Button to VSMC Button")]
    public static void DoConvertButtonToVSMCButton()
    {
        foreach (GameObject g in Selection.gameObjects)
        {
            Button b = g.GetComponent<Button>();
            if (b == null) continue;
            DestroyImmediate(b);
            VSMCButton vsmcb = g.AddComponent<VSMCButton>();
            vsmcb.colors = b.colors;
            vsmcb.interactable = b.interactable;
            vsmcb.onClick = b.onClick;
            vsmcb.image = b.image;
            vsmcb.targetGraphic = b.targetGraphic;
        }
    }
}
