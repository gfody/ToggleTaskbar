using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

class Program
{
    static void Main(string[] args)
    {
        bool vanish = args.Contains("-x"), show = args.Contains("-v"), hide = args.Contains("-h");
        bool toggle = !(show ^ hide);
        var trayWindow = FindWindow("Shell_TrayWnd", null);
        var trayVisible = IsWindowVisible(trayWindow);
        var trayData = new APPBARDATA { cbSize = Marshal.SizeOf(typeof(APPBARDATA)) };
        var trayState = SHAppBarMessage(ABM_GETSTATE, ref trayData);

        if ((toggle | show) && ((trayState & ABS_AUTOHIDE) > 0 || (vanish && !trayVisible)))
        {
            ShowWindow(trayWindow, SW_SHOW);
            trayData.lParam = trayState & ~ABS_AUTOHIDE;
            SHAppBarMessage(ABM_SETSTATE, ref trayData);
        }
        else if ((toggle | hide) && ((trayState & ABS_AUTOHIDE) == 0 || (vanish && trayVisible)))
        {
            trayData.lParam = trayState | ABS_AUTOHIDE;
            SHAppBarMessage(ABM_SETSTATE, ref trayData);
            if (vanish)
                ShowWindow(trayWindow, SW_HIDE);
            else
            {
                GetCursorPos(out var cursor);
                SetForegroundWindow(WindowFromPoint(cursor)); // deactivate taskbar to trigger autohide
            }
        }
    }

    #region winuser.h, shellapi.h
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
    #endregion
}
