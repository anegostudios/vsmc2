using UnityEditor;
using UnityEngine;

namespace Riten.Native.Cursors.Editor
{
    [CustomEditor(typeof(VirtualCursor))]
    public class VirtualCursorInspector : UnityEditor.Editor
    {
        VirtualCursor _target;
        Texture2D _originalTexture;
        Texture2D _adaptedTexture;
        
        bool _dragging;

        private void RefreshTexture()
        {
            if (_originalTexture == _target.texture) return;
            
            _originalTexture = _target.texture;

            if (!_originalTexture)
            {
                _adaptedTexture = null;
                return;
            }
            
            _adaptedTexture = new Texture2D(_originalTexture.width, _originalTexture.height, TextureFormat.RGBA32, false);
            _adaptedTexture.SetPixels(_originalTexture.GetPixels());
            _adaptedTexture.filterMode = FilterMode.Point;
            _adaptedTexture.Apply();
        }
        
        private void OnEnable()
        {
            _target = (VirtualCursor)target;
            RefreshTexture();
        }

        void DragHotspot(Rect texture)
        {
            var uv = new Vector2(
                (Event.current.mousePosition.x - texture.x) / texture.width,
                (Event.current.mousePosition.y - texture.y - 105) / texture.height
            );
            
            uv.x = Mathf.Clamp(uv.x, 0f, 1f);
            uv.y = Mathf.Clamp(uv.y, 0f, 1f);
            
            int pixelX = Mathf.FloorToInt(uv.x * _adaptedTexture.width);
            int pixelY = Mathf.FloorToInt(uv.y * _adaptedTexture.height);
            
            pixelX = Mathf.Clamp(pixelX, 0, _adaptedTexture.width - 1);
            pixelY = Mathf.Clamp(pixelY, 0, _adaptedTexture.height - 1);
            
            uv.x = pixelX / (float)_adaptedTexture.width;
            uv.y = pixelY / (float)_adaptedTexture.height;
            
            _target.hotspot = uv;
        }

        public override void OnInspectorGUI()
        {
            if (!_target) return;

            var width = Screen.width * 0.8f;

            GUILayout.Label("Preview", EditorStyles.boldLabel);
            
            if (_target.isMask)
            {
                GUILayout.Label("This cursor is a mask");
            }
            else
            {
                var text = (Texture2D)EditorGUILayout.ObjectField("Texture", _target.texture, typeof(Texture2D), false);

                if (text != _target.texture)
                {
                    Undo.RecordObject(_target, "Change texture");
                    _target.texture = text;
                    EditorUtility.SetDirty(_target);
                }
            }

            var newHp = EditorGUILayout.Vector2Field("Hotspot", _target.hotspot);
            
            if (!_dragging && newHp != _target.hotspot)
            {
                Undo.RecordObject(_target, "Change hotspot");
                _target.hotspot = newHp;
                EditorUtility.SetDirty(_target);
            }
            
            if (_target.isMask)
            {
                var newBackColor = EditorGUILayout.ColorField("Background Color", _target.backgroundColor);

                if (newBackColor != _target.backgroundColor)
                {
                    Undo.RecordObject(_target, "Change background color");
                    _target.backgroundColor = newBackColor;
                    EditorUtility.SetDirty(_target);
                }

                var newForeColor = EditorGUILayout.ColorField("Foreground Color", _target.foregroundColor);

                if (newForeColor != _target.foregroundColor)
                {
                    Undo.RecordObject(_target, "Change foreground color");
                    _target.foregroundColor = newForeColor;
                    EditorUtility.SetDirty(_target);
                }
            }

            RefreshTexture();

            if (!_target.texture)
            {
                GUILayout.Label("No texture selected");
                return;
            }
            
            var textureRect = GUILayoutUtility.GetRect(width, width);
            textureRect.x = 0;
            textureRect.width = width;
            textureRect.height = width;

            GUI.DrawTexture(textureRect, _adaptedTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(
                width * 0.5f - _adaptedTexture.width * 0.5f, 
                width + textureRect.y,
                _adaptedTexture.width, 
                _adaptedTexture.height), 
                _adaptedTexture
            );

            float onePixel = width / (float)_adaptedTexture.width;
            
            var arcRect = new Rect(
                width * _target.hotspot.x, 
                width * _target.hotspot.y + textureRect.y, 
                onePixel, 
                onePixel);
            
            float arcRadius = arcRect.width * 0.5f;
            
            if (arcRect.Contains(Event.current.mousePosition))
            {
                if (!_dragging)
                {
                    Handles.color = Color.green;
                    Handles.DrawSolidArc(
                        new Vector3(arcRect.x + arcRadius, arcRect.y + arcRadius),
                        Vector3.forward,
                        Vector3.up,
                        360,
                        arcRadius * 2f
                    );
                }

                if (!_dragging && Event.current.type == EventType.MouseDown)
                {
                    _dragging = true;
                    Undo.RecordObject(_target, "Change hotspot");
                }
            }

            if (_dragging && Event.current.type == EventType.MouseUp)
            {
                _dragging = false;
                EditorUtility.SetDirty(_target);
            }

            Handles.color = Color.red;
            Handles.DrawSolidArc(
                new Vector3(arcRect.x + arcRadius, arcRect.y + arcRadius), 
                Vector3.forward, 
                Vector3.up, 
                360, 
                arcRadius * .5f
            );

            if (_dragging)
                DragHotspot(textureRect);

            GUILayout.Space(_adaptedTexture.width * 5);

            Repaint();
        }
    }
}