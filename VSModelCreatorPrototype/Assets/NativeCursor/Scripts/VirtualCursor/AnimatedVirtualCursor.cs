using UnityEngine;

namespace Riten.Native.Cursors
{
    [CreateAssetMenu(fileName = "AnimatedVirtualCursor", menuName = "Native Cursor/Animated Virtual Cursor")]
    public class AnimatedVirtualCursor : VirtualCursorBase
    {
        [field: SerializeField]
        public override VirtualCursor[] frames { get; set; }
        
        [Tooltip("Frames per second")]
        public int fps = 30;

        public override int framesPerSecond { get => fps; set => fps = value; }
        public override Vector2 hotspot { get => default; set {} }
        public override bool isMask { get => default; set {} }
        public override Color32 backgroundColor { get => default; set {} }
        public override Color32 foregroundColor { get => default; set {} }
        public override bool isAnimated { get => true; set {} }
        public override Texture2D texture { get => default; set {} }
    }
}