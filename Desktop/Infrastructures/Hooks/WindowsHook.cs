using Desktop.Infrastructures.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures.Hooks
{
    public abstract class WindowsHook<TMessage>
    {
        protected WindowsHook(int hookId, string hookName)
        {
            HookId = hookId;
            HookName = hookName;
            
            Messages = Observable.Create<TMessage>(InstallHook).Publish().RefCount();
        }

        public int HookId { get; }

        public string HookName { get; }

        public IObservable<TMessage> Messages { get; }

        private IDisposable InstallHook(IObserver<TMessage> observer)
        {
            var hookProc = new NativeApi.HookProc((int nCode, int wParam, IntPtr lParam) =>
            {
                if (nCode >= 0)
                {
                    observer.OnNext(CreateMessage(wParam, lParam));
                }
                return NativeApi.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            });

            var hHook = NativeApi.SetWindowsHookEx(HookId, hookProc, IntPtr.Zero, 0);
            Trace.WriteLine($"Set {HookName} hook: {hHook}.");

            if (hHook == IntPtr.Zero)
            {
                observer.OnError(NativeApi.GetLastException());
                return Disposable.Empty;
            }

            return Disposable.Create(() =>
            {
                // 此处的目的是持有 hookProc 变量的引用，防止 GC 过早回收
                hookProc = null;
                var result = NativeApi.UnhookWindowsHookEx(hHook);
                Trace.WriteLine($"Unhook {HookName} hook({hHook}): {result}.");
            });
        }

        protected abstract TMessage CreateMessage(int wParam, IntPtr lParam);

    }
}
