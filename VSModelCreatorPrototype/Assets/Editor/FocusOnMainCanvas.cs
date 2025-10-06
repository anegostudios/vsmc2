using UnityEngine;
using UnityEditor;
using System.Reflection;
using Unity.Burst.CompilerServices;
using UnityEditorInternal;

public class FocusOnMainCanvas : MonoBehaviour
{
    [MenuItem("Custom/Focus On Main Canvas #&F")]
    public static void DoFocusOnMainCanvas()
    {
        GameObject activeSelected = Selection.activeGameObject;
        Selection.activeGameObject = GameObject.Find("MainCanvas");
        SceneView.lastActiveSceneView.FrameSelected(false, true);
        Selection.activeGameObject = activeSelected;        
    }
}
