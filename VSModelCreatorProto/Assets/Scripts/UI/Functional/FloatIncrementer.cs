using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VSMC;

public class FloatIncrementer : MonoBehaviour
{

    public TMP_InputField inputField;
    public Button incButton;
    public Button decButton;


    [Space(8)]
    public float standardInc = 1;
    public float shiftMult = 0.1f;
    public float shiftAndAltMult = 0.01f;
    string dpFormat;

    [Header("Axis Colors")]
    public bool useAxisColor;
    public EnumAxis axis;

    private ScrollRect parentScrollRect;

    private void Start()
    {
        parentScrollRect = GetComponentInParent<ScrollRect>();
        dpFormat = UIConfigManager.main.decimalFormatting;
        incButton.onClick.AddListener(Inc);
        decButton.onClick.AddListener(Dec);

        if (useAxisColor)
        {
            Color c = axis == EnumAxis.X ? UIConfigManager.main.xAxisColor :
            axis == EnumAxis.Y ? UIConfigManager.main.yAxisColor : UIConfigManager.main.zAxisColor;
            incButton.transform.GetChild(0).GetComponent<Image>().color = c;
            decButton.transform.GetChild(0).GetComponent<Image>().color = c;
        }
    }

    public void Inc()
    {
        if (inputField.text.Equals("~")) return;
        float val = standardInc;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            val *= Input.GetKey(KeyCode.LeftAlt) ? shiftAndAltMult : shiftMult;
        }
        inputField.text = (float.Parse(inputField.text) + val).ToString(dpFormat);
        inputField.onEndEdit.Invoke(inputField.text);
    }

    public void Dec()
    {
        if (inputField.text.Equals("~")) return;

        float val = standardInc;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            val *= Input.GetKey(KeyCode.LeftControl) ? shiftAndAltMult : shiftMult;
        }
        inputField.text = (float.Parse(inputField.text) - val).ToString(dpFormat);
        inputField.onEndEdit.Invoke(inputField.text);
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
