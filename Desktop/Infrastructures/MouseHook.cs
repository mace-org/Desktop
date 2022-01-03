using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
    public enum MouseInputType
    {
        MouseMove = 0x0200,
        LeftButtonDown = 0x0201,
        LeftButtonUp = 0x0202,
        RightButtonDown = 0x0204,
        RightButtonUp = 0x0205,
        MouseWheel = 0x020A,
        MouseHWheel = 0x020E
    }

    public record MouseInput(MouseInputType Type, Point Position, uint MouseData, uint Time);

    public class MouseHook : Hook<MouseInput>
    {
        protected override IDisposable WatchInput(IObserver<MouseInput> observer)
        {
            var proc = new NativeApi.HookProc((int nCode, int wParam, IntPtr lParam) =>
            {
                if (nCode >= 0)
                {
                    var input = Marshal.PtrToStructure<NativeApi.MSLLHOOKSTRUCT>(lParam);
                    var args = new MouseInput((MouseInputType)wParam, new Point(input.pt.x, input.pt.y), input.mouseData, input.time);
                    observer.OnNext(args);
                }
                return NativeApi.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            });

            //var hModule = Process.GetCurrentProcess().MainModule.BaseAddress;
            var hHook = NativeApi.SetWindowsHookEx(NativeApi.WH_MOUSE_LL, proc, IntPtr.Zero, 0);
            Trace.WriteLine($"Set low level mouse hook: {hHook}.");

            if (hHook == IntPtr.Zero)
            {
                observer.OnError(NativeApi.GetLastException());
                return Disposable.Empty;
            }

            return Disposable.Create(() =>
            {
                var result = NativeApi.UnhookWindowsHookEx(hHook);
                Trace.WriteLine($"Unhook low level mouse hook({hHook}): {result}.");

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
