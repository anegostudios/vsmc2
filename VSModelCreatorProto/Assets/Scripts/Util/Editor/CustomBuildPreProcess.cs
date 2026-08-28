using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CustomBuildPreProcess : IPreprocessBuildWithReport
{

    public int callbackOrder { get { return 100; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        //Enable the SSAO postprocess before the build, otherwise it will not be included.
        GameObject.Find("SceneManager").GetComponent<SceneSettings>().rendererData.rendererFeatures.Find(x => x is ScreenSpaceAmbientOcclusion).SetActive(true);
    }

}