using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VSMC;

/// <summary>
/// Used to set specific properties of the frameline, including its width and text elements.
/// </summary>
public class TimelineFrameline : MonoBehaviour
{

    public UnityEvent<int> onFrameSliderValueChanged = new UnityEvent<int>();

    public RectTransform framelineHolderForWidth;
    public RawImage frameTimelineImage;
    public RectTransform scrollViewContentTransform;

    public Transform framelineTexts;
    public GameObject framelineTextPrefab;

    //Interaction Data
    public AnimationEditorUIElements animUIELements;
    bool isMouseDownOnTimeline = false;

    public void SetPropertiesOfFrameline(int frameCount)
    {

        //Round the framecount up to the nearest 15 (major frame)
        int lengthCount = (frameCount + 14) / 15;
        int roundedFrameCount = frameCount;
        framelineHolderForWidth.sizeDelta = new Vector2(480 * lengthCount, 32);
        frameTimelineImage.uvRect = new Rect((480 - 16) / 480f, 0, lengthCount, 1);

        foreach (Transform t in framelineTexts)
        {
            Destroy(t.gameObject);
        }

        for (int i = 0; i < roundedFrameCount; i++)
        {
            GameObject.Instantiate(framelineTextPrefab, framelineTexts).GetComponent<TMP_Text>().text = i.ToString();
        }

        scrollViewContentTransform.sizeDelta = new Vector2(200 + framelineHolderForWidth.sizeDelta.x + 16, scrollViewContentTransform.sizeDelta.y);

    }

    private void Start()
    {
        SetPropertiesOfFrameline(120);
    }

    public void PointerDown(BaseEventData data)
    {
        isMouseDownOnTimeline = true;
    }

    public void PointerUp(BaseEventData data)
    {
        isMouseDownOnTimeline = false;
    }

    private void Update()
    {
        if (isMouseDownOnTimeline)
        {
            //Vector2 pos = RectTransformUtility.PixelAdjustPoint(Input.mousePosition, framelineHolderForWidth, framelineHolderForWidth.root.GetComponent<Canvas>)
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(framelineHolderForWidth, Input.mousePosition, Camera.current, out Vector2 pos))
            {
                //I'm not really sure what the 'if' does, this seems to get called regardless of whether its in the rect or not? But that's okay.
                onFrameSliderValueChanged.Invoke((int)(pos.x / 32));
            }
        }
    }


}
