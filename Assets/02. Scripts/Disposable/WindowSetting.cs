#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowSetting
{
    #region WIN32API
    public static readonly System.IntPtr HWND_TOPMOST = new System.IntPtr(-1);
    public static readonly System.IntPtr HWND_NOT_TOPMOST = new System.IntPtr(-2);
    public static readonly UInt32 SWP_NOSIZE = 0x0001;
    public static readonly UInt32 SWP_NOMOVE = 0x0002;
    public static readonly UInt32 SWP_SHOWWINDOW = 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int X
        {
            get
            {
                return Left;
            }
            set
            {
                Right -= (Left - value);
                Left = value;
            }
        }

        public int Y
        {
            get
            {
                return Top;
            }
            set
            {
                Bottom -= (Top - value);
                Top = value;
            }
        }

        public int Height
        {
            get
            {
                return Bottom - Top;
            }
            set
            {
                Bottom = value + Top;
            }
        }

        public int Width
        {
            get
            {
                return Right - Left;
            }
            set
            {
                Right = value + Left;
            }
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern System.IntPtr FindWindow(String lpClassName, String lpWindowName);

    [DllImport("User32.dll")]
    public static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(System.IntPtr hWnd, System.IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    public static extern long GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    public static extern long SetWindowLong(IntPtr hwnd, int index, long newLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    #endregion   


    // Use this for initialization
    void Start()
    {
        //AssignTopmostWindow("PanoVideoMulti", true);
    }

    public static void HideWindowTitleBar(string WindowTitle)
    {
        IntPtr windowPtr = FindWindow(null, WindowTitle);
        long styleValue = GetWindowLong(windowPtr, -16);

        styleValue &= ~0x00800000L;
        styleValue &= ~0x00400000L;

        SetWindowLong(windowPtr, -16, styleValue);
    }

    public static void AssignTopmostWindow(string WindowTitle, bool MakeTopmost , bool HideTitleBar)
    {
        Cursor.visible = false;

        IntPtr hWnd = FindWindow(null, Application.productName);
        UnityEngine.Debug.Log("Assigning top most flag to window of title: " + WindowTitle);

        if (HideTitleBar)
        {
            long styleValue = GetWindowLong(hWnd, -16);

            styleValue &= ~0x00800000L;
            styleValue &= ~0x00400000L;

            SetWindowLong(hWnd, -16, styleValue);

            SetWindowPos(hWnd, MakeTopmost ? HWND_TOPMOST : HWND_NOT_TOPMOST,
                                0, 0, 2160, 3840,  SWP_SHOWWINDOW);
        }
        else
        {
            SetWindowPos(hWnd, MakeTopmost ? HWND_TOPMOST : HWND_NOT_TOPMOST, 
                                0, 0, 2160, 3840,  SWP_SHOWWINDOW);
        }
    }

    private string[] GetWindowTitles()
    {
        List<string> WindowList = new List<string>();

        Process[] ProcessArray = Process.GetProcesses();
        foreach (Process p in ProcessArray)
        {
            if (!IsNullOrWhitespace(p.MainWindowTitle))
            {
                WindowList.Add(p.MainWindowTitle);
            }
        }

        return WindowList.ToArray();
    }

    public bool IsNullOrWhitespace(string Str)
    {
        if (Str.Equals("null"))
        {
            return true;
        }
        foreach (char c in Str)
        {
            if (c != ' ')
            {
                return false;
            }
        }
        return true;
    }
}
#endif