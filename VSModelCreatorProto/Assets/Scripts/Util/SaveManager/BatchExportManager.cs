using UnityEngine;
using TMPro;
using VSMC;
using System.Collections.Generic;
using Unity.VisualScripting;
using SFB;
using System.IO;

public class BatchExportManager : MonoBehaviour
{

    public GameObject batchExportOverlay;
    public TMP_InputField elemPrefix;
    public TMP_InputField elemSuffix;

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
        if (batchExportDir == null || batchExportDir.Length < 1) { return; }

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
                ShapeHolder.CurrentLoadedShape.Elements = new ShapeElement[] { e };
                ShapeAccessor.SerializeShapeToFile(ShapeHolder.CurrentLoadedShape, batchExportDir[0] + Path.DirectorySeparatorChar + exportName + ".json", ShapeLoader.main.beforeShapeSaveEvent, false);
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
