using UnityEngine;

namespace Riten.Native.Cursors.Virtual
{
    public class AutoSetCursorPack : MonoBehaviour
    {
        [SerializeField] CursorPack _cursorPack;
        [SerializeField] Camera _camera;

        private CursorPack _lastActivated;

        private void OnEnable()
        {
            if (_cursorPack == null)
                return;
            
            NativeCursor.SetCursorPack(_cursorPack, _camera);
            NativeCursor.SetCursor(NTCursors.Arrow);
            
            _lastActivated = _cursorPack;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying || !enabled) return;
            
            if (_lastActivated != null && _lastActivated != _cursorPack)
            {
                NativeCursor.SetCursorPack(_cursorPack, _camera);
                NativeCursor.SetCursor(NTCursors.Arrow);
                _lastActivated = _cursorPack;
            }
        }

        private void OnDisable()
        {
            if (_lastActivated)
            {
                NativeCursor.ClearCursorPack();
                _lastActivated = null;
            }
        }
    }
}