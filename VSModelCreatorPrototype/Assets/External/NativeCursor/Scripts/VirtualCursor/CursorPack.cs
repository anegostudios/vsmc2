using UnityEngine;

namespace Riten.Native.Cursors
{
    [CreateAssetMenu(fileName = "CursorPack", menuName = "Native Cursor/Cursor Pack")]
    public class CursorPack : ScriptableObject
    {
        public VirtualCursorBase @default;
        public VirtualCursorBase pointer;
        public VirtualCursorBase ibeam;
        public VirtualCursorBase wait;
        public VirtualCursorBase cross;
        [Space]
        public VirtualCursorBase grab;
        public VirtualCursorBase grabbing;
        public VirtualCursorBase denied;
        [Space]
        public VirtualCursorBase move;
        public VirtualCursorBase resizeHorizontal;
        public VirtualCursorBase resizeVertical;
        public VirtualCursorBase resizeDiagonal1;
        public VirtualCursorBase resizeDiagonal2;

        public VirtualCursorBase GetCursor(NTCursors ntCursor)
        {
            var cursor = ntCursor switch
            {
                NTCursors.Default => null,
                NTCursors.Arrow => @default,
                NTCursors.Link => pointer,
                NTCursors.IBeam => ibeam,
                NTCursors.Busy => wait,
                NTCursors.Crosshair => cross,
                NTCursors.OpenHand => grab,
                NTCursors.ClosedHand => grabbing,
                NTCursors.Invalid => denied,
                NTCursors.ResizeAll => move,
                NTCursors.ResizeHorizontal => resizeHorizontal,
                NTCursors.ResizeVertical => resizeVertical,
                NTCursors.ResizeDiagonalLeft => resizeDiagonal1,
                NTCursors.ResizeDiagonalRight => resizeDiagonal2,
                _ => null
            };

            return cursor;
        }
        
        /*public bool SetCursorFrame(NTCursors ntCursor, int frame)
        {
            var frameData = cursor.frames[frame];
            
            Cursor.SetCursor(
                frameData.texture, 
                frameData.hotspot * new Vector2(frameData.texture.width, frameData.texture.height),
                CursorMode.Auto
            );
            
            return true;
        }*/
    }
}