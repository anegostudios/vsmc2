using Riten.Native.Cursors.Virtual;
using UnityEngine;

namespace Riten.Native.Cursors
{
    public static class NativeCursor
    {
        static ICursorService _instance;

        private static ICursorService _defaultService;
        private static VirtualCursorService _vcs;
        
        public static string ServiceName => _instance == null ? "NULL" : _instance.GetType().Name;
        
        public static void SetFallbackService(ICursorService service)
        {
            _defaultService = service;
        }
        
        /// <summary>
        /// Set custom cursor service.
        /// You should not need to call this method.
        /// But you can!
        /// </summary>
        public static void SetService(ICursorService service)
        {
            if (_instance == service) 
                return;
            
            _instance?.ResetCursor();
            _instance = service;
            _instance?.SetCursor(NTCursors.Default);
        }
        
        public static bool SetCursor(NTCursors ntCursor)
        {
            return _instance != null && _instance.SetCursor(ntCursor);
        }
        
        /// <summary>
        /// This method uses a virtual cursor pack to set the cursor.
        /// </summary>
        public static void SetCursorPack(CursorPack cursorPack, Camera cmr)
        {
            if (cursorPack == null)
            {
                SetService(_defaultService);
                return;
            }
            
            if (!_vcs)
            {
                var go = new GameObject("VirtualCursorService")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                
                Object.DontDestroyOnLoad(go);
                
                _vcs = go.AddComponent<VirtualCursorService>();
            }
            
            _vcs.UpdatePack(cursorPack, cmr);
            
            SetService(_vcs);
        }
        
        public static void SetCursorPackCamera(Camera cmr)
        {
            if (_vcs)
                _vcs.SetCamera(cmr);
        }

        public static void ClearCursorPack()
        {
            SetCursorPack(null, null);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        
        /// <summary>
        /// Reset cursor to default.
        /// This is safer than setting the cursor to NTCursors.Arrow.
        /// </summary>
        public static void ResetCursor()
        {
            _instance?.ResetCursor();
        }
    }
}
