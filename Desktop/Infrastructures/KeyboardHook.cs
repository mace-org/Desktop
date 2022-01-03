﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Desktop.Infrastructures
{
    public enum KeyboardInputType
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        SysKeyDown = 0x0104,
        SysKeyUp = 0x0105
    }

    public record KeyboardInput(KeyboardInputType Type, Keys VkCode, uint ScanCode, uint Time);

    public class KeyboardHook : Hook<KeyboardInput>
    {
        protected override IDisposable WatchInput(IObserver<KeyboardInput> observer)
        {
            var proc = new NativeApi.HookProc((int nCode, int wParam, IntPtr lParam) =>
            {
                if (nCode >= 0)
                {
                    var input = Marshal.PtrToStructure<NativeApi.KBDLLHOOKSTRUCT>(lParam);
                    var args = new KeyboardInput((KeyboardInputType)wParam, (Keys)input.vkCode, input.scanCode, input.time);
                    observer.OnNext(args);
                }
                return NativeApi.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            });

            //var hModule = Process.GetCurrentProcess().MainModule.BaseAddress;
            var hHook = NativeApi.SetWindowsHookEx(NativeApi.WH_KEYBOARD_LL, proc, IntPtr.Zero, 0);
            Trace.WriteLine($"Set low level keyboard hook: {hHook}.");

            if (hHook == IntPtr.Zero)
            {
                observer.OnError(NativeApi.GetLastException());
                return Disposable.Empty;
            }

            return Disposable.Create(() =>
            {
                var result = NativeApi.UnhookWindowsHookEx(hHook);
                Trace.WriteLine($"Unhook low level keyboard hook({hHook}): {result}.");

                if (result)
                {
                    observer.OnCompleted();
                }
                else
                {
                    observer.OnError(NativeApi.GetLastException());
                }
            });
        }
    }
}