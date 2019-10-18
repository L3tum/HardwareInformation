#region using

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

#endregion

namespace HardwareInformation
{
	internal static class ThreadAffinity
	{
		internal static ulong Set(ulong mask = 0xffffffuL)
		{
			if (mask == 0)
				return 0;

            var returnMask = 0xffffffuL;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// Unix/Posix
				ulong result = 0;
				if (NativeMethods.sched_getaffinity(0, (IntPtr) Marshal.SizeOf(result),
					    ref result) != 0)
					return 0;
				if (NativeMethods.sched_setaffinity(0, (IntPtr) Marshal.SizeOf(mask),
					    ref mask) != 0)
					return 0;
				return result;
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // OSX
                return returnMask;
            }
            
            // Windows
			var threads = Process.GetCurrentProcess().Threads;

			foreach (ProcessThread processThread in threads)
			{
				var threadId = NativeMethods.GetCurrentThreadId();

				if (processThread.Id == threadId)
				{
					try
					{
						processThread.ProcessorAffinity = (IntPtr) mask;
					}
					catch (Win32Exception)
					{
						//Console.WriteLine("{0} with mask {1}", e.Message, GetIntBinaryString(mask));
						// Intentionally left blank
					}
				}
			}

			//Console.WriteLine("Mask worked {0}", GetIntBinaryString(mask));

			return returnMask;
		}

		private static string GetIntBinaryString(ulong n)
		{
			var b = new char[64];
			var pos = 63;
			var i = 0;

			while (i < 64)
			{
				if ((n & (1uL << i)) != 0)
				{
					b[pos] = '1';
				}
				else
				{
					b[pos] = '0';
				}

				pos--;
				i++;
			}

			return new string(b);
		}

		private static class NativeMethods
		{
			private const string KERNEL = "kernel32.dll";

			private const string LIBC = "libc";

			[DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
			public static extern int GetCurrentThreadId();

			[DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
			public static extern int GetLastError();

            /// <summary>
            /// If pid is zero, then the calling thread is used.
            /// </summary>
            /// <param name="pid"></param>
            /// <param name="maskSize"></param>
            /// <param name="mask"></param>
            /// <returns></returns>
           [DllImport(LIBC)]
			public static extern int sched_getaffinity(int pid, IntPtr maskSize,
				ref ulong mask);

            /// <summary>
            /// If pid is zero, then the calling thread is used.
            /// </summary>
            /// <param name="pid"></param>
            /// <param name="maskSize"></param>
            /// <param name="mask"></param>
            /// <returns></returns>
            [DllImport(LIBC)]
			public static extern int sched_setaffinity(int pid, IntPtr maskSize,
				ref ulong mask);

            [DllImport(LIBC, CharSet = CharSet.Unicode)]
            public static extern int sysctlbyname(string function, ref Int32 coreCount, ref Int32 length, int newP = 0, int newPLength = 0);

        }
	}
}