using Desktop.Infrastructures.Native.LocationApi;
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

    public record CivicAddress(
        string CountryRegion,
        string PostalCode,
        string StateProvince,
        string City,
        string AddressLine1,
        string AddressLine2,
        uint GetDetailLevel
        );

    public record GeoAddress(CivicAddress Address, DateTimeOffset Timestamp);

    public class Geolocation
    {
        public Geolocation(bool heightAccuracy = false, uint reportInterval = 0, TimeSpan? reportTimeout = null)
        {
            HeightAccuracy = heightAccuracy;
            ReportInterval = reportInterval;
            ReportTimeout = reportTimeout ?? TimeSpan.FromSeconds(30);

            Positions = WatchPosition();
            Addresses = WatchAddress();
        }

        public bool HeightAccuracy { get; }

        public uint ReportInterval { get; }

        public TimeSpan ReportTimeout { get; }

        public IObservable<GeoPosition> Positions { get; }

        public GeoPosition GetCurrentPosition()
        {
            return Positions.FirstAsync().Wait();
        }

        public IObservable<GeoAddress> Addresses { get; }

        public GeoAddress GetCurrentAddress()
        {
            return Addresses.FirstAsync().Wait();
        }

        private LOCATION_DESIRED_ACCURACY Accuracy => HeightAccuracy
            ? LOCATION_DESIRED_ACCURACY.LOCATION_DESIRED_ACCURACY_HIGH
            : LOCATION_DESIRED_ACCURACY.LOCATION_DESIRED_ACCURACY_DEFAULT;

        private IObservable<GeoPosition> WatchPosition()
        {
            return Observable
                .Defer(() => new LatLongReportWatcher(Accuracy, ReportInterval).Reports)
                .Timeout(ReportTimeout)
                .Select(a => a.CreatePosition())
                .Publish()
                .RefCount();
        }

        private IObservable<GeoAddress> WatchAddress()
        {
            return Observable
                .Defer(() => new CivicAddressReportWatcher(Accuracy, ReportInterval).Reports)
                .Timeout(ReportTimeout)
                .Select(a => a.CreateAddress())
                .Publish()
                .RefCount();
        }
    }
}
