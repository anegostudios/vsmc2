using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

/// <summary>
/// To avoid clutter, the UI elements required and used by the ShapeEditor (since there are so many) are managed here.
/// </summary>
public class ShapeEditorUIElements : MonoBehaviour
{
    public ShapeModelEditor shapeEditor;

    [Header("Misc")]
    public TMP_InputField elemName;

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
    public RotationSlider rotXSlider;
    public RotationSlider rotYSlider;
    public RotationSlider rotZSlider;

    string dpString = "0.###";

    private void Start()
    {
        RegisterUIEvents();
    }

    private void RegisterUIEvents()
    {

        elemName.onEndEdit.AddListener(val => { elemName.SetTextWithoutNotify(shapeEditor.RenameElement(val)); });

        sizeX.onEndEdit.AddListener(val => { shapeEditor.SetSize(EnumAxis.X, float.Parse(val));});
        sizeY.onEndEdit.AddListener(val => { shapeEditor.SetSize(EnumAxis.Y, float.Parse(val));});
        sizeZ.onEndEdit.AddListener(val => { shapeEditor.SetSize(EnumAxis.Z, float.Parse(val));});

        posX.onEndEdit.AddListener(val => { shapeEditor.SetPosition(EnumAxis.X, float.Parse(val)); });
        posY.onEndEdit.AddListener(val => { shapeEditor.SetPosition(EnumAxis.Y, float.Parse(val)); });
        posZ.onEndEdit.AddListener(val => { shapeEditor.SetPosition(EnumAxis.Z, float.Parse(val)); });

        originX.onEndEdit.AddListener(val => { shapeEditor.SetRotationOrigin(EnumAxis.X, float.Parse(val)); });
        originY.onEndEdit.AddListener(val => { shapeEditor.SetRotationOrigin(EnumAxis.Y, float.Parse(val)); });
        originZ.onEndEdit.AddListener(val => { shapeEditor.SetRotationOrigin(EnumAxis.Z, float.Parse(val)); });

        //Rotation fields will also set their respective sliders.
        rotX.onEndEdit.AddListener(val => { shapeEditor.SetRotation(EnumAxis.X, float.Parse(val)); rotXSlider.SetToRotationValue(float.Parse(val)); });
        rotY.onEndEdit.AddListener(val => { shapeEditor.SetRotation(EnumAxis.Y, float.Parse(val)); rotZSlider.SetToRotationValue(float.Parse(val)); });
        rotZ.onEndEdit.AddListener(val => { shapeEditor.SetRotation(EnumAxis.Z, float.Parse(val)); rotYSlider.SetToRotationValue(float.Parse(val)); });

        rotXSlider.rotSlider.onValueChanged.AddListener(val => { shapeEditor.SetRotation(EnumAxis.X, rotXSlider.Val); rotX.SetTextWithoutNotify(rotXSlider.Val.ToString()); });
        rotYSlider.rotSlider.onValueChanged.AddListener(val => { shapeEditor.SetRotation(EnumAxis.Y, rotYSlider.Val); rotY.SetTextWithoutNotify(rotYSlider.Val.ToString()); });
        rotZSlider.rotSlider.onValueChanged.AddListener(val => { shapeEditor.SetRotation(EnumAxis.Z, rotZSlider.Val); rotZ.SetTextWithoutNotify(rotZSlider.Val.ToString()); });
    }

    public void OnElementSelected(ShapeElementGameObject element)
    {
        //Funtime naming of variables here.
        ShapeElement elem = element.element;
        elemName.text = elem.Name;

        Vector3 size = new Vector3(
            (float)(elem.To[0] - elem.From[0]),
            (float)(elem.To[1] - elem.From[1]),
            (float)(elem.To[2] - elem.From[2]));
        sizeX.SetTextWithoutNotify(size.x.ToString(dpString));
        sizeY.SetTextWithoutNotify(size.y.ToString(dpString));
        sizeZ.SetTextWithoutNotify(size.z.ToString(dpString));

        posX.SetTextWithoutNotify(elem.From[0].ToString(dpString));
        posY.SetTextWithoutNotify(elem.From[1].ToString(dpString));
        posZ.SetTextWithoutNotify(elem.From[2].ToString(dpString));

        originX.SetTextWithoutNotify(elem.RotationOrigin[0].ToString(dpString));
        originY.SetTextWithoutNotify(elem.RotationOrigin[1].ToString(dpString));
        originZ.SetTextWithoutNotify(elem.RotationOrigin[2].ToString(dpString));

        rotX.SetTextWithoutNotify(elem.RotationX.ToString(dpString));
        rotY.SetTextWithoutNotify(elem.RotationY.ToString(dpString));
        rotZ.SetTextWithoutNotify(elem.RotationZ.ToString(dpString));
        rotXSlider.SetToRotationValue((float)elem.RotationX);
        rotYSlider.SetToRotationValue((float)elem.RotationY);
        rotZSlider.SetToRotationValue((float)elem.RotationZ);
    }

    public void RefreshSelectionValues()
    {
        if (!ObjectSelector.main.IsAnySelected()) return;
        OnElementSelected(ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>());
    }


}
