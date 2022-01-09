using Desktop.Infrastructures.Native.LocationApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures.Location
{
    public static class LocationReportExtensions
    {
        [DllImport("Ole32")]
        private static extern void PropVariantClear(ref PROPVARIANT pvar);

        public static double GetDoubleValue(this ILocationReport report, uint propertyId)
        {
            var key = new PROPERTYKEY(propertyId);
            var val = report.GetValue(ref key);
            var dbl = val.vt == VarEnum.VT_R8 ? val.dblVal : double.NaN;
            PropVariantClear(ref val);
            return dbl;
        }

        public static DateTimeOffset GetTimeOffset(this ILocationReport report)
        {
            var ts = report.GetTimestamp();
            return new DateTimeOffset(
                ts.wYear,
                ts.wMonth,
                ts.wDay,
                ts.wHour,
                ts.wMinute,
                ts.wSecond,
                ts.wMilliseconds,
                TimeSpan.Zero
                );
        }

        public static GeoPosition CreatePosition(this ILatLongReport report)
        {
            var coordinates = new GeoCoordinates(
                report.GetLatitude(),
                report.GetLongitude(),
                report.GetAltitude(),
                report.GetErrorRadius(),
                report.GetAltitudeError(),
                report.GetDoubleValue(7),
                report.GetDoubleValue(2)
                );
            return new GeoPosition(coordinates, report.GetTimeOffset());
        }

        public static GeoAddress CreateAddress(this ICivicAddressReport report)
        {
            var address = new CivicAddress(
                report.GetCountryRegion(),
                report.GetPostalCode(),
                report.GetStateProvince(),
                report.GetCity(),
                report.GetAddressLine1(),
                report.GetAddressLine2(),
                report.GetDetailLevel()
                );
            return new GeoAddress(address, report.GetTimeOffset());
        }
    }
}
