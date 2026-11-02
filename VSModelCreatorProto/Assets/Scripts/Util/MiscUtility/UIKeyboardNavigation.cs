using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIKeyboardNavigation : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                HasCustomKeyboardNavigation nav = EventSystem.current.currentSelectedGameObject.GetComponentInParent<HasCustomKeyboardNavigation>();
                if (nav != null)
                {
                    EventSystem.current.SetSelectedGameObject(Input.GetKey(KeyCode.LeftShift) ? nav.prevSelect : nav.nextSelect);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Space) && EventSystem.current.currentSelectedGameObject != null)
        {
            Toggle t = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Toggle>();
            if (t != null)
            {
                t.isOn = !t.isOn;
            }
        }
    }

}
