using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatIncrementer : MonoBehaviour
{

    public TMP_InputField inputField;
    public Button incButton;
    public Button decButton;

    [Space(8)]
    public float standardInc = 1;
    public float ctrlInc = 0.1f;

    
    private void Start()
    {
        incButton.onClick.AddListener(Inc);
        decButton.onClick.AddListener(Dec);
    }

    public void Inc()
    {
        if (inputField.text.Equals("~")) return;
        float val = Input.GetKey(KeyCode.LeftControl) ? ctrlInc : standardInc;
        inputField.text = (float.Parse(inputField.text) + val).ToString();
        inputField.onEndEdit.Invoke(inputField.text);
    }

    public void Dec()
    {
        if (inputField.text.Equals("~")) return;
        float val = Input.GetKey(KeyCode.LeftControl) ? ctrlInc : standardInc;
        inputField.text = (float.Parse(inputField.text) - val).ToString();
        inputField.onEndEdit.Invoke(inputField.text);
    }
}
