using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

/// <summary>
/// To avoid clutter, the UI elements required and used by the <see cref="ModelEditor"/> (since there are so many) are managed here.
/// </summary>
public class ModelEditorUIElements : MonoBehaviour
{
    public ModelEditor shapeEditor;

    public GameObject entireModelModeObjectGroup;

    [Header("Misc")]
    public TMP_InputField elemName;
    [Tooltip("Any selectables in here will be not interactable if an object is not selected.")]
    public Selectable[] selectablesThatRequireSelection;

    public OptionSlider localOrGlobalOption;

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
    public RotationSlider rotXSlider;
    public RotationSlider rotYSlider;
    public RotationSlider rotZSlider;

    [Header("Climate color map")]
    public TMP_InputField climateColorMap;
    [Header("Season color map")]
    public TMP_InputField seasonColorMap;
    [Header("Render pass")]
    public TMP_Dropdown renderPass;
    [Header("Z Offset")]
    public TMP_InputField zOffset;

    [Header("Misc")]
    public GizmoController gizmoController;
    public TMP_InputField stepparentInput;
    public GameObject stepparentInputGameObject;
    public GameObject nonRootObjectStepparentWarning;
    public GameObject couldntFindStepParentElementWarning;

    private void Start()
    {
        renderPass.ClearOptions();

        var options = Enum.GetNames(typeof(EnumRenderPass)).ToList();
        renderPass.AddOptions(options);

        RegisterUIEvents();
    }

    private void RegisterUIEvents()
    {
        localOrGlobalOption.AddListenerOnValueChanged(SetLocalOrGlobalMode);
        elemName.onEndEdit.AddListener(val => { elemName.SetTextWithoutNotify(shapeEditor.RenameElement(val)); });

        sizeX.onEndEdit.AddListener(val => { shapeEditor.SetSize(EnumAxis.X, float.Parse(val)); });
        sizeY.onEndEdit.AddListener(val => { shapeEditor.SetSize(EnumAxis.Y, float.Parse(val)); });
        sizeZ.onEndEdit.AddListener(val => { shapeEditor.SetSize(EnumAxis.Z, float.Parse(val)); });

        posX.onEndEdit.AddListener(val => { shapeEditor.SetPosition(EnumAxis.X, float.Parse(val)); });
        posY.onEndEdit.AddListener(val => { shapeEditor.SetPosition(EnumAxis.Y, float.Parse(val)); });
        posZ.onEndEdit.AddListener(val => { shapeEditor.SetPosition(EnumAxis.Z, float.Parse(val)); });

        originX.onEndEdit.AddListener(val => { shapeEditor.SetRotationOrigin(EnumAxis.X, float.Parse(val)); });
        originY.onEndEdit.AddListener(val => { shapeEditor.SetRotationOrigin(EnumAxis.Y, float.Parse(val)); });
        originZ.onEndEdit.AddListener(val => { shapeEditor.SetRotationOrigin(EnumAxis.Z, float.Parse(val)); });

        rotXSlider.AddToOnRotationSetEvent(val => { shapeEditor.SetRotation(EnumAxis.X, rotXSlider.Val); });
        rotYSlider.AddToOnRotationSetEvent(val => { shapeEditor.SetRotation(EnumAxis.Y, rotYSlider.Val); });
        rotZSlider.AddToOnRotationSetEvent(val => { shapeEditor.SetRotation(EnumAxis.Z, rotZSlider.Val); });

        climateColorMap.onEndEdit.AddListener(val => { climateColorMap.SetTextWithoutNotify(shapeEditor.SetClimateColorMap(val)); });
        seasonColorMap.onEndEdit.AddListener(val => { seasonColorMap.SetTextWithoutNotify(shapeEditor.SetSeasonColorMap(val)); });
        renderPass.onValueChanged.AddListener(val => { shapeEditor.SetRenderPass((EnumRenderPass)(val)); });

        zOffset.onEndEdit.AddListener(val => { shapeEditor.SetZOffset(short.Parse(val)); });

        stepparentInput.onEndEdit.AddListener(val => { shapeEditor.SetStepParentElement(val); });
    }

