using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VSMC;

public class AnimationAdditionalShapeAnimationSelector : MonoBehaviour
{

    public TMP_Text shapeLabel;
    public TMP_Dropdown dropdown;
    public LoadedAdditionalShape addShape;
    bool prevExpanded;

    public RectTransform scrollViewTransform;

    public void Initialize(LoadedAdditionalShape addShape, ElementCodeInShape code)
    {
        this.addShape = addShape;
        shapeLabel.text = code.ToString();
        dropdown.interactable = false;
    }

    void Update()
    {
        if (dropdown.IsExpanded && dropdown.IsExpanded != prevExpanded)
        {
            StartCoroutine(RecalculateDropdownSize());
        }
        prevExpanded = dropdown.IsExpanded;
    }
    
    private IEnumerator RecalculateDropdownSize()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); //So this should definitely make it wait until the size is resolved.
        RectTransform scrollViewRt = scrollViewTransform.parent.Find("Dropdown List").GetComponent<RectTransform>();
        RectTransform contentRt = scrollViewRt.Find("Viewport/Content").GetComponent<RectTransform>();
        scrollViewRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentRt.sizeDelta.x);
        Debug.Log("Set size to " + contentRt.sizeDelta.x);
    }

    public void ResolveDropdown(VSMC.Animation forAnim)
    {
        dropdown.interactable = true;
        string cPlayAlongside = "";
        int cPlayAlongsideIndex = 0;
        List<string> options = new List<string>();
        options.Add("CAT_NONE");
        options.Add("CAT_VALID");
        if (addShape.data.animationPlaybackMapping.ContainsKey(forAnim.Code))
        {
            cPlayAlongside = addShape.data.animationPlaybackMapping[forAnim.Code];
        }
        foreach (VSMC.Animation a in addShape.attachmentHolder.GetShape().Animations.OrderBy(x => x.Name))
        {
            if (a.QuantityFrames == forAnim.QuantityFrames)
            {
                options.Add(a.Code + "F" + a.QuantityFrames);
                if (a.Code == cPlayAlongside)
                {
                    cPlayAlongsideIndex = options.Count - 1;
                }
            }
        }
        options.Add("CAT_INVALID");
        foreach (VSMC.Animation a in addShape.attachmentHolder.GetShape().Animations.OrderBy(x => x.Name))
        {
            if (a.QuantityFrames != forAnim.QuantityFrames)
            {
                options.Add(a.Code + "F" + a.QuantityFrames);
                if (a.Code == cPlayAlongside)
                {
                    cPlayAlongsideIndex = options.Count - 1;
                }
            }
        }
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.SetValueWithoutNotify(cPlayAlongsideIndex);
        ResolveSelectedOptionTextAndGetCode();

    }

    public void DisableDropdown()
    {
        dropdown.captionText.text = "";
        dropdown.interactable = false;
    }

    public string ResolveSelectedOptionTextAndGetCode()
    {
        string selCode = dropdown.options[dropdown.value].text;
        string resolvedText;
        string code = "";
        if (selCode == "CAT_INVALID" || selCode == "CAT_VALID")
        {
            //These should not have been selected!
            throw new ArgumentException("You selected an unselectable item in a dropdown list? Quite impressive really.");
        }
        if (selCode == "CAT_NONE")
        {
            resolvedText = "None";
        }
        else
        {
            int i = selCode.LastIndexOf("F");
            resolvedText = selCode.Substring(0, i);
            code = resolvedText;
        }
        dropdown.captionText.text = resolvedText;
        return code;
    }

    public void OnDropdownValueChange(int val)
    {
        string selectedCode = ResolveSelectedOptionTextAndGetCode();
        if (selectedCode == "None")
        {
            selectedCode = "";
        }
        AnimationEditorManager.main.UI.OnPlayAlongsideDropdownSelected(addShape, selectedCode);
    }

}