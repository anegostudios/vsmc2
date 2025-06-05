using TMPro;
using UnityEngine;
using VSMC;

public class GridSizeSetter : MonoBehaviour
{
    [Header("Unity References")]
    public GridManager gridManager;
    public GameObject gridSizeOverlay;
    public TMP_InputField sizeX;
    public TMP_InputField sizeY;
    public TMP_InputField sizeZ;

    private void Start()
    {
        //Grid sizes must be a multiple of two.
        sizeX.onEndEdit.AddListener(x => sizeX.SetTextWithoutNotify(((int.Parse(x) / 2) * 2).ToString()));
        sizeY.onEndEdit.AddListener(y => sizeY.SetTextWithoutNotify(((int.Parse(y) / 2) * 2).ToString()));
        sizeZ.onEndEdit.AddListener(z => sizeZ.SetTextWithoutNotify(((int.Parse(z) / 2) * 2).ToString()));
    }

    public void OpenGridSizeSetter()
    {
        gridSizeOverlay.SetActive(true);
        sizeX.text = gridManager.prefs.gridSizes.x.ToString();
        sizeY.text = gridManager.prefs.gridSizes.y.ToString();
        sizeZ.text = gridManager.prefs.gridSizes.z.ToString();
    }

    public void ApplyNewGridSizes()
    {
        sizeX.onEndEdit.Invoke(sizeX.text);
        sizeY.onEndEdit.Invoke(sizeY.text);
        sizeZ.onEndEdit.Invoke(sizeZ.text);
        gridManager.prefs.gridSizes = new Vector3Int(int.Parse(sizeX.text), int.Parse(sizeY.text), int.Parse(sizeZ.text));
        gridManager.ResizeGrids();
    }

}
