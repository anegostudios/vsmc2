using UnityEngine;
using UnityEngine.EventSystems;

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
    }

}
