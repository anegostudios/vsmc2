using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionSlider : MonoBehaviour
{

    public Scrollbar scrollbar;
    public TMP_Text option1;
    public TMP_Text option2;
    public int cSelectedValue;
    private UnityEvent<int> OnOptionChangedEvent;

    void Start()
    {
        option1.CrossFadeColor(cSelectedValue == 0 ? Color.white : Color.grey, 0f, true, true);
        option2.CrossFadeColor(cSelectedValue == 1 ? Color.white : Color.grey, 0f, true, true);
        scrollbar.onValueChanged.AddListener(OnScrollbarValueSet);
    }

    public void AddListenerOnValueChanged(UnityAction<int> action)
    {
        if (OnOptionChangedEvent == null) OnOptionChangedEvent = new UnityEvent<int>();
        OnOptionChangedEvent.AddListener(action);
    }

    void Update()
    {
        //Only update if scrollbar is not currently held.
        if (EventSystem.current.currentSelectedGameObject == gameObject && Input.GetMouseButton(0)) return;
        //Easy smoothness - The scrollbar will always go towards the cValue.
        if (scrollbar.value > cSelectedValue)
        {
            scrollbar.SetValueWithoutNotify(Mathf.Clamp01(scrollbar.value - Time.deltaTime * UIConfigManager.main.optionSliderChangeSpeed));
        }
        else if (scrollbar.value < cSelectedValue)
        {
            scrollbar.SetValueWithoutNotify(Mathf.Clamp01(scrollbar.value + Time.deltaTime * UIConfigManager.main.optionSliderChangeSpeed));
        }
    }
    
    void OnScrollbarValueSet(float val)
    {
        if (val < 0.5f)
        {
            //Set option 0.
            if (cSelectedValue == 0) return;
            SetValue(0, true);
        }
        else
        {
            //Set option 1.
            if (cSelectedValue == 1) return;
            SetValue(1, true);
        }
    }

    public void SetValue(int value, bool notify = true)
    {
        cSelectedValue = value;
        option1.CrossFadeColor(value == 0 ? Color.white : Color.grey, 0.25f, true, true);
        option2.CrossFadeColor(value == 1 ? Color.white : Color.grey, 0.25f, true, true);
        if (notify && OnOptionChangedEvent != null)
        {
            OnOptionChangedEvent.Invoke(value);
        } 
    }
    



}
