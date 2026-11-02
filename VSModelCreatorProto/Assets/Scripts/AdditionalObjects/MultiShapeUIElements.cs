using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SFB;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class MultiShapeUIElements : MonoBehaviour
{
    public MultiShapeManager manager;

    [Header("Panel")]
    public GameObject[] onPanelOpenedElements;



    [Header("Values")]
    public Color selectedMultishapeColor;
    public Color deselectedMultishapeColor;
    public Sprite parentExpandedSprite;
    public Sprite parentCollapsedSprite;
    public Sprite entryEnabledSprite;
    public Sprite entryDisabledSprite;


    [Header("Hierarchy")]
    public Transform hierarchyParent;
    public GameObject hierarchyEntryPrefab;
    public Button removeSelectedButton;


    [Header("Selection Properties")]
    public GameObject selectionParent;
    public TMP_InputField attachedToInput;
    public Selectable[] onlyWorkOnNonEditableObjectsSelectables;
    public GameObject onlyShowOnNonEditableObjects;
    public Toggle hideTexturesToggle;
    public GameObject onlyShowOnHideTextures;
    public TMP_InputField colorIndexInput;
    public Slider opacitySlider;
    public Toggle useUniqueAnimationsToggle;

    [Header("Selection Properties - Transformation Fields")]
    public TMP_InputField posX;
    public TMP_InputField posY;
    public TMP_InputField posZ;
    [Space(12)]
    public RotationSlider rotXSlider;
    public RotationSlider rotYSlider;
    public RotationSlider rotZSlider;
    [Space(12)]
    public TMP_InputField originX;
    public TMP_InputField originY;
    public TMP_InputField originZ;
    [Space(12)]
    public TMP_InputField sizeX;
    public TMP_InputField sizeY;
    public TMP_InputField sizeZ;



    [Header("Other")]
    public AttachObjectToUIOverlay attachObjectToOverlay;
    public float translationAndOffsetMultiplier = 16;

    [Header("Runtime")]
    public MultiShapeUIListEntry cSelected;
    public Dictionary<MultiShapeUIListEntry, List<MultiShapeUIListEntry>> createdHierarchyEntriesByParentShapeEntries;

    class TransformationValuesForJSON
    {

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public Vector3Ser Translation;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public Vector3Ser Rotation;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public Vector3Ser Origin;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public Vector3Ser ScaleXYZ;
    }

    void Start()
    {
        EditModeManager.RegisterForOnModeSelect(OnEditModeSelected);
        EditModeManager.RegisterForOnModeDeselect(OnEditModeDeselected);
        ObjectSelector.main.RegisterForObjectSelectedEvent(OnObjectSelected);

        //Register events for elements.
        posX.onEndEdit.AddListener(val => { SetOriginTranslation(EnumAxis.X, float.Parse(val)); });
        posY.onEndEdit.AddListener(val => { SetOriginTranslation(EnumAxis.Y, float.Parse(val)); });
        posZ.onEndEdit.AddListener(val => { SetOriginTranslation(EnumAxis.Z, float.Parse(val)); });

        rotXSlider.AddToOnRotationSetEvent(val => { SetOriginRotation(EnumAxis.X, rotXSlider.Val); });
        rotYSlider.AddToOnRotationSetEvent(val => { SetOriginRotation(EnumAxis.Y, rotYSlider.Val); });
        rotZSlider.AddToOnRotationSetEvent(val => { SetOriginRotation(EnumAxis.Z, rotZSlider.Val); });

        sizeX.onEndEdit.AddListener(val => { SetOriginScale(EnumAxis.X, float.Parse(val)); });
        sizeY.onEndEdit.AddListener(val => { SetOriginScale(EnumAxis.Y, float.Parse(val)); });
        sizeZ.onEndEdit.AddListener(val => { SetOriginScale(EnumAxis.Z, float.Parse(val)); });

        originX.onEndEdit.AddListener(val => { SetOriginOrigin(EnumAxis.X, float.Parse(val)); });
        originY.onEndEdit.AddListener(val => { SetOriginOrigin(EnumAxis.Y, float.Parse(val)); });
        originZ.onEndEdit.AddListener(val => { SetOriginOrigin(EnumAxis.Z, float.Parse(val)); });

        hideTexturesToggle.onValueChanged.AddListener(OnToggleHideTextures);
        colorIndexInput.onEndEdit.AddListener(OnChangeColorIndex);
        opacitySlider.onValueChanged.AddListener(OnOpacitySliderChange);

        useUniqueAnimationsToggle.onValueChanged.AddListener(OnUseUniqueAnimationsToggled);

    }

    public void RecreateHierarchy()
    {
        if (!IsOpen()) return;

        //Store the currently selected element.
        ElementCodeInShape cSelectedCode = null;
        if (cSelected != null && cSelected.rootElem != null)
        {
            cSelectedCode = cSelected.rootElem.GetElementCodeInShape();
        }
        DeselectEntry(false);

        //Find the currently expanded paths.
        List<string> storedExpandedPaths = new List<string>();
        if (createdHierarchyEntriesByParentShapeEntries != null)
        {
            foreach (MultiShapeUIListEntry pEntry in createdHierarchyEntriesByParentShapeEntries.Keys)
            {
                if (pEntry.isCurrentlyExpanded) storedExpandedPaths.Add(pEntry.storedFilepath);
            }
        }

        foreach (Transform t in hierarchyParent)
        {
            Destroy(t.gameObject);
        }


        createdHierarchyEntriesByParentShapeEntries = new Dictionary<MultiShapeUIListEntry, List<MultiShapeUIListEntry>>();

        //Order all the root elements by their root shape path.
        List<ShapeElement> allRootElements = manager.GetAllRootShapeElements();
        Dictionary<string, List<ShapeElement>> rootElementsByShapePath = new Dictionary<string, List<ShapeElement>>();
        foreach (ShapeElement e in allRootElements)
        {
            if (rootElementsByShapePath.ContainsKey(e.parentShape.shapeLoadedFromPath))
            {
                rootElementsByShapePath[e.parentShape.shapeLoadedFromPath].Add(e);
            }
            else
            {
                rootElementsByShapePath[e.parentShape.shapeLoadedFromPath] = new List<ShapeElement>() { e };
            }
        }

        MultiShapeUIListEntry cSelectedEntry = null;
        foreach (var pair in rootElementsByShapePath)
        {
            MultiShapeUIListEntry parentEntry = Instantiate(hierarchyEntryPrefab, hierarchyParent).GetComponent<MultiShapeUIListEntry>();
            bool expanded = storedExpandedPaths.Contains(pair.Key);
            parentEntry.Initialize(manager, this, pair.Key, expanded);
            createdHierarchyEntriesByParentShapeEntries.Add(parentEntry, new List<MultiShapeUIListEntry>());
            foreach (ShapeElement elem in pair.Value)
            {
                MultiShapeUIListEntry elemEntry = Instantiate(hierarchyEntryPrefab, hierarchyParent).GetComponent<MultiShapeUIListEntry>();
                elemEntry.Initialize(manager, this, elem.parentShape, elem, !elem.ShouldHideFromView());
                if (!expanded) elemEntry.gameObject.SetActive(false);
                Debug.Log(elem.GetElementCodeInShape());
                if (elem.GetElementCodeInShape() == cSelectedCode)
                {
                    cSelectedEntry = elemEntry;
                }
                createdHierarchyEntriesByParentShapeEntries[parentEntry].Add(elemEntry);
            }   
        }
        //Reselect the prev selected entry.
        if (cSelectedEntry != null)
        {
            SelectEntry(cSelectedEntry);
        }
        else //SelectEntry will already resolve selection UI.
        {
            ResolveSelectionUI();
        }
    }

    public MultiShapeUIListEntry GetEntryFromRootShapeElement(ShapeElement root)
    {
        if (createdHierarchyEntriesByParentShapeEntries == null) return null;
        foreach (List<MultiShapeUIListEntry> l in createdHierarchyEntriesByParentShapeEntries.Values)
        {
            foreach (MultiShapeUIListEntry m in l)
            {
                if (m.rootElem == root)
                {
                    return m;
                }
            }
        }
        return null;
    }

    public void OnParentEntryCollapseStateChanged(MultiShapeUIListEntry entry)
    {
        foreach (MultiShapeUIListEntry elemEntry in createdHierarchyEntriesByParentShapeEntries[entry])
        {
            elemEntry.gameObject.SetActive(entry.isCurrentlyExpanded);
        }
    }

    public void OnRootElementEnabledStateChanged(MultiShapeUIListEntry entry)
    {
        entry.rootElem.renderInEditor = !entry.rootElem.renderInEditor;
        entry.rootElem.RecalculateHiddenStatus();
        if (entry.shape == ShapeHolder.CurrentLoadedShape)
        {
            ElementHierarchyManager.ElementHierarchy.GetElementHierarchyItem(entry.rootElem).ResolveVisibilityIcon();
        }
    }

    public void OnRootElementEntryClicked(MultiShapeUIListEntry entry)
    {
        SelectEntry(entry);
    }

    public void SelectEntry(MultiShapeUIListEntry entry)
    {
        if (cSelected != null) DeselectEntry(false);
        entry.OnSelected();
        ObjectSelector.main.DeselectAll();
        cSelected = entry;

        LoadedAdditionalShape las = entry.GetAsLoadedAdditionalShape();
        if (las != null)
        {
            las.attachmentHolder.ShowOutlines();
        }

        ResolveSelectionUI();
    }

    public void DeselectEntry(bool resolveUI = true)
    {
        if (cSelected == null) return;
        cSelected.OnDeselected();
        
        LoadedAdditionalShape las = cSelected.GetAsLoadedAdditionalShape();
        if (las != null)
        {
            las.attachmentHolder.HideOutlines();
        }

        cSelected = null;
        if (resolveUI) ResolveSelectionUI();
    }

    public void ResolveSelectionUI()
    {
        bool editableObject = cSelected != null && cSelected.rootElem.parentShape == ShapeHolder.CurrentLoadedShape;
        selectionParent.SetActive(cSelected != null);
        foreach (Selectable s in onlyWorkOnNonEditableObjectsSelectables)
        {
            s.interactable = cSelected != null && !editableObject;
        }
        if (cSelected == null)
        {
            return;
        }

        attachedToInput.text = cSelected.rootElem.GetOriginString();
        IOriginPoint o = cSelected.Origin;
        string dpString = UIConfigManager.main.decimalFormatting;
        sizeX.SetTextWithoutNotify(o.itemScaleXYZ.x.ToString(dpString));
        sizeY.SetTextWithoutNotify(o.itemScaleXYZ.y.ToString(dpString));
        sizeZ.SetTextWithoutNotify(o.itemScaleXYZ.z.ToString(dpString));

        posX.SetTextWithoutNotify((o.itemTranslation.x * translationAndOffsetMultiplier).ToString(dpString));
        posY.SetTextWithoutNotify((o.itemTranslation.y * translationAndOffsetMultiplier).ToString(dpString));
        posZ.SetTextWithoutNotify((o.itemTranslation.z * translationAndOffsetMultiplier).ToString(dpString));

        originX.SetTextWithoutNotify((o.itemOrigin.x * translationAndOffsetMultiplier).ToString(dpString));
        originY.SetTextWithoutNotify((o.itemOrigin.y * translationAndOffsetMultiplier).ToString(dpString));
        originZ.SetTextWithoutNotify((o.itemOrigin.z * translationAndOffsetMultiplier).ToString(dpString));

        rotXSlider.SetToRotationValue(o.itemRotation.x);
        rotYSlider.SetToRotationValue(o.itemRotation.y);
        rotZSlider.SetToRotationValue(o.itemRotation.z);

        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(cSelected.rootElem.GetElementCodeInShape());

        onlyShowOnNonEditableObjects.SetActive(las != null);
        if (las != null)
        {
            hideTexturesToggle.SetIsOnWithoutNotify(las.data.hideTextures);
            onlyShowOnHideTextures.SetActive(las.data.hideTextures);
            colorIndexInput.SetTextWithoutNotify(las.data.flatColorIndex.ToString());
            opacitySlider.SetValueWithoutNotify(las.data.opacity);
            useUniqueAnimationsToggle.SetIsOnWithoutNotify(las.data.useUniqueAnimations);
        }
    }

    public void ToggleOpenClosePanel()
    {
        bool cOpen = !ProgramPreferences.ShowBackdropsAndAttachmentsPanel.GetValue();
        ProgramPreferences.ShowBackdropsAndAttachmentsPanel.SetValue(cOpen);
        if (cOpen)
        {
            AttachmentPointsManager.main.OnOpenedBackdropsPanel();
            OpenPanel();
        }
        else
        {
            ClosePanel();
        }
    }

    public void OpenPanel()
    {
        foreach (GameObject g in onPanelOpenedElements)
        {
            g.SetActive(true);
        }
        RecreateHierarchy();
        ResolveSelectionUI();
    }

    public void ClosePanel()
    {
        DeselectEntry(true);
        foreach (GameObject g in onPanelOpenedElements)
        {
            g.SetActive(false);
        }
    }

    /// <summary>
    /// Checks if all the "on open" elements are opened.
    /// </summary>
    bool IsOpen()
    {
        foreach (GameObject g in onPanelOpenedElements)
        {
            if (!g.activeSelf) return false;
        }
        return true;
    }

    public void OnOpenedAPPanel()
    {
        ProgramPreferences.ShowBackdropsAndAttachmentsPanel.SetValue(false);
        ClosePanel();
    }


    /// <summary>
    /// Used to open or close the edit mode manager based on the current preference setting.
    /// </summary>
    public void OnEditModeSelected(VSEditMode mode)
    {
        if (mode != VSEditMode.Model) return;
        if (ProgramPreferences.ShowBackdropsAndAttachmentsPanel.GetValue())
        {
            OpenPanel();
        }
    }

    public void OnEditModeDeselected(VSEditMode mode)
    {
        if (mode != VSEditMode.Model) return;
        ClosePanel();
    }


    public void OnObjectSelected(GameObject selected)
    {
        DeselectEntry(true);
    }

    public void OnNewShapeButtonClicked()
    {
        // Load file.
        string[] files = StandaloneFileBrowser.OpenFilePanel("Select Additional Shape",
        (string)AssetPathManager.main.GetFirstPreferredAssetPath().Clone(), "json", true);

        if (files == null || files.Length == 0 || string.IsNullOrWhiteSpace(files[0]))
        {
            return;
        }

        foreach (string s in files)
        {
            string resolved = AssetPathManager.main.GetRelativePathForFile(s, "shapes");
            if (MultiShapeManager.main.GetAllLoadedAdditionalShapesForShapePath(resolved.Replace(".json","")).Count() > 0)
            {
                InfoLogger.main.LogText("A shape at this path is already loaded.");
                continue;
            }
            TaskAddNewAdditionalShape addShapeTask = new TaskAddNewAdditionalShape(resolved);
            addShapeTask.DoTask();
            UndoManager.main.CommitTask(addShapeTask);
        }
    }

    public void OnRemoveShapeButtonClicked()
    {
        if (cSelected == null || cSelected.shape == ShapeHolder.CurrentLoadedShape) return;
        TaskRemoveAdditionalShape removeShapeTask = new TaskRemoveAdditionalShape(cSelected.shape);
        removeShapeTask.DoTask();
        UndoManager.main.CommitTask(removeShapeTask);
    }

    public void OnAttachToButtonClicked()
    {
        if (cSelected == null) return;
        attachObjectToOverlay.Open(cSelected.rootElem);
    }
    
    public void SetOriginTranslation(EnumAxis axis, float val)
    {
        val /= translationAndOffsetMultiplier;
        Vector3 old = cSelected.Origin.itemTranslation;
        Vector3 newTranslation = new Vector3(axis == EnumAxis.X ? val : old.x,
                                             axis == EnumAxis.Y ? val : old.y,
                                             axis == EnumAxis.Z ? val : old.z);
        TaskSetOriginPointTranslation setTranslationTask = new TaskSetOriginPointTranslation(cSelected.Origin, newTranslation);
        setTranslationTask.DoTask();
        UndoManager.main.CommitTask(setTranslationTask);
    }

    public void SetOriginRotation(EnumAxis axis, float val)
    {
        Vector3 old = cSelected.Origin.itemRotation;
        Vector3 newRot = new Vector3(axis == EnumAxis.X ? val : old.x,
                                             axis == EnumAxis.Y ? val : old.y,
                                             axis == EnumAxis.Z ? val : old.z);
        TaskSetOriginPointRotation setRotTask = new TaskSetOriginPointRotation(cSelected.Origin, newRot);
        setRotTask.DoTask();
        UndoManager.main.CommitTask(setRotTask);
    }
    public void SetOriginOrigin(EnumAxis axis, float val)
    {
        val /= translationAndOffsetMultiplier;
        Vector3 old = cSelected.Origin.itemOrigin;
        Vector3 newOrig = new Vector3(axis == EnumAxis.X ? val : old.x,
                                             axis == EnumAxis.Y ? val : old.y,
                                             axis == EnumAxis.Z ? val : old.z);
        TaskSetOriginPointOrigin setOrigTask = new TaskSetOriginPointOrigin(cSelected.Origin, newOrig);
        setOrigTask.DoTask();
        UndoManager.main.CommitTask(setOrigTask);
    }
    public void SetOriginScale(EnumAxis axis, float val)
    {
        Vector3 old = cSelected.Origin.itemScaleXYZ;
        Vector3 newScale = new Vector3(axis == EnumAxis.X ? val : old.x,
                                             axis == EnumAxis.Y ? val : old.y,
                                             axis == EnumAxis.Z ? val : old.z);
        TaskSetOriginPointScale setScaleTask = new TaskSetOriginPointScale(cSelected.Origin, newScale);
        setScaleTask.DoTask();
        UndoManager.main.CommitTask(setScaleTask);
    }

    public void OnToggleHideTextures(bool isOn)
    {
        TaskAdditionalShapeHideTextures hideTextures = new TaskAdditionalShapeHideTextures(cSelected.rootElem, isOn);
        hideTextures.DoTask();
        UndoManager.main.CommitTask(hideTextures);
    }

    public void OnChangeColorIndex(string val)
    {
        if (int.TryParse(val, out int newIndex))
        {
            if (newIndex < 0) newIndex = MultiShapeManager.main.shapeColors.Length - 1;
            if (newIndex >= MultiShapeManager.main.shapeColors.Length) newIndex = 0;
            TaskAdditionalShapeSetColorIndex setColorIndex = new TaskAdditionalShapeSetColorIndex(cSelected.rootElem, newIndex);
            setColorIndex.DoTask();
            UndoManager.main.CommitTask(setColorIndex);
            return;
        }
        colorIndexInput.SetTextWithoutNotify(
            MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(cSelected.rootElem.GetElementCodeInShape()).data.flatColorIndex.ToString());
    }

    public void OnOpacitySliderChange(float val)
    {
        TaskAdditionalShapeSetOpacity setOpacityTask = new TaskAdditionalShapeSetOpacity(cSelected.rootElem, val);
        setOpacityTask.DoTask();
        UndoManager.main.CommitTask(setOpacityTask);
        UndoManager.main.MergeTopTasks();
    }

    public void OnUseUniqueAnimationsToggled(bool isOn)
    {
        TaskAdditionalShapeSetUseUniqueAnimations useUniqueAnimsTask = new TaskAdditionalShapeSetUseUniqueAnimations(cSelected.rootElem, isOn);
        useUniqueAnimsTask.DoTask();
        UndoManager.main.CommitTask(useUniqueAnimsTask);
    }
    
    public void OnOpenInContextButtonClicked()
    {
        MultiShapeManager.main.OpenShapeInContextButtonClicked(cSelected.rootElem);
    }

    public void CopyTransformationJSONIntoClipboard()
    {
        TransformationValuesForJSON transformation = new TransformationValuesForJSON();
        transformation.Translation = new Vector3Ser(cSelected.Origin.itemTranslation);
        transformation.Rotation = new Vector3Ser(cSelected.Origin.itemRotation);
        transformation.Origin = new Vector3Ser(cSelected.Origin.itemOrigin);
        transformation.ScaleXYZ = new Vector3Ser(cSelected.Origin.itemScaleXYZ);
        if (cSelected.Origin.itemTranslation == Vector3.zero) transformation.Translation = null;
        if (cSelected.Origin.itemRotation == Vector3.zero) transformation.Rotation = null;
        if (cSelected.Origin.itemOrigin == new Vector3(0.5f, 0.5f, 0.5f)) transformation.Origin = null;
        if (cSelected.Origin.itemScaleXYZ == Vector3.one) transformation.ScaleXYZ = null;

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy(false, false) },
            Converters = new List<JsonConverter>
            {
                new Vector3SerJsonConverter(),
            }
        };
        string s = JsonConvert.SerializeObject(transformation, Formatting.Indented, settings);
        GUIUtility.systemCopyBuffer = s;
        InfoLogger.main.LogText("Copied transformation to system clipboard.");
    }
    
    public void PasteTransformationJSONFromClipboard()
    {
        string s = GUIUtility.systemCopyBuffer;
        try
        {
            s = s.Substring(s.IndexOf('{'), s.LastIndexOf('}') - s.IndexOf('{') + 1);
            TransformationValuesForJSON vals = JsonConvert.DeserializeObject<TransformationValuesForJSON>(s);
            if (vals.Origin == null) vals.Origin = new Vector3Ser(0.5f, 0.5f, 0.5f);
            if (vals.Rotation == null) vals.Rotation = new Vector3Ser(0, 0, 0);
            if (vals.Translation == null) vals.Translation = new Vector3Ser(0, 0, 0);
            if (vals.ScaleXYZ == null) vals.ScaleXYZ = new Vector3Ser(1, 1, 1);
            TaskSetAllOriginPointData setAllTask = new TaskSetAllOriginPointData(
                cSelected.Origin, vals.Origin.ToVec3(), vals.Translation.ToVec3(), vals.Rotation.ToVec3(), vals.ScaleXYZ.ToVec3());
            setAllTask.DoTask();
            UndoManager.main.CommitTask(setAllTask);
            InfoLogger.main.LogText("Pasted transformation from system clipboard.");
        }
        catch
        {
            InfoLogger.main.LogText("Clipboard does not contain a valid transformation property");
        }
    }
}
