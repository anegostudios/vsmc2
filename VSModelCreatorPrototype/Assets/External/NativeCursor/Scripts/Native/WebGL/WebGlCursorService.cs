#if !UNITY_EDITOR && UNITY_WEBGL

using System.Runtime.InteropServices;
using UnityEngine;

namespace Riten.Native.Cursors
{
    public class WebGlCursorService : ICursorService
    {
        [DllImport("__Internal")]
        private static extern void SetCursorStyle(string cursor);
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            NativeCursor.SetService(new WebGlCursorService());
        }
        
        public bool SetCursor(NTCursors cursor)
        {
            string cursorName = cursor switch
            {
                NTCursors.Default => "default",
                NTCursors.Arrow => "default",
                NTCursors.IBeam => "text",
                NTCursors.Crosshair => "crosshair",
                NTCursors.Link => "pointer",
                NTCursors.Busy => "wait",
                NTCursors.Invalid => "not-allowed",
                NTCursors.ResizeVertical => "ns-resize",
                NTCursors.ResizeHorizontal => "ew-resize",
                NTCursors.ResizeDiagonalLeft => "nwse-resize",
                NTCursors.ResizeDiagonalRight => "nesw-resize",
                NTCursors.ResizeAll => "move",
                NTCursors.OpenHand => "grab",
                NTCursors.ClosedHand => "grabbing",
                _ => "default"
            };
            
            SetCursorStyle(cursorName);
            return true;
        }

        public void ResetCursor()
        {
            SetCursorStyle("default");
        }
    }
}
#endif
