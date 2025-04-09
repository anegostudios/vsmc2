using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace Riten.Native.Cursors.Editor
{
    [HarmonyPatch(typeof(EditorGUIUtility), "Internal_AddCursorRect", typeof(Rect), typeof(MouseCursor), typeof(int))]
    internal static class EditorCursorPatch
    {
        public static MouseCursor? targetCursor;
        
        public static void Prefix(Rect r, ref MouseCursor m, int controlID)
        {
            if (m == MouseCursor.CustomCursor && targetCursor != null)
                m = targetCursor.Value;
        }
    }
    
    public class EditorCursorService : ICursorService
    {
        Harmony _harmony;

        private void OnEnable()
        {
            _harmony = new Harmony("com.riten.nativecursor");
            
            try
            {
                _harmony.PatchAll();
            }
            catch
            {
                var macos = Resources.Load<CursorPack>("CursorPacks/MacOS");

                NativeCursor.SetCursorPack(macos, Camera.main);
            }
        }

        private void OnDisable()
        {
            _harmony?.UnpatchAll();
            _harmony = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            var service = new EditorCursorService();
            
            NativeCursor.SetFallbackService(service);
            NativeCursor.SetService(service);
            
            service.OnEnable();

            EditorApplication.playModeStateChanged += service.OnPlayModeStateChanged;
        }
        
        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            
            if (state == PlayModeStateChange.ExitingPlayMode)
                OnDisable();
        }

        public bool SetCursor(NTCursors ntCursorName)
        {
            EditorCursorPatch.targetCursor = ntCursorName switch
            {
                NTCursors.Default => null,
                NTCursors.Arrow => MouseCursor.Arrow,
                NTCursors.IBeam => MouseCursor.Text,
                NTCursors.Crosshair => MouseCursor.ArrowPlus,
                NTCursors.Link => MouseCursor.Link,
                NTCursors.ResizeVertical => MouseCursor.ResizeVertical,
                NTCursors.ResizeHorizontal => MouseCursor.ResizeHorizontal,
                NTCursors.Busy => MouseCursor.RotateArrow,
                NTCursors.Invalid => MouseCursor.ArrowMinus,
                NTCursors.ResizeDiagonalLeft => MouseCursor.ResizeUpLeft,
                NTCursors.ResizeDiagonalRight => MouseCursor.ResizeUpRight,
                NTCursors.ResizeAll => MouseCursor.ScaleArrow,
                NTCursors.OpenHand => MouseCursor.Pan,
                NTCursors.ClosedHand => MouseCursor.Pan,
                _ => null
            };
            
            return true;
        }

        public void ResetCursor()
        {
            EditorCursorPatch.targetCursor = null;
        }
    }
}
