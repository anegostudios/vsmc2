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
    public Slider rotSlider;
    public TMP_InputField rotInputField;
    private UnityEvent<float> OnRotationSetEvent;
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
        if (OnRotationSetEvent == null) OnRotationSetEvent = new UnityEvent<float>();
        dpFormat = UIConfigManager.main.decimalFormatting;
        rotSlider.minValue = -180;
        rotSlider.maxValue = 180;
        if (rotInputField != null)
        {
            rotInputField.onEndEdit.AddListener(SetRotationFromInputField);
            rotSlider.onValueChanged.AddListener(OnSliderChanged);
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
        rotInputField.text = (float.Parse(rotInputField.text) + val).ToString(dpFormat);
        rotInputField.onEndEdit.Invoke(rotInputField.text);
    }

    public void Dec()
    {
        if (rotInputField.text.Equals("~")) return;

        float val = standardInc;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            val *= Input.GetKey(KeyCode.LeftControl) ? shiftAndControlMult : shiftMult;
        }
        rotInputField.text = (float.Parse(rotInputField.text) - val).ToString(dpFormat);
        rotInputField.onEndEdit.Invoke(rotInputField.text);
    }

    public void AddToOnRotationSetEvent(UnityAction<float> action)
    {
        if (OnRotationSetEvent == null) OnRotationSetEvent = new UnityEvent<float>();
        OnRotationSetEvent.AddListener(action);
    }

    private void SetRotationFromInputField(string rot)
    {
        if (float.TryParse(rot, out float v))
        {
            rotSlider.value = v;
        }
        else
        {
            if (rotInputField != null) rotInputField.SetTextWithoutNotify(Val.ToString(dpFormat));
        }
    }

    public void SetToRotationValue(float rot, bool notify = false)
    {
        if (notify)
        {
            rotSlider.value = rot;
        }
        else
        {
            rotSlider.SetValueWithoutNotify(rot);
            rotInputField.SetTextWithoutNotify(rot.ToString(dpFormat));
        }
    }

    void OnSliderChanged(float val)
    {
        //If left shift is not down, the mouse is down and the relevant slider is currently selected.
        if (Input.GetMouseButton(0) && EventSystem.current.currentSelectedGameObject.GetComponentInParent<Slider>() == rotSlider)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                //Lock the value to 1/8ths and also clamp to the minmax values.
                float snapped = Mathf.Clamp(Mathf.Round(val / 22.5f) * 22.5f, -180f, 180f);
                rotSlider.SetValueWithoutNotify(snapped);
                rotInputField.SetTextWithoutNotify(snapped.ToString(dpFormat));
                OnRotationSetEvent.Invoke(snapped);
            }
            else
            {
                //Lock to whole values
                float snapped = Mathf.Clamp(Mathf.Round(val), -180f, 180f);
                rotSlider.SetValueWithoutNotify(snapped);
                rotInputField.SetTextWithoutNotify(snapped.ToString(dpFormat));
                OnRotationSetEvent.Invoke(snapped);
            }
        }
        else //set by UI or set by input field
        {
            rotSlider.SetValueWithoutNotify(val);
            rotInputField.SetTextWithoutNotify(val.ToString(dpFormat));
            OnRotationSetEvent.Invoke(val);
        }
    }

    public float Val
    {
        get
        {
            return rotSlider.value;
            //return UnlockAllAngles ? rotSlider.value : rotSlider.value * (180f / 8f);
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
