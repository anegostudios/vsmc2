using UnityEngine;

namespace Riten.Native.Cursors
{
    public abstract class VirtualCursorBase : ScriptableObject
    {
        public abstract bool isMask { get; set; }
        
        public abstract Color32 backgroundColor { get; set; }
        
        public abstract Color32 foregroundColor { get; set; }
        
        public abstract bool isAnimated { get; set; }
        
        public abstract Texture2D texture { get; set; }
        
        public abstract VirtualCursor[] frames { get; set; }
        
        public abstract int framesPerSecond { get; set; }
        
        public abstract Vector2 hotspot { get; set; }
    }
}