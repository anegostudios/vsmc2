#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
using System.Runtime.InteropServices;
using UnityEngine;

namespace Riten.Native.Cursors
{
    public class MacOSCursorService : ICursorService
    {
        [DllImport("CursorWrapper")]
        private static extern void SetCursorToArrow();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToIBeam();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToCrosshair();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToOpenHand();
        
        [DllImport("CursorWrapper")]
        private static extern void SetCursorToClosedHand();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToResizeLeftRight();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToResizeUp();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToResizeDown();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToResizeUpDown();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToOperationNotAllowed();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToPointingHand();
        
        [DllImport("CursorWrapper")]
        private static extern void SetCursorToBusy();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            var service = new MacOSCursorService();
            NativeCursor.SetFallbackService(service);
            NativeCursor.SetService(service);
        }

        public bool SetCursor(NTCursors cursor)
        {
            switch (cursor)
            {
                case NTCursors.Default:
                case NTCursors.Arrow: SetCursorToArrow(); return true;
                
                case NTCursors.IBeam: SetCursorToIBeam(); return true;
                case NTCursors.Crosshair: SetCursorToCrosshair(); return true;
                case NTCursors.Link: SetCursorToPointingHand(); return true;
                case NTCursors.Busy: SetCursorToBusy(); return true;
                case NTCursors.Invalid: SetCursorToOperationNotAllowed(); return true;
                case NTCursors.ResizeVertical: SetCursorToResizeUpDown(); return true;
                case NTCursors.ResizeHorizontal: SetCursorToResizeLeftRight(); return true;
                case NTCursors.ResizeDiagonalLeft: SetCursorToResizeUp(); return true;
                case NTCursors.ResizeDiagonalRight: SetCursorToResizeDown(); return true;
                case NTCursors.ResizeAll: SetCursorToArrow(); return true;
                case NTCursors.OpenHand: SetCursorToOpenHand(); return true;
                case NTCursors.ClosedHand: SetCursorToClosedHand(); return true;
                default: return false;
            }
        }

        public void ResetCursor()
        {
            SetCursorToArrow();
        }
    }
}

#endif