using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BetterColorPicker
{
    public static class MouseColour
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(IntPtr window, IntPtr dc);
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private static Color GetColorAt(int x, int y)
        {
            var desk = GetDesktopWindow();
            var dc = GetWindowDC(desk);
            var a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return new Color(((a >> 0) & 0xff) / 255f, ((a >> 8) & 0xff) / 255f, ((a >> 16) & 0xff) / 255f);
        }

        public static Color Get()
        {
            GetCursorPos(out POINT cursorPos);
            return GetColorAt(cursorPos.X, cursorPos.Y);
        }
    }
}