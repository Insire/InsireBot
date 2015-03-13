using InsireBot.Objects;
using InsireBot.Util.Collections;
using System;
using System.Runtime.InteropServices;

namespace InsireBot.Util.Services
{
	/// <summary>
	/// Retrieves Audiodevices currently available under Windows
	/// http: //www.pinvoke.net/default.aspx/winmm.waveoutgetdevcaps
	/// </summary>
	public class AudioDeviceAPI
	{
		public static ThreadSafeObservableCollection<AudioDevice> getDevices()
		{
			ThreadSafeObservableCollection<AudioDevice> collection = new ThreadSafeObservableCollection<AudioDevice>();
			AudioDeviceAPI.WAVEOUTCAPS[] _devices = AudioDeviceAPI.GetDevCapsPlayback();
			for (int i = 0; i < _devices.Length; i++)
			{
				collection.Add(new AudioDevice(_devices[i].wMid, _devices[i].wPid, _devices[i].vDriverVersion, _devices[i].ToString(), _devices[i].dwFormats, _devices[i].wChannels, _devices[i].wReserved, _devices[i].dwSupport));
			}
			return collection;
		}

		/// <summary>
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
		public struct WAVEOUTCAPS
		{
			public short wMid;
			public short wPid;
			public int vDriverVersion;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			private string szPname;

			public uint dwFormats;
			public short wChannels;
			public short wReserved;
			public uint dwSupport;

			public override string ToString()
			{
				return string.Format("{0}", new object[] { szPname });
			}
		}

		/// <summary>
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
		public struct WAVEINCAPS
		{
			public short wMid;
			public short wPid;
			public int vDriverVersion;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szPname;

			public uint dwFormats;
			public short wChannels;
			public short wReserved;
			//public int dwSupport;

			public override string ToString()
			{
				return string.Format("wMid:{0}|wPid:{1}|vDriverVersion:{2}|'szPname:{3}'|dwFormats:{4}|wChannels:{5}|wReserved:{6}", new object[] { wMid, wPid, vDriverVersion, szPname, dwFormats, wChannels, wReserved });
			}
		}

		public static WAVEOUTCAPS[] GetDevCapsPlayback()
		{
			uint waveOutDevicesCount = waveOutGetNumDevs();
			if (waveOutDevicesCount > 0)
			{
				WAVEOUTCAPS[] list = new WAVEOUTCAPS[waveOutDevicesCount];
				for (int uDeviceID = 0; uDeviceID < waveOutDevicesCount; uDeviceID++)
				{
					WAVEOUTCAPS waveOutCaps = new WAVEOUTCAPS();
					waveOutGetDevCaps(uDeviceID, ref waveOutCaps, Marshal.SizeOf(typeof(WAVEOUTCAPS)));
					list[uDeviceID] = waveOutCaps;
				}
				return list;
			}
			else
			{
				return null;
			}
		}

		public static WAVEINCAPS[] GetDevCapsRecording()
		{
			uint waveInDevicesCount = waveInGetNumDevs();
			if (waveInDevicesCount > 0)
			{
				WAVEINCAPS[] list = new WAVEINCAPS[waveInDevicesCount];
				for (Int32 uDeviceID = 0; uDeviceID < waveInDevicesCount; uDeviceID++)
				{
					WAVEINCAPS waveInCaps = new WAVEINCAPS();
					waveInGetDevCaps(uDeviceID, ref waveInCaps, Marshal.SizeOf(typeof(WAVEINCAPS)));
					list[uDeviceID] = waveInCaps;
				}
				return list;
			}
			else
			{
				return null;
			}
		}

		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern uint waveInGetDevCaps(Int32 hwo, ref WAVEINCAPS pwic, /*uint*/ int cbwic);

		[DllImport("winmm.dll", SetLastError = true)]
		private static extern uint waveInGetNumDevs();

		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern uint waveOutGetDevCaps(int hwo, ref WAVEOUTCAPS pwoc, /*uint*/ int cbwoc);

		[DllImport("winmm.dll", SetLastError = true)]
		private static extern uint waveOutGetNumDevs();
	}
}