using UnityEngine;
using UnityEngine.EventSystems;

namespace Riten.Native.Cursors.UI
{
    public class OnPressCursor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private NTCursors _cursor;
        [Tooltip("Higher priority means this cursor will override other cursors with lower priority")]
        [SerializeField] private int _priority = -1;

        public NTCursors Cursor
        {
            get => _cursor;
            set
            {
                if (_cursor == value) return;

                if (_isPressing)
                    CursorStack.Replace(_pushedId, value);
                
                _cursor = value;
            }
        }

        private bool _isPressing;
        
        private int _pushedId;

        private int _transformDepth;
        
        private void Awake()
        {
            _transformDepth = transform.CalculateTransformDepth();
        }

        private void OnTransformParentChanged()
        {
            _transformDepth = transform.CalculateTransformDepth();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressing = true;
            _pushedId = CursorStack.Push(_cursor, _priority, transform.GetSecondaryPriority(_transformDepth));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressing = false;
            CursorStack.Pop(_pushedId);
        }
    }
}