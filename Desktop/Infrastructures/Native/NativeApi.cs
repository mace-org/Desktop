using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures.Native
{
    public static class NativeApi
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;

        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        [DllImport("User32")]
        public extern static IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hmod, uint dwThreadId);

        [DllImport("User32")]
        public extern static int CallNextHookEx(IntPtr hhk, int nCode, int wParam, IntPtr lParam);

        [DllImport("User32")]
        public extern static bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("Kernel32.dll")]
        public extern static uint GetLastError();

        [DllImport("Kernel32")]
        public extern static uint GetCurrentProcessId();

        [DllImport("Kernel32.dll")]
        public extern static IntPtr GetModuleHandle(string lpModuleName);

        public static Win32Exception GetLastException()
        {
            return new Win32Exception((int)GetLastError());
        }
    }
}
