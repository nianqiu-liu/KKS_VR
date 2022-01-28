using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine.SceneManagement;

namespace KoikatuVR.Util
{
    internal static class WindowTools
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("User32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        private const int GWL_STYLE = -16;
        private const int WS_THICKFRAME = 0x40000;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const string GET_CLASS_NAME_MAGIC = "UnityWndClass";

        private static IntPtr WindowHandle = IntPtr.Zero;
        private static bool prev = false;

        private static void GetWindowHandle()
        {
            var pid = Process.GetCurrentProcess().Id;
            EnumWindows((w, param) =>
            {
                if (w == IntPtr.Zero) return true;
                if (GetWindowThreadProcessId(w, out uint lpdwProcessId) == 0) return true;
                if (lpdwProcessId != pid) return true;
                var cn = new StringBuilder(256);
                if (GetClassName(w, cn, cn.Capacity) == 0) return true;
                if (cn.ToString() != GET_CLASS_NAME_MAGIC) return true;
                WindowHandle = w;
                return false;
            }, IntPtr.Zero);

            if (WindowHandle == IntPtr.Zero)
                Console.WriteLine("Could not find Unity window handle");
            else
                Console.WriteLine("Found Unity window handle ptr=" + WindowHandle);
        }

        public static void BringWindowToFront()
        {
            if (WindowHandle == IntPtr.Zero) GetWindowHandle();
            SetForegroundWindow(WindowHandle);
        }
    }
}
