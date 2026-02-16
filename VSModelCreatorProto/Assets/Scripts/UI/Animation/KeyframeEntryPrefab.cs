using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class KeyframeEntryPrefab : MonoBehaviour
{

    public TMP_Text frame;
    public TMP_Text translation;
    public TMP_Text rotation;
    //public TMP_Text scale;

    public void Initialize(AnimationKeyFrame keyframe)
    {
        frame.text = keyframe.Frame.ToString();
        bool fT = false;
        bool fR = false;
        //bool fS = false;
        foreach (AnimationKeyFrameElement kfelem in keyframe.Elements.Values)
        {
            if (fT && fR) break;
            if (!fT)
            {
                if (kfelem.PositionSet)
                {
                    fT = true;
                }
            }
            if (!fR)
            {
                if (kfelem.RotationSet)
                {
                    fR = true;
                }
            }
        }

        translation.gameObject.SetActive(fT);
        rotation.gameObject.SetActive(fR);
    }

}
