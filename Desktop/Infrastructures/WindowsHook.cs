using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures
{
    public abstract class WindowsHook<TMessage> : IDisposable
    {
        private IDisposable _disposable;

        /// <summary>
        /// 做为成员变量，防止 GC 回收
        /// </summary>
        private NativeApi.HookProc _hookProc;

        protected WindowsHook(int hookId, string hookName)
        {
            HookId = hookId;
            HookName = hookName;
            
            Messages = Observable.Create<TMessage>(observer => _disposable = InstallHook(observer)).Publish().RefCount();
        }

        private IDisposable InstallHook(IObserver<TMessage> observer)
        {
            _hookProc = new NativeApi.HookProc((int nCode, int wParam, IntPtr lParam) =>
            {
                if (nCode >= 0)
                {
                    observer.OnNext(CreateMessage(wParam, lParam));
                }
                return NativeApi.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            });

            var hHook = NativeApi.SetWindowsHookEx(HookId, _hookProc, IntPtr.Zero, 0);
            Trace.WriteLine($"Set {HookName} hook: {hHook}.");

            if (hHook == IntPtr.Zero)
            {
                observer.OnError(NativeApi.GetLastException());
                return Disposable.Empty;
            }

            return Disposable.Create(() =>
            {
                var result = NativeApi.UnhookWindowsHookEx(hHook);
                Trace.WriteLine($"Unhook {HookName} hook({hHook}): {result}.");

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

        protected abstract TMessage CreateMessage(int wParam, IntPtr lParam);

        public void Dispose()
        {
            _disposable?.Dispose();
            _disposable = null;
            _hookProc = null;
        }

        public int HookId { get; }

        public string HookName { get; }

        public IObservable<TMessage> Messages { get; }
    }
}
