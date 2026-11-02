using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class VSMCToggle : MonoBehaviour
{

    Toggle t;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        t = GetComponent<Toggle>();
        t.onValueChanged.AddListener(SetSelected);
    }

    void SetSelected(bool isOn)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            t.isOn = !t.isOn;            
        }
    }
}
