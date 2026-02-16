using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    public class TimelineElementPrefab : MonoBehaviour
    {

        public Text shapeElementName;


        public void Init(TimelineManager tlMan)
        {
            RecreateTLElement(tlMan);
        }

        public void RecreateTLElement(TimelineManager tlMan)
        {

        }
    }
}