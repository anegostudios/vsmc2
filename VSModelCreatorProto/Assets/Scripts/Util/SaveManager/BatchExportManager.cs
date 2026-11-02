using UnityEngine;
using TMPro;
using VSMC;
using System.Collections.Generic;
using Unity.VisualScripting;
using SFB;
using System.IO;
using UnityEngine.UI;

public class BatchExportManager : MonoBehaviour
{

    public GameObject batchExportOverlay;
    public TMP_InputField elemPrefix;
    public TMP_InputField elemSuffix;
    public Toggle forceLowercaseFileNamesToggle;

    public TMP_Text success;
    public TMP_Text failure;
    public void OpenBatchExportOverlay()
    {
        success.gameObject.SetActive(false);
        failure.gameObject.SetActive(false);
        batchExportOverlay.SetActive(true);
    }

    public void DoBatchExport()
    {
        string[] batchExportDir = StandaloneFileBrowser.OpenFolderPanel("Select the batch export folder", "", false);
        if (batchExportDir == null || batchExportDir.Length < 1 || string.IsNullOrWhiteSpace(batchExportDir[0])) { return; }

        List<ShapeElement> rootElements = new List<ShapeElement>();
        rootElements.AddRange(ShapeHolder.CurrentLoadedShape.Elements);

        int successCount = 0;
        foreach (ShapeElement e in rootElements)
        {
            if (e.Name.StartsWith(elemPrefix.text, System.StringComparison.CurrentCultureIgnoreCase))
            {
                int lC = e.Name.Length - elemPrefix.text.Length;
                if (e.Name.EndsWith(elemSuffix.text, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    lC -= elemSuffix.text.Length;
                }
                string exportName = e.Name.Substring(elemPrefix.text.Length, lC);
                if (forceLowercaseFileNamesToggle.isOn)
                {
                    exportName = exportName.ToLower();
                }
                if (exportName == "")
                {
                    //Don't allow an empty file name.
                    continue;
                }
                ShapeHolder.CurrentLoadedShape.Elements = new ShapeElement[] { e };
                string path = batchExportDir[0] + Path.DirectorySeparatorChar + exportName + ".json";
                ShapeAccessor.SerializeShapeToFile(ShapeHolder.CurrentLoadedShape, path, ShapeLoader.main.beforeShapeSaveEvent, false);
                successCount++;
            }
        }
        ShapeHolder.CurrentLoadedShape.Elements = rootElements.ToArray();
        if (successCount == rootElements.Count)
        {
            success.text = "Success! Exported " + successCount + " objects.";
            success.gameObject.SetActive(true); 
        }
        else
        {
            failure.text = "Not all root shapes matched the prefix and suffix. Exported " + successCount + " of " + rootElements.Count + " objects.";
            failure.gameObject.SetActive(true);
        }
    }

}
