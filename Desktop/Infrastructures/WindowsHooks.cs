using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

    public class WindowsHooks : IDisposable
    {
        private CompositeDisposable _disposables = new CompositeDisposable();

        public WindowsHooks()
        {
            KeyboardInputs = Observable.Create<KeyboardInput>(WatchKeyboard).Publish().RefCount();
        }

        private IDisposable WatchKeyboard(IObserver<KeyboardInput> observer)
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

            var hHook = NativeApi.SetWindowsHookEx(NativeApi.WH_KEYBOARD_LL, proc, IntPtr.Zero, 0);
            Trace.WriteLine($"Set low level keyboard hook: {hHook}.");

            if (hHook == IntPtr.Zero)
            {
                observer.OnError(NativeApi.GetLastException());
                return Disposable.Empty;
            }

            var disposable = Disposable.Create(() =>
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
            _disposables.Add(disposable);

            return Disposable.Create(() =>
            {
                _disposables.Remove(disposable);
            });

        }

        public void Dispose()
        {
            ((IDisposable)_disposables).Dispose();
        }

        public IObservable<KeyboardInput> KeyboardInputs { get; }

    }
}
