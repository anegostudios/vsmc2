#if !UNITY_EDITOR && UNITY_STANDALONE_WIN

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Riten.Native.Cursors
{
    internal class WindowsCursorPatch : MonoBehaviour, ICursorService
    {
        private const uint IDC_ARROW = 32512;        // Normal select
        private const uint IDC_IBEAM = 32513;        // Text select
        private const uint IDC_WAIT = 32650;         // Busy
        private const uint IDC_CROSS = 32515;        // Precision select
        private const uint IDC_SIZENWSE = 32642;     // Diagonal resize 1
        private const uint IDC_SIZENESW = 32643;     // Diagonal resize 2
        private const uint IDC_SIZEWE = 32644;       // Horizontal resize
        private const uint IDC_SIZENS = 32645;       // Vertical resize
        private const uint IDC_SIZEALL = 32646;      // Move
        private const uint IDC_NO = 32648;           // Unavailable
        private const uint IDC_HAND = 32649;         // Link select
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            var go = new GameObject("NativeCursor#WindowsCursorService")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            DontDestroyOnLoad(go);

            var service = go.AddComponent<WindowsCursorPatch>();

            NativeCursor.SetFallbackService(service);
            NativeCursor.SetService(service);
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr LoadCursor(IntPtr hInstance, uint lpCursorName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", EntryPoint = "CallWindowProcA")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint wMsg, IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcA")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        private static HandleRef hMainWindow;
        private static IntPtr unityWndProcHandler, customWndProcHandler;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private static WndProcDelegate procDelegate;

        private const int GWLP_WNDPROC = -4;
        private const uint WM_SETCURSOR = 0x0020;
        private const uint WM_MOUSEMOVE = 0x0200;

        private static IntPtr cursorHandle;

        static readonly Dictionary<NTCursors, IntPtr> _cursors = new ();

        void Awake()
        {
            cursorHandle = LoadCursor(IntPtr.Zero, IDC_HAND);

            hMainWindow = new HandleRef(null, GetActiveWindow());
            procDelegate = WndProc;
            customWndProcHandler = Marshal.GetFunctionPointerForDelegate(procDelegate);
            unityWndProcHandler = SetWindowLongPtr(hMainWindow, GWLP_WNDPROC, customWndProcHandler);
        }

        void OnDestroy()
        {
            SetWindowLongPtr(hMainWindow, GWLP_WNDPROC, unityWndProcHandler);
            hMainWindow = new HandleRef(null, IntPtr.Zero);
            unityWndProcHandler = IntPtr.Zero;
            customWndProcHandler = IntPtr.Zero;
            procDelegate = null;
        }

        [AOT.MonoPInvokeCallback(typeof(WndProcDelegate))]
        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg != WM_SETCURSOR && msg != WM_MOUSEMOVE)
                return CallWindowProc(unityWndProcHandler, hWnd, msg, wParam, lParam);

            SetCursor(cursorHandle);
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        static IntPtr GetCursor(NTCursors nativeCursorName)
        {
            if (_cursors.TryGetValue(nativeCursorName, out var cursor))
                return cursor;
            
            cursor = LoadCursor(IntPtr.Zero, nativeCursorName switch
            {
                NTCursors.Default => IDC_ARROW,
                NTCursors.Arrow => IDC_ARROW,
                NTCursors.IBeam => IDC_IBEAM,
                NTCursors.Crosshair => IDC_CROSS,
                NTCursors.Link => IDC_HAND,
                NTCursors.ResizeVertical => IDC_SIZENS,
                NTCursors.ResizeHorizontal => IDC_SIZEWE,
                NTCursors.ResizeDiagonalLeft => IDC_SIZENWSE,
                NTCursors.ResizeDiagonalRight => IDC_SIZENESW,
                NTCursors.ResizeAll => IDC_SIZEALL,
                NTCursors.Busy => IDC_WAIT,
                NTCursors.Invalid => IDC_NO,
                
                NTCursors.OpenHand => 32516,
                NTCursors.ClosedHand => IDC_HAND,
                
                _ => throw new ArgumentOutOfRangeException(nameof(nativeCursorName), nativeCursorName, null)
            });
            
            _cursors.Add(nativeCursorName, cursor);
            return cursor;
        }

        public bool SetCursor(NTCursors nativeCursorName)
        {
            var arrowCursorHandle = GetCursor(nativeCursorName);

            if (arrowCursorHandle == IntPtr.Zero)
                return false;

            cursorHandle = arrowCursorHandle;
            SetCursor(arrowCursorHandle);
            return true;
        }

        public void ResetCursor()
        {
            SetCursor(NTCursors.Default);
        }
    }
}

#endif