using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    /// <summary>
    /// The UV panel is a nightmare to correctly layout.
    /// This will adjust all six UV children to fit within the bounds of this object.
    /// </summary>
    public class UVImageLayout : MonoBehaviour
    {
        public GridLayoutGroup gridLayout;
        public GameObject noFaceSelectedLabel;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ReorganizeElements();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnEnable()
        {
            Invoke("OnRectTransformDimensionsChange", 0.001f);
        }

        private void OnRectTransformDimensionsChange()
        {
            ReorganizeElements(true);
        }

        /// <summary>
        /// Invokes the reorganization for next frame to ensure elements are set to correct active states.
        /// </summary>
        public void ReorganizeNextFrame()
        {
            Invoke("OnRectTransformDimensionsChange", 0.02f);
        }


        public void ReorganizeElements(bool fromUIEvent = false)
        {
            RectTransform myRect = GetComponent<RectTransform>();
            float totWidth = myRect.rect.width - (gridLayout.padding.left + gridLayout.padding.right);
            float totHeight = myRect.rect.height - (gridLayout.padding.top + gridLayout.padding.bottom);
            int activeChildren = 0;
            foreach (RectTransform c in transform)
            {
                if (c.gameObject.activeSelf && c.gameObject != noFaceSelectedLabel) activeChildren++;
            }
            if (activeChildren == 0)
            {
                if (noFaceSelectedLabel != null && !fromUIEvent) noFaceSelectedLabel.SetActive(true);
                return;
            }
            if (noFaceSelectedLabel != null && !fromUIEvent) noFaceSelectedLabel.SetActive(false);

            //Hope the maths is right...
            //float prefElemWidth = (totWidth + gridLayout.spacing.x) / activeChildren - gridLayout.spacing.x;
            //float prefElemHeight = (totHeight + gridLayout.spacing.y) / activeChildren - gridLayout.spacing.y;


            int childPerRow = activeChildren;
            int[] widths = new int[activeChildren];
            int bestSize = 0;
            int i = 0;
            for (; childPerRow > 0; childPerRow--)
            {
                widths[i] = (int)((totWidth + gridLayout.spacing.x) / childPerRow - gridLayout.spacing.x);
                //Check if fits...
                int rows = Mathf.CeilToInt((float)activeChildren / childPerRow);
                if (widths[i] + (rows - 1) * (gridLayout.spacing.y + widths[i]) <= totHeight)
                {
                    //This size is valid!
                    bestSize = Mathf.Max(bestSize, widths[i]);
                }
                i++;
            }


            int childPerCol = activeChildren;
            int[] heights = new int[activeChildren];
            i = 0;
            for (; childPerCol > 0; childPerCol--)
            {
                heights[i] = (int)((totHeight + gridLayout.spacing.y) / childPerCol - gridLayout.spacing.y);
                //Check if fits...
                int cols = Mathf.CeilToInt((float)activeChildren / childPerCol);
                if (heights[i] + (cols - 1) * (gridLayout.spacing.x + heights[i]) <= totWidth)
                {
                    //This size is valid!
                    bestSize = Mathf.Max(bestSize, heights[i]);
                }
                i++;
            }

            if (bestSize > totWidth) bestSize = (int)totWidth;
            if (bestSize > totHeight) bestSize = (int)totHeight;

            gridLayout.cellSize = Vector2.one * bestSize;


        }

    }
}