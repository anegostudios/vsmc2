using UnityEngine;

/// <summary>
/// Place this on the dropdown list itself.
/// </summary>
public class MenubarDropdown : MonoBehaviour
{
    public GameObject dropdownList;

    public void ShowMenuButtonClicked()
    {
        dropdownList.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) ||  Input.GetMouseButtonDown(1))
        {
            dropdownList.SetActive(false);
        }
    }
}
