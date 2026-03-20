using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class KeyframeSelector
{

    public List<TimelineKeyFrameElementMarker> currentSelectedKeyframeMarkers;

    public void Reinitialize()
    {
        currentSelectedKeyframeMarkers = new List<TimelineKeyFrameElementMarker>();
    }

    public void ClickSelectKeyframeMarker(TimelineKeyFrameElementMarker marker)
    {
        //Multi-select...
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
        Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (currentSelectedKeyframeMarkers.Contains(marker))
            {
                DeselectMarker(marker);
            }
            else
            {
                SelectMarker(marker);
            }
        }
        else //Single select
        {
            //If the only selected element is this, then deselect 
            if (currentSelectedKeyframeMarkers.Contains(marker) && currentSelectedKeyframeMarkers.Count == 1)
            {
                DeselectMarker(marker);
            }
            else //Otherwise, make sure this is the only selected element.
            {
                DeselectAllMarkers();
                SelectMarker(marker);
            }
        }
    }

    public void SelectMarker(TimelineKeyFrameElementMarker marker)
    {
        if (currentSelectedKeyframeMarkers.Contains(marker)) return;
        marker.GetComponent<Outline>().enabled = true;
        currentSelectedKeyframeMarkers.Add(marker);
    }

    public void DeselectMarker(TimelineKeyFrameElementMarker marker)
    {
        if (!currentSelectedKeyframeMarkers.Contains(marker)) return;
        marker.GetComponent<Outline>().enabled = false;
        currentSelectedKeyframeMarkers.Remove(marker);
    }
    
    public void DeselectAllMarkers()
    {
        while (currentSelectedKeyframeMarkers.Count > 0)
        {
            DeselectMarker(currentSelectedKeyframeMarkers[0]);
        }
    }

}
