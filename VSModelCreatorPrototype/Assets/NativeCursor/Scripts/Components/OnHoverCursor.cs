using UnityEngine;
using UnityEngine.EventSystems;

namespace Riten.Native.Cursors.UI
{
    public class OnHoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private NTCursors _cursor;
        [Tooltip("Higher priority means this cursor will override other cursors with lower priority")]
        [SerializeField] private int _priority = -2;

        public NTCursors Cursor
        {
            get => _cursor;
            set
            {
                if (_cursor == value) return;

                if (_isHovering)
                    CursorStack.Replace(_pushedId, value);
                
                _cursor = value;
            }
        }
        
        private bool _isHovering;

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

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            _pushedId = CursorStack.Push(
                _cursor, 
                _priority, 
                transform.GetSecondaryPriority(_transformDepth)
            );
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            CursorStack.Pop(_pushedId);
        }
    }
}