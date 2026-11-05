using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VSMC;

public class RotationSlider : MonoBehaviour
{
    public float rotationValue;
    public Slider rotSlider;
    public TMP_InputField rotInputField;
    private UnityEvent<float> OnRotationChangedEvent;
    private UnityEvent<float> OnRotationSetFinalizedEvent;
    private string dpFormat;

    [Space(8)]
    public float standardInc = 1;
    public float shiftMult = 0.1f;
    public float shiftAndControlMult = 0.01f;

    public bool setAxisColor;
    public EnumAxis axis;
    public Image handleForColor;

    private ScrollRect parentScrollRect;

    private void Start()
    {
        parentScrollRect = GetComponentInParent<ScrollRect>();
        if (OnRotationChangedEvent == null) OnRotationChangedEvent = new UnityEvent<float>();
        if (OnRotationSetFinalizedEvent == null) OnRotationSetFinalizedEvent = new UnityEvent<float>();
        dpFormat = UIConfigManager.main.decimalFormatting;
        rotSlider.minValue = -180;
        rotSlider.maxValue = 180;
        if (rotInputField != null)
        {
            rotInputField.onEndEdit.AddListener(x => FinalizeEdit());
            rotInputField.onValueChanged.AddListener(SetRotationFromInputField);
            rotSlider.onValueChanged.AddListener(OnSliderChanged);
            var trigger = rotSlider.GetOrAddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener(x => FinalizeEdit());
            trigger.triggers.Add(entry);
        }

        if (setAxisColor)
        {
            Color c = axis == EnumAxis.X ? UIConfigManager.main.xAxisColor :
            axis == EnumAxis.Y ? UIConfigManager.main.yAxisColor : UIConfigManager.main.zAxisColor;
            handleForColor.color = c;
        }
        //UnlockAllAnglesChanged.AddListener(UnlockAllAnglesChangedCall);
    }


    public void Inc()
    {
        if (rotInputField.text.Equals("~")) return;
        float val = standardInc;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            val *= Input.GetKey(KeyCode.LeftControl) ? shiftAndControlMult : shiftMult;
        }
        rotInputField.text = (rotationValue + val).ToString(dpFormat); //Should trigger onValueChanged
        rotInputField.onEndEdit.Invoke(rotInputField.text); //Trigger onEndEdit
    }

    public void Dec()
    {
        if (rotInputField.text.Equals("~")) return;

        float val = standardInc;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            val *= Input.GetKey(KeyCode.LeftControl) ? shiftAndControlMult : shiftMult;
        }
        rotInputField.text = (rotationValue - val).ToString(dpFormat); //Should trigger onValueChanged
        rotInputField.onEndEdit.Invoke(rotInputField.text); //Trigger onEndEdit
    }

    public void AddToOnRotationSetEvent(UnityAction<float> action)
    {
        if (OnRotationChangedEvent == null) OnRotationChangedEvent = new UnityEvent<float>();
        OnRotationChangedEvent.AddListener(action);
    }

    public void AddToOnRotationFinalizedEvent(UnityAction<float> action)
    {
        if (OnRotationSetFinalizedEvent == null) OnRotationSetFinalizedEvent = new UnityEvent<float>();
        OnRotationSetFinalizedEvent.AddListener(action);
    }

    private void SetRotationFromInputField(string rot)
    {
        if (rot == "") rot = "0";
        if (float.TryParse(rot, out float v))
        {
            SetRotationValueTo(v, true, false);
        }
        //No longer validate here - We do validate onEndEdit though.
    }

    public void SetToRotationValue(float rot, bool notify = false)
    {
        SetRotationValueTo(rot, notify);
    }

    void OnSliderChanged(float val)
    {
        //If left shift is not down, the mouse is down and the relevant slider is currently selected.
        if (Input.GetMouseButton(0) && EventSystem.current.currentSelectedGameObject.GetComponentInParent<Slider>() == rotSlider)
        {
            if (Input.GetKey(KeyCode.LeftShift) ^ ProgramPreferences.RoundRotationValuesOnSliders.GetValue())
            {
                //Lock the value to 1/8ths and also clamp to the minmax values.
                float snapped = Mathf.Clamp(Mathf.Round(val / 22.5f) * 22.5f, -180f, 180f);
                SetRotationValueTo(snapped, true);
            }
            else
            {
                //Lock to whole values
                float snapped = Mathf.Clamp(Mathf.Round(val), -180f, 180f);
                SetRotationValueTo(snapped, true);
            }
        }
        else //set by UI or set by input field
        {
            SetRotationValueTo(val, true);
        }
    }

    void SetRotationValueTo(float val, bool notify = false, bool updateInputField = true)
    {
        rotationValue = val;
        if (updateInputField && !(EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_InputField>() != null))
        {
            rotInputField.SetTextWithoutNotify(val.ToString(dpFormat));
        }
        rotSlider.SetValueWithoutNotify(val);
        if (notify)
        {
            OnRotationChangedEvent.Invoke(val);
        }
    }

    void FinalizeEdit()
    {
        if (rotInputField != null) rotInputField.SetTextWithoutNotify(rotationValue.ToString(dpFormat));
        OnRotationSetFinalizedEvent.Invoke(rotationValue);
    }

    public float Val
    {
        get
        {
            return rotationValue;
        }
    }

    public void OnScrollInputField(BaseEventData data)
    {
        if (Input.GetKey(KeyCode.LeftAlt) && parentScrollRect != null)
        {
            parentScrollRect.OnScroll(data as PointerEventData);
            return;
        }
        if (Input.mouseScrollDelta.y > Mathf.Epsilon) { Inc(); }
        else if (Input.mouseScrollDelta.y < -Mathf.Epsilon) { Dec(); }
    }
}
