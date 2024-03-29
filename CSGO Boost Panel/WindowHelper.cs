﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using static CSGO_Boost_Panel.MainWindow;

namespace CSGO_Boost_Panel
{
    public class CSGOCoefficients
    {
        public static readonly CSGOCoefficients GO = new CSGOCoefficients { XCoefficient = 3.36, YCoefficient = 22.85, RectPosX = 2, RectPosY = 2};
        public static readonly CSGOCoefficients Play = new CSGOCoefficients { XCoefficient = 29.52, YCoefficient = 7.35, RectPosX = 1, RectPosY = 1 };
        public static readonly CSGOCoefficients Reconnect = new CSGOCoefficients { XCoefficient = 1.348, YCoefficient = 16.818181, RectPosX = 1, RectPosY = 1 };
        public static readonly CSGOCoefficients Accept = new CSGOCoefficients { XCoefficient = 1.9948553, YCoefficient = 1.775648, RectPosX = 1, RectPosY = 1 };
        public static readonly CSGOCoefficients RightMenuButtons = new CSGOCoefficients { XCoefficient = 30.476, YCoefficient = 4.285, RectPosX = 2, RectPosY = 1 };
        public static readonly CSGOCoefficients AddFriend = new CSGOCoefficients { XCoefficient = 4.353, YCoefficient = 3.404, RectPosX = 2, RectPosY = 1 };
        public static readonly CSGOCoefficients FriendCodeField = new CSGOCoefficients { XCoefficient = 2.310, YCoefficient = 2.060, RectPosX = 1, RectPosY = 1 };
        public static readonly CSGOCoefficients CopyCode = new CSGOCoefficients { XCoefficient = 2.154, YCoefficient = 2.364, RectPosX = 2, RectPosY = 2 };
        public static readonly CSGOCoefficients СheckMark = new CSGOCoefficients { XCoefficient = 2.245, YCoefficient = 2.060, RectPosX = 2, RectPosY = 1 };
        public static readonly CSGOCoefficients PlayerField = new CSGOCoefficients { XCoefficient = 2.452, YCoefficient = 2.096, RectPosX = 1, RectPosY = 2 };
        public static readonly CSGOCoefficients Invite = new CSGOCoefficients { XCoefficient = 3.855, YCoefficient = 2.096, RectPosX = 2, RectPosY = 2 };
        public static readonly CSGOCoefficients Cancel = new CSGOCoefficients { XCoefficient = 2.7, YCoefficient = 2.594, RectPosX = 2, RectPosY = 2 };
        public static readonly CSGOCoefficients RightInvitesMenu = new CSGOCoefficients { XCoefficient = 30.476, YCoefficient = 3.779, RectPosX = 2, RectPosY = 1 };
        public static readonly CSGOCoefficients Invitation = new CSGOCoefficients { XCoefficient = 5.423, YCoefficient = 3.809, RectPosX = 2, RectPosY = 1 };
        public double XCoefficient { get; private set; }
        public double YCoefficient { get; private set; }
        public short RectPosX { get; private set; }
        public short RectPosY { get; private set; }
    }

    public class WindowHelper
    {
        static readonly IntPtr zero = IntPtr.Zero;
        [DllImport("User32.DLL")]
        static extern int PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter,
            string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);
        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr handle);

        static IntPtr MakeLParam(int x, int y) => (IntPtr)((y << 16) | (x & 0xFFFF));


        const int WM_LBUTTONUP = 0x202;
        const int WM_LBUTTONDOWN = 0x201;
        const int WM_MOUSEMOVE = 0x0200;
        const int MK_LBUTTON = 0x0001;
        const int WM_CHAR = 0x0102;
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int VK_RETURN = 0x0D;
        const int ALT = 0xA4;
        const int EXTENDEDKEY = 0x1;
        const int KEYUP = 0x2;
        public const int VK_MENU = 0x12;
        public const int VK_ESCAPE = 0x1B;
        public const int VK_F10 = 0x79; 

        public static bool IsExist(string WinTitle)
        {
            return FindWindowEx(zero, zero, null, WinTitle) != zero;
        }

        /// <summary>
        /// Simulate mouse click in a certain place of window.
        /// </summary>
        /// <param name="MouseMove">Is it mouse move or it is just click</param>
        /// <param name="Coefficient">Use value from CSGOCoefficients class</param>
        public static void Click(string WinTitle, CSGOCoefficients Coefficient, bool MouseMove = false)
        {
            Rect WindowRect = new Rect();
            IntPtr WindowHWND = FindWindowEx(zero, zero, null, WinTitle);
            if (IsIconic(WindowHWND))
                ShowWindow(WindowHWND, 4);
            if (ProgramSettings.FocusWindows)
            {
                keybd_event((byte)ALT, 0x45, EXTENDEDKEY | 0, 0);
                keybd_event((byte)ALT, 0x45, EXTENDEDKEY | KEYUP, 0);
                SetForegroundWindow(WindowHWND);
            }
            GetClientRect(WindowHWND, ref WindowRect);
            int CoordX = Convert.ToInt16(WindowRect.Right / Coefficient.XCoefficient);
            if (Coefficient.RectPosX == 2)
                CoordX = WindowRect.Right - CoordX;
            int CoordY = Convert.ToInt16(WindowRect.Bottom / Coefficient.YCoefficient);
            if (Coefficient.RectPosY == 2)
                CoordY = WindowRect.Bottom - CoordY;
            IntPtr lParam = MakeLParam(CoordX, CoordY);
            PostMessage(WindowHWND, WM_MOUSEMOVE, zero, lParam);
            if (MouseMove)
                return;
            PostMessage(WindowHWND, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, lParam);
            PostMessage(WindowHWND, WM_LBUTTONUP, zero, lParam);
        }

        public static void SendText(string WinTitle, string text)
        {
            IntPtr WindowHWND = FindWindowEx(zero, zero, null, WinTitle);
            foreach (char c in text)
            {
                IntPtr lParam = new IntPtr(c);
                PostMessage(WindowHWND, WM_CHAR, lParam, zero);
            }
            PostMessage(WindowHWND, WM_KEYDOWN, (IntPtr)VK_RETURN, zero);
            PostMessage(WindowHWND, WM_KEYUP, (IntPtr)VK_RETURN, zero);
        }
        public static void SendKey(string WinTitle, int key)
        {
            IntPtr lParam = IntPtr.Zero;
            if (key == VK_F10)
                lParam = (IntPtr)0x00440001;
            IntPtr WindowHWND = FindWindowEx(zero, zero, null, WinTitle);
            PostMessage(WindowHWND, WM_KEYDOWN, (IntPtr)key, lParam);
            PostMessage(WindowHWND, WM_KEYUP, (IntPtr)key, lParam);
        }

        public static void GetRect(string WinTitle, ref Rect WindowRect)
        {
            IntPtr WindowHWND = FindWindowEx(zero, zero, null, WinTitle);
            GetWindowRect(WindowHWND, ref WindowRect);
        }

        public static void EnableWindow(string WinTitle, bool enable)
        {
            IntPtr WindowHWND = FindWindowEx(zero, zero, null, WinTitle);
            EnableWindow(WindowHWND, enable);
        }
    }
}