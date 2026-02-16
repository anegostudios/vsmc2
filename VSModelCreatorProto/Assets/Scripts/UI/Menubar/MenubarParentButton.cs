using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// I've never actually overriden a Unity button before.
/// Hopefully it works.
/// 
/// Plot twist; It did not work. Now a regular gameobject with a button attached.
/// </summary>
public class MenubarParentButton : MonoBehaviour
{

    public GameObject menubarChildMenu;
    int menubarButtonIndex = -1;

    void Start()
    {
        //Need to register this button on the menubar manager.
        menubarButtonIndex = MenubarManager.Instance.RegisterMenubarButtonAndChild(menubarChildMenu);

        //Adding the click event is quite easy...
        GetComponent<Button>().onClick.AddListener(OnPointerClick);

        //But the pointer enter is more difficult.
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener(data => { OnPointerEnter(); });
        trigger.triggers.Add(entry);
    }


    public void OnPointerClick()
    {
        //Hardcoded to open (or close) with the menubar manager.
        MenubarManager.Instance.OnClickButton(menubarButtonIndex);
    }

    public void OnPointerEnter()
    {
        //This will set the index to this menubar button's ID.
        MenubarManager.Instance.OnHoverButton(menubarButtonIndex);
    }   

}
