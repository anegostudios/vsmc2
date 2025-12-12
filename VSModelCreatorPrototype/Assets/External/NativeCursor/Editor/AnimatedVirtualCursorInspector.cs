using System;
using UnityEditor;
using UnityEngine;

namespace Riten.Native.Cursors.Editor
{
    [CustomEditor(typeof(AnimatedVirtualCursor))]
    public class AnimatedVirtualCursorInspector : UnityEditor.Editor
    {
        private AnimatedVirtualCursor _value;
        
        private void OnEnable()
        {
            _value = (AnimatedVirtualCursor)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (_value.frames != null && _value.frames.Length > 0)
            {
                float time = Time.realtimeSinceStartup;

                float framesPerSecond = 1f / _value.fps;
                var frameId = Mathf.FloorToInt(time / framesPerSecond) % _value.frames.Length;

                GUILayout.Label(_value.frames[frameId].texture);

                Repaint();
            }
        }
    }
}