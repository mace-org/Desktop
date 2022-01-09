using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures.Native.LocationApi
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct SYSTEMTIME
	{
		public ushort wYear;
		public ushort wMonth;
		public ushort wDayOfWeek;
		public ushort wDay;
		public ushort wHour;
		public ushort wMinute;
		public ushort wSecond;
		public ushort wMilliseconds;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct PROPERTYKEY
	{
		public Guid fmtid;
		public uint pid;

		public PROPERTYKEY(uint propertyId)
		{
			pid = propertyId;
			fmtid = new Guid("055C74D8-CA6F-47D6-95C6-1ED3637A0FF4");
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 24)]
	public struct PROPVARIANT
	{
		[FieldOffset(0)]
		public VarEnum vt;
		[FieldOffset(8)]
		public double dblVal;
		[FieldOffset(8)]
		public IntPtr other;
	}

	public enum LOCATION_REPORT_STATUS
	{
		REPORT_NOT_SUPPORTED,
		REPORT_ERROR,
		REPORT_ACCESS_DENIED,
		REPORT_INITIALIZING,
		REPORT_RUNNING
	}

	public enum LOCATION_DESIRED_ACCURACY
	{
		LOCATION_DESIRED_ACCURACY_DEFAULT,
		LOCATION_DESIRED_ACCURACY_HIGH
	}

	[ComImport]
	[Guid("C8B7F7EE-75D0-4DB9-B62D-7A0F369CA456")]
	[InterfaceType(1)]
	public interface ILocationReport
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		Guid GetSensorID();

		[MethodImpl(MethodImplOptions.InternalCall)]
		SYSTEMTIME GetTimestamp();

		[MethodImpl(MethodImplOptions.InternalCall)]
		PROPVARIANT GetValue([In] ref PROPERTYKEY pKey);
	}

	[ComImport]
	[InterfaceType(1)]
	[Guid("7FED806D-0EF8-4F07-80AC-36A0BEAE3134")]
	public interface ILatLongReport : ILocationReport
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		new Guid GetSensorID();

		[MethodImpl(MethodImplOptions.InternalCall)]
		new SYSTEMTIME GetTimestamp();

		[MethodImpl(MethodImplOptions.InternalCall)]
		new PROPVARIANT GetValue([In] ref PROPERTYKEY pKey);

		[MethodImpl(MethodImplOptions.InternalCall)]
		double GetLatitude();

		[MethodImpl(MethodImplOptions.InternalCall)]
		double GetLongitude();

		[MethodImpl(MethodImplOptions.InternalCall)]
		double GetErrorRadius();

		[MethodImpl(MethodImplOptions.InternalCall)]
		double GetAltitude();

		[MethodImpl(MethodImplOptions.InternalCall)]
		double GetAltitudeError();
	}

	[ComImport]
	[Guid("C0B19F70-4ADF-445D-87F2-CAD8FD711792")]
	[InterfaceType(1)]
	public interface ICivicAddressReport : ILocationReport
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		new Guid GetSensorID();

		[MethodImpl(MethodImplOptions.InternalCall)]
		new SYSTEMTIME GetTimestamp();

		[MethodImpl(MethodImplOptions.InternalCall)]
		new PROPVARIANT GetValue([In] ref PROPERTYKEY pKey);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.BStr)]
		string GetAddressLine1();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.BStr)]
		string GetAddressLine2();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.BStr)]
		string GetCity();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.BStr)]
		string GetStateProvince();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.BStr)]
		string GetPostalCode();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.BStr)]
		string GetCountryRegion();

		[MethodImpl(MethodImplOptions.InternalCall)]
		uint GetDetailLevel();
	}

	[ComImport]
	[InterfaceType(1)]
	[Guid("CAE02BBF-798B-4508-A207-35A7906DC73D")]
	public interface ILocationEvents
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void OnLocationChanged([In] ref Guid reportType, [In][MarshalAs(UnmanagedType.Interface)] ILocationReport pLocationReport);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void OnStatusChanged([In] ref Guid reportType, [In] LOCATION_REPORT_STATUS newStatus);
	}

	[ComImport]
	[Guid("AB2ECE69-56D9-4F28-B525-DE1B0EE44237")]
	[InterfaceType(1)]
	public interface ILocation
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void RegisterForReport([In][MarshalAs(UnmanagedType.Interface)] ILocationEvents pEvents, [In] ref Guid reportType, [In] uint dwRequestedReportInterval);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void UnregisterForReport([In] ref Guid reportType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.Interface)]
		ILocationReport GetReport([In] ref Guid reportType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		LOCATION_REPORT_STATUS GetReportStatus([In] ref Guid reportType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		uint GetReportInterval([In] ref Guid reportType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void SetReportInterval([In] ref Guid reportType, [In] uint millisecondsRequested);

		[MethodImpl(MethodImplOptions.InternalCall)]
		LOCATION_DESIRED_ACCURACY GetDesiredAccuracy([In] ref Guid reportType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void SetDesiredAccuracy([In] ref Guid reportType, [In] LOCATION_DESIRED_ACCURACY desiredAccuracy);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void RequestPermissions([In] IntPtr hParent, [In] ref Guid pReportTypes, [In] uint count, [In] int fModal);
	}

	[ComImport]
	[CoClass(typeof(LocationClass))]
	[Guid("AB2ECE69-56D9-4F28-B525-DE1B0EE44237")]
	public interface LocationImpl : ILocation
	{
	}

	[ComImport]
	[ClassInterface(0u)]
	[Guid("E5B8E079-EE6D-4E33-A438-C87F2E959254")]
	[TypeLibType(2)]
	public class LocationClass
	{
	}
}
