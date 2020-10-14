using System;
using System.Drawing;
using System.Runtime.InteropServices;

class Program
{
    [StructLayout(LayoutKind.Sequential)]
    struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public Rectangle rc;
        public int lParam;
    }

    const int ABS_AUTOHIDE = 1;
    const int ABS_ALWAYSONTOP = 2;
    const int ABM_GETSTATE = 4;
    const int ABM_SETSTATE = 10;
    const int SW_HIDE = 0;
    const int SW_SHOW = 5;

    [DllImport("shell32")]
    static extern int SHAppBarMessage(int msg, ref APPBARDATA data);
    [DllImport("user32")]
    static extern IntPtr SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32")]
    static extern bool GetCursorPos(out Point lpPoint);
    [DllImport("user32")]
    static extern IntPtr WindowFromPoint(Point lpPoint);
    [DllImport("user32")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    static void Main(string[] args)
    {
        GetCursorPos(out var cursor);
        var someWindow = WindowFromPoint(cursor);
        var trayWindow = FindWindow("Shell_TrayWnd", null);
        if (args.Length > 0 && !IsWindowVisible(trayWindow))
            ShowWindow(trayWindow, SW_SHOW);

        var data = new APPBARDATA { cbSize = Marshal.SizeOf(typeof(APPBARDATA)) };
        data.lParam = SHAppBarMessage(ABM_GETSTATE, ref data) == ABS_AUTOHIDE ? ABS_ALWAYSONTOP : ABS_AUTOHIDE;
        SHAppBarMessage(ABM_SETSTATE, ref data);

        if (data.lParam == ABS_AUTOHIDE)
        {
            if (args.Length > 0)
                ShowWindow(trayWindow, SW_HIDE);
            else
                SetForegroundWindow(someWindow); // deactivate taskbar to trigger autohide
        }
    }
}
