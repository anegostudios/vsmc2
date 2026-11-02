using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VSMC;

public class ModelInfoOverlay : MonoBehaviour
{

    [TextArea]
    public string infoString;
    public TMP_Text infoText;

    public void Open()
    {
        //Get info
        int tris = 0;
        int totElems = 0;
        List<ShapeElement> elems = new List<ShapeElement>();
        elems.AddRange(ShapeHolder.CurrentLoadedShape.Elements);
        while (elems.Count > 0)
        {
            ShapeElement e = elems[0];
            elems.RemoveAt(0);
            totElems++;
            foreach (ShapeElementFace f in e.FacesResolved)
            {
                if (f.Enabled) tris += 2;
            }
            if (e.Children != null) elems.AddRange(e.Children);
        }

        //Calculate joints
        int joints = 0;
        if (EditModeManager.main.cEditMode != VSEditMode.Animation)
        {
            ShapeHolder.CurrentLoadedShape.InitForAnimations("forInfo");
        }
        joints = ShapeHolder.CurrentLoadedShape.JointsById.Count;

        infoText.text = string.Format(infoString,
        tris, totElems, TextureManager.main.loadedTextures.Count, ShapeHolder.CurrentLoadedShape.Animations.Length, joints);
        gameObject.SetActive(true);
    }
}
