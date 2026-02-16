using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This is a singleton class that manages the menubar buttons and submenus.
/// The expanded menus use default Unity logic and button events, whereas the menubar buttons use <see cref="MenubarParentButton"/> to automatically register their events.
/// </summary>
public class MenubarManager : MonoBehaviour
{
    public static MenubarManager Instance;

    private List<GameObject> menubarExpandedMenus;

    int cMenubarOpenIndex = -1;

    private void Awake()
    {
        Instance = this;
        cMenubarOpenIndex = -1;
        menubarExpandedMenus = new List<GameObject>();
        //Set all the menu bar items to the menubar tag, except for the actual menubar image Aitself.
        RecursiveSetLayerToMenubar(gameObject.transform.parent);
        gameObject.transform.parent.tag = "Untagged";
    }

    /// <summary>
    /// Using a tag is the easiest way of differentiating between what is a menubar element and what is not.
    /// Would normally use layers - But Unity requires UI elements to be on the UI layer for certain things.
    /// </summary>
    /// <param name="parent"></param>
    private void RecursiveSetLayerToMenubar(Transform parent)
    {
        parent.gameObject.tag = "Menubar";
        foreach (Transform t in parent)
        {
            RecursiveSetLayerToMenubar(t);
        }
    }

    private void Update()
    {
        //If there is a click on any non-menubar object, then we should close the menubar.
        if (cMenubarOpenIndex != -1 && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (!IsMouseCursorOverMenubar())
            {
                CloseAllMenus(true);
            }
        }
    }

    /// <summary>
    /// Based on an answer from:
    /// https://discussions.unity.com/t/detect-mouseover-click-for-ui-canvas-object/152611/3
    /// Thanks Krishx007.
    /// </summary>
    private bool IsMouseCursorOverMenubar()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject.CompareTag("Menubar")) {
                return true;
            }
        }
        return false;
    }


    public int RegisterMenubarButtonAndChild(GameObject childMenu)
    {
        menubarExpandedMenus.Add(childMenu);
        return menubarExpandedMenus.Count - 1;
    }

    public void OnHoverButton(int menubarIndex)
    {
        if (cMenubarOpenIndex != -1)
        {
            CloseAllMenus(false);
            cMenubarOpenIndex = menubarIndex;
            menubarExpandedMenus[cMenubarOpenIndex].SetActive(true);
        }
    }

    public void OnClickButton(int menubarIndex)
    {
        CloseAllMenus(false);
        if (cMenubarOpenIndex == menubarIndex)
        {
            cMenubarOpenIndex = -1;
        }
        else
        {
            cMenubarOpenIndex = menubarIndex;
            menubarExpandedMenus[cMenubarOpenIndex].SetActive(true);
        }
    }

    public void CloseAllMenus(bool setIndex)
    {
        foreach (GameObject g in menubarExpandedMenus)
        {
            g.SetActive(false);
        }
        if (setIndex) cMenubarOpenIndex = -1;
    }


}
