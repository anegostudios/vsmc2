using TMPro;
using UnityEngine;

public class EditModeDropdown : TMP_Dropdown
{
    protected override DropdownItem CreateItem(DropdownItem itemTemplate)
    {
        DropdownItem item = Object.Instantiate(itemTemplate);
        item.rectTransform.Find("Shortcut").GetComponent<TMP_Text>().text = "Ctrl + " + (itemTemplate.rectTransform.parent.childCount - 1); 
        return item;
    }

}
