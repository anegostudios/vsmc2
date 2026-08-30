using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class AttachmentPointsManager : MonoBehaviour
{
    public static AttachmentPointsManager main;

    [Header("Unity Refs")]
    public Toggle onlyShowAPsForSelectedElement;
    public GameObject[] onPanelOpenedElements;
    public Transform apListContent;
    public GameObject apListEntryPrefab;
    public SetAPParentOverlay setAPParentOverlay;
    public GameObject apHolder;

    [Header("AP Edit Refs")]
    public GameObject apEditParent;
    public TMP_InputField apParentName;
    public Button addNewAPButton;
    public Button deleteAPButton;
    public TMP_InputField apCode;
    public TMP_InputField posX;
    public TMP_InputField posY;
    public TMP_InputField posZ;
    public RotationSlider rotX;
    public RotationSlider rotY;
    public RotationSlider rotZ;

    [Header("Runtime Data")]
    public List<LoadedAP> allAPs;
    public APUIEntry[] apEntries;
    public LoadedAP cSelected;

    void Awake()
    {
        main = this;
    }

    void Start()
    {
        apEditParent.SetActive(false);
        onlyShowAPsForSelectedElement.SetIsOnWithoutNotify(ProgramPreferences.OnlyShowAPsForSelectedElement.GetValue());
        EditModeManager.RegisterForOnModeSelect(OnEditModeSelected);
        EditModeManager.RegisterForOnModeDeselect(OnEditModeDeselected);
        ObjectSelector.main.RegisterForObjectSelectedEvent(OnObjectSelected);
        ObjectSelector.main.RegisterForObjectDeselectedEvent(OnObjectDeselected);
        ShapeLoader.RegisterForOnShapeLoadEvent(OnShapeLoaded);
        ShapeLoader.RegisterForOnShapeSaveEvent(BeforeShapeSave);
        UndoManager.RegisterForAnyActionDoneOrUndone(OnAnyActionDone);

        apCode.onEndEdit.AddListener(x => { OnAPCodeChanged(); });
        posX.onEndEdit.AddListener(x => { OnAPPositionChanged(); });
        posY.onEndEdit.AddListener(x => { OnAPPositionChanged(); });
        posZ.onEndEdit.AddListener(x => { OnAPPositionChanged(); });
        rotX.AddToOnRotationSetEvent(x => { OnAPRotationChanged(); });
        rotY.AddToOnRotationSetEvent(x => { OnAPRotationChanged(); });
        rotZ.AddToOnRotationSetEvent(x => { OnAPRotationChanged(); });

    }

    void Update()
    {
    }

    void OnAnyActionDone()
    {
        if (cSelected == null || !apHolder.activeSelf) return;
        //Set the AP Preview.
        apHolder.transform.position = cSelected.GetElement().gameObject.transform.position;
        apHolder.transform.rotation = cSelected.GetElement().gameObject.transform.rotation;

        Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));
        Matrix4x4 rot = cSelected.GetElement().meshData.storedMatrix;
        rot *= Matrix4x4.Rotate(Quaternion.AngleAxis(cSelected.rotation.x, Vector3.right));
        rot *= Matrix4x4.Rotate(Quaternion.AngleAxis(cSelected.rotation.y, Vector3.up));
        rot *= Matrix4x4.Rotate(Quaternion.AngleAxis(cSelected.rotation.z, Vector3.forward));
        rot = flipZ * rot * flipZ;

        Vector3 p = new Vector3(cSelected.position.x, cSelected.position.y, -cSelected.position.z); //flip Z
        apHolder.transform.Translate(p / 16f);
        apHolder.transform.rotation = rot.rotation;
    }

    public void OnShapeLoaded(Shape shape, LoadingContext context)
    {
        List<ShapeElement> es = new List<ShapeElement>();
        es.AddRange(shape.Elements);
        allAPs = new List<LoadedAP>();
        while (es.Count > 0)
        {
            ShapeElement e = es[0];
            es.RemoveAt(0);
            if (e.AttachmentPoints != null)
            {
                foreach (var ap in e.AttachmentPoints)
                {
                    allAPs.Add(new LoadedAP(e, ap));
                }
                e.AttachmentPoints = null;
            }
            if (e.Children != null) es.AddRange(e.Children);
        }
        RecreateAPList();
    }

    public void BeforeShapeSave(Shape shape)
    {
        //Remove all APs from shape data first.
        List<ShapeElement> es = new List<ShapeElement>();
        es.AddRange(shape.Elements);
        while (es.Count > 0)
        {
            ShapeElement e = es[0];
            es.RemoveAt(0);
            e.AttachmentPoints = null;
            if (e.Children != null) es.AddRange(e.Children);
        }

        foreach (LoadedAP ap in allAPs)
        {
            ShapeElement e = ap.GetElement();
            if (e == null) break;
            if (e.AttachmentPoints == null) e.AttachmentPoints = new AttachmentPoint[0];
            e.AttachmentPoints = e.AttachmentPoints.Append(ap.ConvertToJSONAP());
        }
    }

    public void ToggleOnlyShowAPsForSelectedElement()
    {
        ProgramPreferences.OnlyShowAPsForSelectedElement.SetValue(!ProgramPreferences.OnlyShowAPsForSelectedElement.GetValue());
        onlyShowAPsForSelectedElement.SetIsOnWithoutNotify(ProgramPreferences.OnlyShowAPsForSelectedElement.GetValue());
        RecreateAPList();
    }

    public void SelectAttachmentPoint(LoadedAP ap)
    {
        //Block selection if panel isn't open.
        if (!ProgramPreferences.ShowAttachmentPointsPanel.GetValue() || EditModeManager.main.cEditMode != VSEditMode.Model) return;
        DeselectCurrentAP();
        apHolder.SetActive(true);
        deleteAPButton.interactable = true;
        ap.uiEntry.OnSelected();
        cSelected = ap;
        apEditParent.SetActive(true);
        UpdateAPValues();
    }

    public void UpdateAPValues()
    {
        if (cSelected == null) return;
        apParentName.SetTextWithoutNotify(cSelected.GetElement().Name);
        apCode.SetTextWithoutNotify(cSelected.code);
        posX.SetTextWithoutNotify(cSelected.position.x.ToString("0.00"));
        posY.SetTextWithoutNotify(cSelected.position.y.ToString("0.00"));
        posZ.SetTextWithoutNotify(cSelected.position.z.ToString("0.00"));
        rotX.SetToRotationValue(cSelected.rotation.x);
        rotY.SetToRotationValue(cSelected.rotation.y);
        rotZ.SetToRotationValue(cSelected.rotation.z);

        apHolder.transform.position = cSelected.GetElement().gameObject.transform.position;
        apHolder.transform.rotation = cSelected.GetElement().gameObject.transform.rotation;

        Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));
        Matrix4x4 rot = Matrix4x4.Rotate(cSelected.GetElement().gameObject.transform.rotation);
        rot *= Matrix4x4.Rotate(Quaternion.AngleAxis(cSelected.rotation.x, Vector3.right));
        rot *= Matrix4x4.Rotate(Quaternion.AngleAxis(cSelected.rotation.y, Vector3.up));
        rot *= Matrix4x4.Rotate(Quaternion.AngleAxis(cSelected.rotation.z, Vector3.forward));
        rot = flipZ * rot * flipZ;

        Vector3 p = new Vector3(cSelected.position.x, cSelected.position.y, -cSelected.position.z); //flip Z
        apHolder.transform.Translate(p / 16f);
        apHolder.transform.rotation = rot.rotation;
    }

    public void ReselectCurrentAP()
    {
        if (cSelected == null) return;
        SelectAttachmentPoint(cSelected);
    }

    public void OnSelectAPParentOverlay()
    {
        if (cSelected == null) return;
        setAPParentOverlay.OpenOverlay(cSelected);
    }

    public void SetAPParent(LoadedAP ap, ShapeElement parent)
    {
        TaskSetAttachmentPointParent setParentTask = new TaskSetAttachmentPointParent(ap, parent);
        setParentTask.DoTask();
        UndoManager.main.CommitTask(setParentTask);
    }

    public void DeselectCurrentAP()
    {
        apEditParent.SetActive(false);
        apHolder.SetActive(false);
        deleteAPButton.interactable = false;
        if (cSelected == null) return;
        cSelected.uiEntry.OnDeselected();
        cSelected = null;
    }

    public void RecreateAPList()
    {
        foreach (Transform t in apListContent)
        {
            Destroy(t.gameObject);
        }

        LoadedAP sSel = cSelected;
        DeselectCurrentAP();
        apEntries = new APUIEntry[allAPs.Count];
        bool onlyShowSelectedAPs = ProgramPreferences.OnlyShowAPsForSelectedElement.GetValue();
        ShapeElement cSel = null;
        addNewAPButton.interactable = ObjectSelector.main.IsAnySelected();
        if (ObjectSelector.main.IsAnySelected())
        {
            cSel = ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
        }
        if (cSel == null && onlyShowSelectedAPs) return;
        for (int i = 0; i < allAPs.Count; i++)
        {
            if (onlyShowSelectedAPs && allAPs[i].GetElement() != cSel) continue;
            APUIEntry e = Instantiate(apListEntryPrefab, apListContent).GetComponent<APUIEntry>();
            e.Initialize(allAPs[i]);
            if (allAPs[i] == sSel)
            {
                SelectAttachmentPoint(sSel);
            }
            apEntries[i] = e;
        }
    }

    public LoadedAP GetLoadedAPFromCode(string code)
    {
        return allAPs.FirstOrDefault(x => x.code == code);
    }

    public void AddNewAP(LoadedAP newAP)
    {
        allAPs.Add(newAP);
        RecreateAPList();
        SelectAttachmentPoint(newAP);
    }
    
    public void RemoveAP(LoadedAP removeAP)
    {
        allAPs.Remove(removeAP);
        RecreateAPList();
    }

    public void OnCreateNewAPButtonClicked()
    {
        if (!ObjectSelector.main.IsAnySelected()) return;
        string code = "Point1";
        //Get unique name.
        while (allAPs.Any(x => x.code == code))
        {
            code = ShapeElement.IncrementName(code);
        }
        TaskAddNewAttachmentPoint addAPTask = new TaskAddNewAttachmentPoint(code, ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element);
        addAPTask.DoTask();
        UndoManager.main.CommitTask(addAPTask);
    }

    public void OnDeleteAPButtonClicked()
    {
        if (cSelected == null) return;
        TaskDeleteAttachmentPoint delAPTask = new TaskDeleteAttachmentPoint(cSelected);
        delAPTask.DoTask();
        UndoManager.main.CommitTask(delAPTask);
    }

    public void OnAPCodeChanged()
    {
        if (cSelected == null) return;
        string newCode = apCode.text;
        if (newCode == cSelected.code) return;
        if (allAPs.Any(x => x.code == newCode)) //AP codes must be lowercase.
        {
            apCode.SetTextWithoutNotify(cSelected.code);
            return;
        }
        TaskSetAttachmentPointCode setCodeTask = new TaskSetAttachmentPointCode(cSelected, newCode);
        setCodeTask.DoTask();
        UndoManager.main.CommitTask(setCodeTask);
    }

    public void OnAPPositionChanged()
    {
        try
        {
            float x = float.Parse(posX.text);
            float y = float.Parse(posY.text);
            float z = float.Parse(posZ.text);
            TaskSetAttachmentPointPosition setPosTask = new TaskSetAttachmentPointPosition(cSelected, new Vector3(x, y, z));
            setPosTask.DoTask();
            UndoManager.main.CommitTask(setPosTask);
        }
        catch
        {
            ReselectCurrentAP();
        }
    }

    public void OnAPRotationChanged()
    {
        TaskSetAttachmentPointRotation setRotTask = new TaskSetAttachmentPointRotation(cSelected, new Vector3(rotX.Val, rotY.Val, rotZ.Val));
        setRotTask.DoTask();
        UndoManager.main.CommitTask(setRotTask);
    }

    public void ToggleOpenClosePanel()
    {
        bool cOpen = !ProgramPreferences.ShowAttachmentPointsPanel.GetValue();
        ProgramPreferences.ShowAttachmentPointsPanel.SetValue(cOpen);
        if (cOpen)
        {
            BackdropAndAttachmentMenuManager.main.OnOpenedAPPanel();
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
    }

    public void ClosePanel()
    {
        DeselectCurrentAP();
        foreach (GameObject g in onPanelOpenedElements)
        {
            g.SetActive(false);
        }
    }

    public void OnOpenedBackdropsPanel()
    {
        ProgramPreferences.ShowAttachmentPointsPanel.SetValue(false);
        ClosePanel();
    }

    /// <summary>
    /// Used to open or close the edit mode manager based on the current preference setting.
    /// </summary>
    public void OnEditModeSelected(VSEditMode mode)
    {
        if (mode != VSEditMode.Model) return;
        if (ProgramPreferences.ShowAttachmentPointsPanel.GetValue())
        {
            Debug.Log("Opening AP Panel.");
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
        RecreateAPList();
    }

    public void OnObjectDeselected(GameObject deselected)
    {
        RecreateAPList();
    }

}
