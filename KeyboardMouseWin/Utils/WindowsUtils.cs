using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin.Utils
{
    internal class WindowsUtils
    {
        [DllImport("user32.dll")]
        public static extern nint GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner  
            public int Top;         // y position of upper-left corner  
            public int Right;       // x position of lower-right corner  
            public int Bottom;      // y position of lower-right corner  
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern nint SetWindowPos(nint hWnd, int hWndInsertAfter, int x, int y, int width, int height, int wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(nint hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    }
}
