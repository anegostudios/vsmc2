using UnityEngine;
using UnityEngine.UI;
using VSMC;

/// <summary>
/// Each keyframe marker will have this attached. It's mainly used for the drag'n'drop movement functionality.
/// </summary>
public class TimelineKeyFrameElementMarker : MonoBehaviour
{

    public Image i;
    private RectTransform framelineHolderForWidth;
    private bool isBeingDragged = false;
    private TimelineManager timeline;
    private int cFrame;
    public AnimationKeyFrameElement assosciatedKFE;
    public int assosciatedFlag;
    private KeyframeSelector keyframeSelector;
    private bool prevSelectState;
    private bool hasBeenDragged;

    public void Initialize(TimelineManager timeline, RectTransform framelineHolderForWidth, int cFrame, AnimationKeyFrameElement kfe, int flag, KeyframeSelector kfSelector)
    {
        this.timeline = timeline;
        this.framelineHolderForWidth = framelineHolderForWidth;
        this.cFrame = cFrame;
        this.assosciatedKFE = kfe;
        this.assosciatedFlag = flag;
        this.keyframeSelector = kfSelector;
    }

    public void OnPointerDown()
    {
        if (!AnimationSelector.main.IsAnySelected()) return;

        bool prevState = keyframeSelector.currentSelectedKeyframeMarkers.Contains(this);
        keyframeSelector.ClickSelectKeyframeMarker(this);

        if (prevState && !keyframeSelector.currentSelectedKeyframeMarkers.Contains(this))
        {
            //Do not actually disable until mouse up...
            prevSelectState = false;
            keyframeSelector.SelectMarker(this);
        }
        else
        {
            prevSelectState = true;
        }
        hasBeenDragged = false;
    }

    public void OnBeginDrag()
    {
        isBeingDragged = keyframeSelector.currentSelectedKeyframeMarkers.Contains(this);
        hasBeenDragged = isBeingDragged;
    }

    void Update()
    {
        if (!AnimationSelector.main.IsAnySelected()) return;
        if (isBeingDragged)
        {
            //Update visual position.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(framelineHolderForWidth, Input.mousePosition, Camera.current, out Vector2 pos))
            {
                int frame = (int)(pos.x / 32);
                int dif = frame - cFrame;
                foreach (TimelineKeyFrameElementMarker marker in keyframeSelector.currentSelectedKeyframeMarkers)
                {
                    int nFrame = dif + marker.cFrame;
                    //Why in the seven hells does this need to be anchored position? Local worked fine BEFORE this point. 
                    //I hate Unity UI.
                    marker.GetComponent<RectTransform>().anchoredPosition = new Vector2(nFrame * 32 + 8, 0);
                }
            }
        }
    }

    public void ResetPosition()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(assosciatedKFE.Frame * 32 + 8, 0);
    }

    public void OnEndDrag()
    {
        if (!AnimationSelector.main.IsAnySelected()) return;
        if (isBeingDragged)
        {
            isBeingDragged = false;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(framelineHolderForWidth, Input.mousePosition, Camera.current, out Vector2 pos))
            {
                int frame = (int)(pos.x / 32);
                if (frame == cFrame) return; //Do nothing here...
                timeline.OnKFEMarkerMoved(frame - cFrame);
            }
            //keyframeSelector.DeselectAllMarkers();
        }
    }

    public void OnPointerUp()
    {
        if (!prevSelectState && !hasBeenDragged)
        {
            isBeingDragged = false;
            keyframeSelector.DeselectMarker(this);
        }
        hasBeenDragged = false;
    }


}
