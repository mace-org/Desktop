using LocationApiLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures
{
    public class Geolocation : ILocationEvents
    {
        private ILocation _location;

        private Guid _latLongReportId = typeof(ILatLongReport).GUID;
        private Guid _civicAddressReportId = typeof(ICivicAddressReport).GUID;

        void ILocationEvents.OnLocationChanged(ref Guid reportType, ILocationReport pLocationReport)
        {
            Debug.WriteLine($"Location Changed: {reportType}, {pLocationReport.GetSensorID()}, {pLocationReport.GetTimestamp()}, {pLocationReport}");
            if(reportType == _latLongReportId)
            {
                var latLongReport = pLocationReport as ILatLongReport;
                Debug.WriteLine($"Lat Long Report -- Altitude: {latLongReport.GetAltitude()}, AltitudeError: {latLongReport.GetAltitudeError()}, ErrorRadius: {latLongReport.GetErrorRadius()}, Latitude: {latLongReport.GetLatitude()}, Longitude: {latLongReport.GetLongitude()}");
            }
            else if(reportType == _civicAddressReportId)
            {
                var civicAddressReport = pLocationReport as ICivicAddressReport;
                Debug.WriteLine($"Civic Address Report -- AddressLine1: {civicAddressReport.GetAddressLine1()}, AddressLine2: {civicAddressReport.GetAddressLine2()}, City: {civicAddressReport.GetCity()}, CountryRegion: {civicAddressReport.GetCountryRegion()}, DetailLevel: {civicAddressReport.GetDetailLevel()}, PostalCode: {civicAddressReport.GetPostalCode()}, StateProvince: {civicAddressReport.GetStateProvince()}");
            }
        }

        void ILocationEvents.OnStatusChanged(ref Guid reportType, LOCATION_REPORT_STATUS newStatus)
        {
            Debug.WriteLine($"Status Changed: {newStatus}");
        }

        public void Start()
        {
            if(_location == null)
            {
                _location = new LocationClass();

                _location.RegisterForReport(this, ref _latLongReportId, 0);
                var llacc = _location.GetDesiredAccuracy(ref _latLongReportId);
                var llint = _location.GetReportInterval(ref _latLongReportId);

                Debug.WriteLine($"Location Report -- accuracy: {llacc}, interval: {llint}");

                _location.RegisterForReport(this, ref _civicAddressReportId, 0);
                var caacc = _location.GetDesiredAccuracy(ref _civicAddressReportId);
                var caint = _location.GetReportInterval(ref _civicAddressReportId);

                Debug.WriteLine($"Civic Address Report -- accuracy: {caacc}, interval: {caint}");
            }
        }

        public void Stop()
        {
            if(_location != null)
            {
                 _location.UnregisterForReport(ref _latLongReportId);
               _location.UnregisterForReport(ref _civicAddressReportId);
                _location = null;
            }
        }
    }
}
