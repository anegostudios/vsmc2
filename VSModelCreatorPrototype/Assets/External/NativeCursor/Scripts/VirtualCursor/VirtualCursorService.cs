using UnityEngine;

namespace Riten.Native.Cursors.Virtual
{
    public class VirtualCursorService : MonoBehaviour, ICursorService
    {
        private CursorPack _cursorPack;
        
        private VirtualCursorBase _activeCursor;
        private Camera _camera;

        private int _lastFrame;
        private int _frame;
        private float _fps;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void OnEnable()
        {
            Camera.onPostRender += OnPostRenderCb;
        }
        
        private void OnDisable()
        {
            Camera.onPostRender -= OnPostRenderCb;
        }

        private void OnPostRenderCb(Camera cmr)
        {
            if (!Application.isPlaying) return;
            
            if (cmr != _camera) return;

            if (_activeCursor.isMask)
            {
                DoMaskedPostProcess();
            }
        }

        public void UpdatePack(CursorPack pack, Camera cmr)
        {
            if (cmr) _camera = cmr;
            _cursorPack = pack;
        }
        
        public bool SetCursor(NTCursors ntCursor)
        {
            _activeCursor = _cursorPack.GetCursor(ntCursor);

            if (_activeCursor == null)
                return false;
            
            _frame = 0;
            _fps = Mathf.Abs(_activeCursor.framesPerSecond == 0 ? 0 : 1f / _activeCursor.framesPerSecond);

            DoCursorUpdate();
            return true;
        }

        public void ResetCursor()
        {
            SetCursor(NTCursors.Arrow);
        }

        void Update()
        {
            if (!_activeCursor || !_activeCursor.isAnimated || _activeCursor.frames.Length == 0) 
                return;
            
            _frame = Mathf.RoundToInt(_fps == 0f ? 0 : Time.time / _fps) % _activeCursor.frames.Length;

            if (_lastFrame != _frame)
            {
                _lastFrame = _frame;
                DoCursorUpdate();
            }
        }

        private void DoCursorUpdate()
        {
            if (!_activeCursor) return;

            if (_activeCursor.isAnimated && _activeCursor.frames.Length > 0)
            {
                var cursor = _activeCursor.frames[_frame];
                var texture = cursor.texture;
                
                Cursor.SetCursor(
                    texture,
                    new Vector2(
                        cursor.hotspot.x * texture.width, 
                        cursor.hotspot.y * texture.height
                    ),
                    CursorMode.Auto
                );
            }
            else
            {
                var texture = _activeCursor.texture;
                
                Cursor.SetCursor(
                    _activeCursor.texture,
                    new Vector2(texture.width, texture.height) * _activeCursor.hotspot,
                    CursorMode.Auto
                );
            }
        }

        private Texture2D _screenTexture;
        private Texture2D _maskTexture;

        private void CaptureScreen()
        {
            _screenTexture ??= new Texture2D(_maskTexture.width, _maskTexture.height, TextureFormat.RGBA32, false);
            
            if (_screenTexture.width != _maskTexture.width || _screenTexture.height != _maskTexture.height)
                _screenTexture.Reinitialize(_maskTexture.width, _maskTexture.height, TextureFormat.RGBA32, false);

            var pos = Input.mousePosition;
            var hot = _activeCursor.hotspot * new Vector2(_maskTexture.width, _maskTexture.height);
            var region = new Rect(pos.x - hot.x, Screen.height - pos.y - hot.y, _screenTexture.width, _screenTexture.height);
            
            if (region.x + region.width > Screen.width)
                region.x = Screen.width - region.width;
            
            if (region.y + region.height > Screen.height)
                region.y = Screen.height - region.height;
            
            if (region.x < 0)
                region.x = 0;
            
            if (region.y < 0)
                region.y = 0;
            
            _screenTexture.ReadPixels(region, 0, 0, false);
            _screenTexture.Apply();
        }

        private void DoMaskedPostProcess()
        {
            var texture = _activeCursor.texture;
            var pixels = texture.GetPixels32();

            if (!_maskTexture)
            {
                _maskTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            }
            else if (_maskTexture.width != texture.width || _maskTexture.height != texture.height)
            {
                _maskTexture.Reinitialize(texture.width, texture.height, TextureFormat.RGBA32, false);
            }
            
            CaptureScreen();
            
            var screen = _screenTexture.GetPixels32();

            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];

                if (pixel.a == 255)
                {
                    var screenPixel = screen[i];
                    screenPixel.r = (byte)(255 - screenPixel.r);
                    screenPixel.g = (byte)(255 - screenPixel.g);
                    screenPixel.b = (byte)(255 - screenPixel.b);
                    pixels[i] = screenPixel;
                }
            }
            
            _maskTexture.SetPixels32(pixels);
            _maskTexture.Apply();
            
            Cursor.SetCursor(
                _maskTexture,
                new Vector2(
                    _activeCursor.hotspot.x * _maskTexture.width, 
                    _activeCursor.hotspot.y * _maskTexture.height
                ),
                CursorMode.ForceSoftware
            );
        }

        public void SetCamera(Camera cmr)
        {
            _camera = cmr;
        }
    }
}