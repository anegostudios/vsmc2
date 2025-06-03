using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

/// <summary>
/// To avoid clutter, the UI elements required and used by the ShapeEditor (since there are so many) are managed here.
/// </summary>
public class ShapeEditorUIElements : MonoBehaviour
{
    public ShapeEditor shapeEditor;

    [Header("Size")]
    public TMP_InputField sizeX;
    public TMP_InputField sizeY;
    public TMP_InputField sizeZ;

    [Header("Position")]
    public TMP_InputField posX;
    public TMP_InputField posY;
    public TMP_InputField posZ;

    [Header("Rotation Origin")]
    public TMP_InputField originX;
    public TMP_InputField originY;
    public TMP_InputField originZ;

    [Header("Rotations")]
    public TMP_InputField rotX;
    public TMP_InputField rotY;
    public TMP_InputField rotZ;

    private void Start()
    {
        RegisterUIEvents();
    }

    private void RegisterUIEvents()
    {
        sizeX.onEndEdit.AddListener(val => { shapeEditor.SetSize(EnumAxis.X, float.Parse(val));});
        sizeY.onEndEdit.AddListener(val => { shapeEditor.SetSize(EnumAxis.Y, float.Parse(val));});
        sizeZ.onEndEdit.AddListener(val => { shapeEditor.SetSize(EnumAxis.Z, float.Parse(val));});

        posX.onEndEdit.AddListener(val => { shapeEditor.SetPosition(EnumAxis.X, float.Parse(val)); });
        posY.onEndEdit.AddListener(val => { shapeEditor.SetPosition(EnumAxis.Y, float.Parse(val)); });
        posZ.onEndEdit.AddListener(val => { shapeEditor.SetPosition(EnumAxis.Z, float.Parse(val)); });

        originX.onEndEdit.AddListener(val => { shapeEditor.SetRotationOrigin(EnumAxis.X, float.Parse(val)); });
        originY.onEndEdit.AddListener(val => { shapeEditor.SetRotationOrigin(EnumAxis.Y, float.Parse(val)); });
        originZ.onEndEdit.AddListener(val => { shapeEditor.SetRotationOrigin(EnumAxis.Z, float.Parse(val)); });

        rotX.onEndEdit.AddListener(val => { shapeEditor.SetRotation(EnumAxis.X, float.Parse(val)); });
        rotY.onEndEdit.AddListener(val => { shapeEditor.SetRotation(EnumAxis.Y, float.Parse(val)); });
        rotZ.onEndEdit.AddListener(val => { shapeEditor.SetRotation(EnumAxis.Z, float.Parse(val)); });
    }

    public void OnElementSelected(ShapeElementGameObject element)
    {
        //Funtime naming of variables here.
        ShapeElement elem = element.element;
        Vector3 size = new Vector3(
            (float)(elem.To[0] - elem.From[0]),
            (float)(elem.To[1] - elem.From[1]),
            (float)(elem.To[2] - elem.From[2]));
        sizeX.SetTextWithoutNotify(size.x.ToString());
        sizeY.SetTextWithoutNotify(size.y.ToString());
        sizeZ.SetTextWithoutNotify(size.z.ToString());

        posX.SetTextWithoutNotify(elem.From[0].ToString());
        posY.SetTextWithoutNotify(elem.From[1].ToString());
        posZ.SetTextWithoutNotify(elem.From[2].ToString());

        originX.SetTextWithoutNotify(elem.RotationOrigin[0].ToString());
        originY.SetTextWithoutNotify(elem.RotationOrigin[1].ToString());
        originZ.SetTextWithoutNotify(elem.RotationOrigin[2].ToString());

        rotX.SetTextWithoutNotify(elem.RotationX.ToString());
        rotY.SetTextWithoutNotify(elem.RotationY.ToString());
        rotZ.SetTextWithoutNotify(elem.RotationZ.ToString());
    }


}