    public void OnElementSelected(ShapeElementGameObject element)
    {
        //Funtime naming of variables here.
        ShapeElement elem = element.element;
        elemName.text = elem.Name;
        RefreshSelectables();

        Vector3 pos;
        Vector3 orig;
        Vector3 rot;
        //if (true) //This was gonna change the values from local to global but I don't think its necessary right now.
        //{
        //Local.
        pos = new Vector3((float)elem.From[0], (float)elem.From[1], (float)elem.From[2]);
        orig = new Vector3((float)elem.RotationOrigin[0], (float)elem.RotationOrigin[1], (float)elem.RotationOrigin[2]);
        rot = new Vector3((float)elem.RotationX, (float)elem.RotationY, (float)elem.RotationZ);
        //}
        //else
        //{
        //    //Global
        //    pos = elem.GetWorldFrom();
        //    orig = elem.GetWorldRotationOrigin();
        //    rot = elem.meshData.storedMatrix.rotation.eulerAngles;
        //}

        string dpString = UIConfigManager.main.decimalFormatting;

        Vector3 size = new Vector3(
            (float)(elem.To[0] - elem.From[0]), 
            (float)(elem.To[1] - elem.From[1]),
            (float)(elem.To[2] - elem.From[2]));
        sizeX.SetTextWithoutNotify(size.x.ToString(dpString));
        sizeY.SetTextWithoutNotify(size.y.ToString(dpString));
        sizeZ.SetTextWithoutNotify(size.z.ToString(dpString));

        posX.SetTextWithoutNotify(pos.x.ToString(dpString));
        posY.SetTextWithoutNotify(pos.y.ToString(dpString));
        posZ.SetTextWithoutNotify(pos.z.ToString(dpString));

        originX.SetTextWithoutNotify(orig.x.ToString(dpString));
        originY.SetTextWithoutNotify(orig.y.ToString(dpString));
        originZ.SetTextWithoutNotify(orig.z.ToString(dpString));

        rotXSlider.SetToRotationValue(rot.x);
        rotYSlider.SetToRotationValue(rot.y);
        rotZSlider.SetToRotationValue(rot.z);

        climateColorMap.text = elem.ClimateColorMap;
        seasonColorMap.text = elem.SeasonColorMap;
        renderPass.SetValueWithoutNotify(elem.RenderPass);

        stepparentInput.SetTextWithoutNotify(elem.StepParentName);
        //Non-root objects should not have step parents.
        if (elem.ParentElement == null)
        {
            stepparentInputGameObject.SetActive(true);
            nonRootObjectStepparentWarning.SetActive(false);
            couldntFindStepParentElementWarning.SetActive(elem.StepParentName != "" && elem.StepParentName != null && elem.StepParentElement == null);
        }
        else
        {
            stepparentInputGameObject.SetActive(false);
            nonRootObjectStepparentWarning.SetActive(true);
            couldntFindStepParentElementWarning.SetActive(false);
        }
    }

    public void RefreshSelectionValues()
    {
        if (!ObjectSelector.main.IsAnySelected()) return;
        OnElementSelected(ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>());
    }

    public void RefreshSelectables()
    {
        foreach (Selectable s in selectablesThatRequireSelection)
        {
            s.interactable = ObjectSelector.main.IsAnySelected();
        }
    }

    public void HideAllUIElements()
    {
        RefreshSelectables();
        entireModelModeObjectGroup.SetActive(false);
    }

    public void ShowAllUIElements()
    {
        entireModelModeObjectGroup.SetActive(true);
    }

    private void SetLocalOrGlobalMode(int val)
    {
        gizmoController.SetGlobalLocalTranslation(val == 1);
        //RefreshSelectionValues();
    }

}
