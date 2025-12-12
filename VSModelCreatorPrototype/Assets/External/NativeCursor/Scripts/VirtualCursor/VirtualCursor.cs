using System;
using UnityEngine;

namespace Riten.Native.Cursors
{
    [CreateAssetMenu(fileName = "VirtualCursor", menuName = "Native Cursor/Virtual Cursor")]
    public class VirtualCursor : VirtualCursorBase
    {
        [field: SerializeField] public override bool isMask { get; set; }

        [field: SerializeField] public override Texture2D texture  { get; set; }

        [Tooltip("Hotspot of the cursor in uv coordinates [0-1]")]
        [field: SerializeField] public override Vector2 hotspot { get; set; }
        
        [field: SerializeField] public override Color32 backgroundColor  { get; set; }
        
        [field: SerializeField] public override Color32 foregroundColor  { get; set; }
        
        public override bool isAnimated
        {
            get => false;
            set {} 
        }

        public override VirtualCursor[] frames
        {
            get => Array.Empty<VirtualCursor>();
            set{}
        }
        
        public override int framesPerSecond
        {
            get => default; 
            set{} 
        }
    }
}
