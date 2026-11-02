using System;
using System.Globalization;
using System.Linq;
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

    [Header("Misc")]
    public GizmoController gizmoController;
    public TMP_Dropdown renderPassDropdown;
    public TMP_InputField stepparentInput;
    public GameObject stepparentInputGameObject;
    public GameObject nonRootObjectStepparentWarning;
    public GameObject couldntFindStepParentElementWarning;
    public GameObject createNewOriginElementWarningOverlay;

    private void Start()
    {
        RegisterUIEvents();
    }

    private void RegisterUIEvents()
    {
        localOrGlobalOption.AddListenerOnValueChanged(SetLocalOrGlobalMode);
        elemName.onEndEdit.AddListener(val => { elemName.SetTextWithoutNotify(shapeEditor.RenameElement(val)); });

        sizeX.onValueChanged.AddListener(val => { if (float.TryParse(val, out float fVal)) { shapeEditor.SetSize(EnumAxis.X, fVal); } });
        sizeY.onValueChanged.AddListener(val => { if (float.TryParse(val, out float fVal)) { shapeEditor.SetSize(EnumAxis.Y, fVal); } });
        sizeZ.onValueChanged.AddListener(val => { if (float.TryParse(val, out float fVal)) { shapeEditor.SetSize(EnumAxis.Z, fVal); } });
        sizeX.onEndEdit.AddListener(val => { FinalizeInputTask(); });
        sizeY.onEndEdit.AddListener(val => { FinalizeInputTask(); });
        sizeZ.onEndEdit.AddListener(val => { FinalizeInputTask(); });

        posX.onValueChanged.AddListener(val => { if (float.TryParse(val, out float fVal)) { shapeEditor.SetPosition(EnumAxis.X, fVal); } });
        posY.onValueChanged.AddListener(val => { if (float.TryParse(val, out float fVal)) { shapeEditor.SetPosition(EnumAxis.Y, fVal); } });
        posZ.onValueChanged.AddListener(val => { if (float.TryParse(val, out float fVal)) { shapeEditor.SetPosition(EnumAxis.Z, fVal); } });
        posX.onEndEdit.AddListener(val => { FinalizeInputTask(); });
        posY.onEndEdit.AddListener(val => { FinalizeInputTask(); });
        posZ.onEndEdit.AddListener(val => { FinalizeInputTask(); });


        originX.onValueChanged.AddListener(val => { if (float.TryParse(val, out float fVal)) { shapeEditor.SetRotationOrigin(EnumAxis.X, fVal); } });
        originY.onValueChanged.AddListener(val => { if (float.TryParse(val, out float fVal)) { shapeEditor.SetRotationOrigin(EnumAxis.Y, fVal); } });
        originZ.onValueChanged.AddListener(val => { if (float.TryParse(val, out float fVal)) { shapeEditor.SetRotationOrigin(EnumAxis.Z, fVal); } });
        originX.onEndEdit.AddListener(val => { FinalizeInputTask(); });
        originY.onEndEdit.AddListener(val => { FinalizeInputTask(); });
        originZ.onEndEdit.AddListener(val => { FinalizeInputTask(); });

        rotXSlider.AddToOnRotationSetEvent(val => { shapeEditor.SetRotation(EnumAxis.X, val); });
        rotYSlider.AddToOnRotationSetEvent(val => { shapeEditor.SetRotation(EnumAxis.Y, val); });
        rotZSlider.AddToOnRotationSetEvent(val => { shapeEditor.SetRotation(EnumAxis.Z, val); });
        rotXSlider.AddToOnRotationFinalizedEvent(val => FinalizeInputTask());
        rotYSlider.AddToOnRotationFinalizedEvent(val => FinalizeInputTask());
        rotZSlider.AddToOnRotationFinalizedEvent(val => FinalizeInputTask());

        stepparentInput.onEndEdit.AddListener(val => { shapeEditor.SetStepParentElement(val); });

        renderPassDropdown.ClearOptions();
        renderPassDropdown.AddOptions(Enum.GetNames(typeof(EnumChunkRenderPass)).ToList());
        renderPassDropdown.onValueChanged.AddListener(val => { shapeEditor.SetRenderPass(val - 1); });
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

        stepparentInput.SetTextWithoutNotify(elem.StepParentName);
        //Non-root objects should not have step parents.
        if (elem.ParentElement == null)
        {
            stepparentInputGameObject.SetActive(true);
            nonRootObjectStepparentWarning.SetActive(false);
            couldntFindStepParentElementWarning.SetActive(elem.StepParentName != "" && elem.StepParentName != null && elem.OriginPoint == null);
        }
        else
        {
            stepparentInputGameObject.SetActive(false);
            nonRootObjectStepparentWarning.SetActive(true);
            couldntFindStepParentElementWarning.SetActive(false);
        }

        // + 1 on render pass, as default (0th option) is -1 in data.
        renderPassDropdown.SetValueWithoutNotify(elem.RenderPass + 1);

    }

    public void RefreshSelectionValues()
    {
        if (!ObjectSelector.main.IsAnySelected()) return;
        OnElementSelected(ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>());
    }

    public void FinalizeInputTask()
    {
        UndoManager.main.MergeTopTasks();
        RefreshSelectionValues();
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

    public void OnCreateNewOriginElementButtonClicked()
    {
        //Check for any step parents or attached objects.
        foreach (ShapeElement e in ShapeHolder.CurrentLoadedShape.Elements)
        {
            if (!string.IsNullOrEmpty(e.StepParentName) || e.OriginPoint is not TransformOnlyOriginPoint ||
            (e.OriginPoint as TransformOnlyOriginPoint).HasAnyEdits())
            {
                createNewOriginElementWarningOverlay.SetActive(true);
                return;
            }
        }
        shapeEditor.CreateNewOriginElement();
    }

}
