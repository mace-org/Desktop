using Desktop.Infrastructures.Location.ApiLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures.Location
{
    public record GeoCoordinates(
        double Latitude,
        double Longitude,
        double Altitude,
        double Accuracy,
        double AltitudeAccuracy,
        double Heading,
        double Speed
        );

    public record GeoPosition(GeoCoordinates Coordinates, DateTimeOffset Timestamp);

    public class Geolocation : IDisposable
    {
        private static readonly Guid LatLongReportId = typeof(ILatLongReport).GUID;
        private static readonly Guid CivicAddressReportId = typeof(ICivicAddressReport).GUID;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public Geolocation(bool heightAccuracy = false, uint reportInterval = 0, TimeSpan? reportTimeout = null)
        {
            HeightAccuracy = heightAccuracy;
            ReportInterval = reportInterval;
            ReportTimeout = reportTimeout ?? TimeSpan.FromSeconds(30);

            Positions = WatchPosition();
        }

        public bool HeightAccuracy { get; }

        public uint ReportInterval { get; }

        public TimeSpan ReportTimeout { get; }

        public IObservable<GeoPosition> Positions { get; }

        public GeoPosition GetCurrentPosition()
        {
            return Positions.FirstAsync().Wait();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private LOCATION_DESIRED_ACCURACY Accuracy => HeightAccuracy
            ? LOCATION_DESIRED_ACCURACY.LOCATION_DESIRED_ACCURACY_HIGH
            : LOCATION_DESIRED_ACCURACY.LOCATION_DESIRED_ACCURACY_DEFAULT;

        private double GetDoubleProperty(ILocationReport report, uint propertyId)
        {
            var key = new PROPERTYKEY(propertyId);
            using (var val = report.GetValue(ref key))
            {
                if (val.vt == VarEnum.VT_R8)
                {
                    return (double)val.GetValue();
                }
                return double.NaN;
            }
        }

        private GeoPosition CreatePosition(ILatLongReport report)
        {
            var ts = report.GetTimestamp();
            var timestamp = new DateTimeOffset(
                ts.wYear, 
                ts.wMonth, 
                ts.wDay, 
                ts.wHour, 
                ts.wMinute, 
                ts.wSecond, 
                ts.wMilliseconds, 
                TimeSpan.Zero
                );
            var coordinates = new GeoCoordinates(
                report.GetLatitude(),
                report.GetLongitude(),
                report.GetAltitude(),
                report.GetErrorRadius(),
                report.GetAltitudeError(),
                GetDoubleProperty(report, 7),
                GetDoubleProperty(report, 2)
                );
            return new GeoPosition(coordinates, timestamp);
        }

        private IObservable<GeoPosition> WatchPosition()
        {
            var report = new LocationReport(LatLongReportId, Accuracy, ReportInterval, ReportTimeout);
            _disposables.Add(report);
            return report.Reports.Cast<ILatLongReport>().Select(CreatePosition).Publish().RefCount();
        }





        //void ILocationEvents.OnLocationChanged(ref Guid reportType, ILocationReport pLocationReport)
        //{
        //    Debug.WriteLine($"Location Changed: {reportType}, {pLocationReport.GetSensorID()}, {pLocationReport.GetTimestamp()}, {pLocationReport}");
        //    if (reportType == _latLongReportId)
        //    {
        //        var latLongReport = pLocationReport as ILatLongReport;
        //        Debug.WriteLine($"Lat Long Report -- Altitude: {latLongReport.GetAltitude()}, AltitudeError: {latLongReport.GetAltitudeError()}, ErrorRadius: {latLongReport.GetErrorRadius()}, Latitude: {latLongReport.GetLatitude()}, Longitude: {latLongReport.GetLongitude()}");

        //        var civicAddressReport = pLocationReport as ICivicAddressReport;
        //        Debug.WriteLine($"{civicAddressReport == null}");

        //        var pKey = new _tagpropertykey { fmtid = new Guid("055C74D8-CA6F-47D6-95C6-1ED3637A0FF4"), pid = 28 };
        //        var pVal = pLocationReport.GetValue(ref pKey);
        //        if (pVal.vt == 0x1F)
        //        {

        //        }
        //    }
        //    else if (reportType == _civicAddressReportId)
        //    {
        //        var civicAddressReport = pLocationReport as ICivicAddressReport;
        //        Debug.WriteLine($"Civic Address Report -- AddressLine1: {civicAddressReport.GetAddressLine1()}, AddressLine2: {civicAddressReport.GetAddressLine2()}, City: {civicAddressReport.GetCity()}, CountryRegion: {civicAddressReport.GetCountryRegion()}, DetailLevel: {civicAddressReport.GetDetailLevel()}, PostalCode: {civicAddressReport.GetPostalCode()}, StateProvince: {civicAddressReport.GetStateProvince()}");
        //    }
        //}

        //void ILocationEvents.OnStatusChanged(ref Guid reportType, LOCATION_REPORT_STATUS newStatus)
        //{
        //    if (newStatus == LOCATION_REPORT_STATUS.)
        //        Debug.WriteLine($"Status Changed: {newStatus}");
        //}

        //public void Start()
        //{
        //    if (_location == null)
        //    {
        //        _location = new LocationClass();

        //        _location.SetDesiredAccuracy(ref _latLongReportId, LOCATION_DESIRED_ACCURACY.LOCATION_DESIRED_ACCURACY_HIGH);
        //        _location.RegisterForReport(this, ref _latLongReportId, 0);
        //        //var llacc = _location.GetDesiredAccuracy(ref _latLongReportId);
        //        //var llint = _location.GetReportInterval(ref _latLongReportId);

        //        //Debug.WriteLine($"Location Report -- accuracy: {llacc}, interval: {llint}");

        //        //_location.RegisterForReport(this, ref _civicAddressReportId, 0);
        //        //var caacc = _location.GetDesiredAccuracy(ref _civicAddressReportId);
        //        //var caint = _location.GetReportInterval(ref _civicAddressReportId);

        //        //Debug.WriteLine($"Civic Address Report -- accuracy: {caacc}, interval: {caint}");
        //    }
        //}

        //public void Stop()
        //{
        //    if (_location != null)
        //    {
        //        _location.UnregisterForReport(ref _latLongReportId);
        //        //_location.UnregisterForReport(ref _civicAddressReportId);
        //        _location = null;
        //    }
        //}

        private class LocationReport : ILocationEvents, IDisposable
        {
            private IObserver<ILocationReport> _observer;
            private IDisposable _disposable;

            public LocationReport(Guid reportId, LOCATION_DESIRED_ACCURACY accuracy, uint reportInterval, TimeSpan? reportTimeout)
            {
                var observable = WatchReport(reportId, accuracy, reportInterval);
                if (reportTimeout.HasValue)
                {
                    observable = observable.Timeout(reportTimeout.Value);
                }
                Reports = observable.Publish().RefCount();
            }

            public IObservable<ILocationReport> Reports { get; }

            public void Dispose()
            {
                _disposable?.Dispose();
            }

            private IObservable<ILocationReport> WatchReport(Guid reportId, LOCATION_DESIRED_ACCURACY accuracy, uint reportInterval)
            {
                return Observable.Create<ILocationReport>(observer =>
                {
                    _observer = observer;

                    var location = new LocationImpl();
                    location.RequestPermissions(IntPtr.Zero, ref reportId, 1, 0);
                    location.SetDesiredAccuracy(ref reportId, accuracy);
                    if (reportInterval > 0)
                    {
                        location.SetReportInterval(ref reportId, reportInterval);
                    }
                    location.RegisterForReport(this, ref reportId, 0);

                    return _disposable = Disposable.Create(() =>
                    {
                        try
                        {
                            location.UnregisterForReport(ref reportId);
                            observer.OnCompleted();
                        }
                        catch (Exception exp)
                        {
                            observer.OnError(exp);
                        }
                        finally
                        {
                            _observer = null;
                            _disposable = null;
                        }
                    });
                });
            }

            void ILocationEvents.OnLocationChanged(ref Guid reportType, ILocationReport pLocationReport)
            {
                _observer.OnNext(pLocationReport);
            }

            void ILocationEvents.OnStatusChanged(ref Guid reportType, LOCATION_REPORT_STATUS newStatus)
            {
                if (newStatus <= LOCATION_REPORT_STATUS.REPORT_ACCESS_DENIED)
                {
                    _observer.OnError(new Exception(newStatus.ToString()));
                }
            }
        }
    }
}
