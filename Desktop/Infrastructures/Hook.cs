using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures
{
    public abstract class Hook<TInput>
    {
        private IDisposable _disposable;

        protected Hook()
        {
            Inputs = Observable.Create<TInput>(observer => _disposable = WatchInput(observer)).Publish().RefCount();
        }

        protected abstract IDisposable WatchInput(IObserver<TInput> observer);

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        public IObservable<TInput> Inputs { get; }
    }
}
