using Desktop.Infrastructures.Native.LocationApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures.Location
{
    public abstract class LocationReportWatcher<TReport> : ILocationEvents where TReport: ILocationReport
    {
        private IObserver<ILocationReport> _locationObserver;
        private IObserver<Unit> _statusObserver;
        private string _name;

        protected LocationReportWatcher(Guid reportId, string name, LOCATION_DESIRED_ACCURACY accuracy, uint reportInterval)
        {
            _name = name;
            Reports = WatchReport(reportId, accuracy, reportInterval).Publish().RefCount();
        }

        public IObservable<TReport> Reports { get; }

        private IObservable<ILocationReport> WatchLocation(Guid reportId, LOCATION_DESIRED_ACCURACY accuracy, uint reportInterval)
        {
            return Observable.Create<ILocationReport>(observer =>
            {
                _locationObserver = observer;

                Trace.WriteLine($"Register for {_name} report.");
                var location = new LocationImpl();
                location.RequestPermissions(IntPtr.Zero, ref reportId, 1, 0);
                location.SetDesiredAccuracy(ref reportId, accuracy);
                location.RegisterForReport(this, ref reportId, 0);
                // 只能放在 RegisterForReport 后面，否则会发生异常
                // HRESULT_FROM_WIN32(ERROR_INVALID_STATE)
                // The caller is not registered to receive events for the specified report type.
                if (reportInterval > 0)
                {
                    location.SetReportInterval(ref reportId, reportInterval);
                }

                return Disposable.Create(() =>
                {
                    try
                    {
                        Trace.WriteLine($"Unregister for {_name} report.");
                        location.UnregisterForReport(ref reportId);
                    }
                    finally
                    {
                        _locationObserver = null;
                    }
                });
            });
        }

        private IObservable<Unit> WatchStatus()
        {
            return Observable.Create<Unit>(observer =>
            {
                _statusObserver = observer;
                return () => _statusObserver = null;
            });
        }

        private IObservable<TReport> WatchReport(Guid reportId, LOCATION_DESIRED_ACCURACY accuracy, uint reportInterval)
        {
            return Observable.Defer(() =>
            {
                var status = WatchStatus();
                var location = WatchLocation(reportId, accuracy, reportInterval);
                // status 要放到前面，因为在 location.RegisterForReport 的过程中就有可能触发 OnStatusChanged 事件
                return status.CombineLatest(location, (a, b) => (TReport)b);
            });
        }

        void ILocationEvents.OnLocationChanged(ref Guid reportType, ILocationReport pLocationReport)
        {
            _locationObserver?.OnNext(pLocationReport);
        }

        void ILocationEvents.OnStatusChanged(ref Guid reportType, LOCATION_REPORT_STATUS newStatus)
        {
            Trace.WriteLine($"The {_name} report status changed to '{newStatus}'.");
            if (newStatus <= LOCATION_REPORT_STATUS.REPORT_ACCESS_DENIED)
            {
                _statusObserver?.OnError(new Exception(newStatus.ToString()));
            }
            else if(newStatus == LOCATION_REPORT_STATUS.REPORT_RUNNING)
            {
                _statusObserver?.OnNext(Unit.Default);
            }
        }
    }

    public class LatLongReportWatcher : LocationReportWatcher<ILatLongReport>
    {
        public LatLongReportWatcher(LOCATION_DESIRED_ACCURACY accuracy, uint reportInterval)
            : base(typeof(ILatLongReport).GUID, "latitude and longitude", accuracy, reportInterval)
        {
        }
    }

    public class CivicAddressReportWatcher : LocationReportWatcher<ICivicAddressReport>
    {
        public CivicAddressReportWatcher(LOCATION_DESIRED_ACCURACY accuracy, uint reportInterval)
            : base(typeof(ICivicAddressReport).GUID, "civic address", accuracy, reportInterval)
        {
        }
    }
}
