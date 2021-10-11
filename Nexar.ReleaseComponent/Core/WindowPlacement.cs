// Helper to load and save window placement details using application settings.
// If window was closed on a monitor that is now disconnected from the
// computer, it will place the window onto a visible monitor.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct WindowPlacement
{
    public int length;
    public int flags;
    public int showCmd;
    public Point minPosition;
    public Point maxPosition;
    public Rect normalPosition;

    #region Win32 API declarations to set and get window placement

    [DllImport("user32.dll")]
    static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);

    [DllImport("user32.dll")]
    static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);

    const int SwShowNormal = 1;
    const int SwShowMinimized = 2;

    #endregion

    public static void Set(Window window, WindowPlacement value)
    {
        value.length = Marshal.SizeOf(typeof(WindowPlacement));
        value.flags = 0;
        value.showCmd = value.showCmd == SwShowMinimized ? SwShowNormal : value.showCmd;
        SetWindowPlacement(new WindowInteropHelper(window).Handle, ref value);
    }

    public static WindowPlacement Get(Window window)
    {
        GetWindowPlacement(new WindowInteropHelper(window).Handle, out WindowPlacement value);
        return value;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
