using UnityEngine;
using VSMC;

public class ReloadShapeFileOverlay : MonoBehaviour
{

    public void Open()
    {
        if (ShapeLoader.main.storedSaveLocationForFile == "" || ShapeLoader.main.storedSaveLocationForFile == null)
        {
            InfoLogger.main.LogText("You do not have a stored filepath for this shape... please try saving first.");
            return;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Confirm();
            return;
        }
        gameObject.SetActive(true);
    }
    
    public void Confirm()
    {
        gameObject.SetActive(false);
        ShapeLoader.main.ReloadShape();
    }

}
