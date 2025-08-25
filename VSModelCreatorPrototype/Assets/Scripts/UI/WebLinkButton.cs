using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WebLinkButton : MonoBehaviour
{
    public string urlToOpen;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            Application.OpenURL(urlToOpen);
        });
    }

}
