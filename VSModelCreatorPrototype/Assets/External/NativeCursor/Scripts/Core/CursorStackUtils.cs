using UnityEngine;

namespace Riten.Native.Cursors.UI
{
    public static class CursorStackUtils
    {
        public static int CalculateTransformDepth(this Transform transform)
        {
            var depth = 0;
            var parent = transform.parent;
            while (parent != null)
            {
                depth++;
                parent = parent.parent;
            }

            return depth;
        }
        
        public static int GetSecondaryPriority(this Transform transform, int transformDepth)
        {
            return transformDepth * 100 + transform.GetSiblingIndex();
        }
    }
}