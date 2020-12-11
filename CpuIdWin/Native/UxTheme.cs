﻿namespace RJCP.Diagnostics.CpuIdWin.Native
{
    using System;
    using System.Runtime.InteropServices;

    internal static class UxTheme
    {
        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public extern static Int32 SetWindowTheme(IntPtr hWnd, String textSubAppName, String textSubIdList);
    }
}
