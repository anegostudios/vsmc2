using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdditionalShapeAnimationSelectorItem : MonoBehaviour
{
    public TMP_Text labelText;
    public TMP_Text frame;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (labelText.text == "CAT_VALID")
        {
            labelText.text = "Recommended";
            labelText.fontSize = 12;
            GetComponentInChildren<HorizontalLayoutGroup>().padding = new RectOffset(0, 2, 4, 0);
            labelText.color = Color.grey;
            frame.text = "";
            DestroyImmediate(GetComponentInChildren<Image>());
            DestroyImmediate(GetComponentInChildren<Toggle>());
        }
        else if (labelText.text == "CAT_INVALID")
        {
            labelText.text = "Other";
            labelText.fontSize = 12;
            labelText.color = Color.grey;
            frame.text = "";
            GetComponentInChildren<HorizontalLayoutGroup>().padding = new RectOffset(0, 2, 4, 0);
            DestroyImmediate(GetComponentInChildren<Image>());
            DestroyImmediate(GetComponentInChildren<Toggle>());
        }
        else if (labelText.text == "CAT_NONE")
        {
            labelText.text = "None";
            frame.text = "";
        }
        else
        {
            //Convert the name into the name and frame code.
            int fI = labelText.text.LastIndexOf("F");
            frame.text = labelText.text.Substring(fI + 1);
            labelText.text = labelText.text.Substring(0, fI);
        }
    }

}
