#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace HardwareInformation
{
	internal static class Util
	{
		internal static byte[] ToByteArray(this string hex)
		{
			return Enumerable.Range(0, hex.Length)
				.Where(x => x % 2 == 0)
				.Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
				.ToArray();
		}

		internal static string HexStringToString(this string hexString)
		{
			if (hexString == null || (hexString.Length & 1) == 1)
			{
				throw new ArgumentException();
			}

			var sb = new StringBuilder();
			for (var i = 0; i < hexString.Length; i += 2)
			{
				var hexChar = hexString.Substring(i, 2);
				sb.Append((char) Convert.ToByte(hexChar, 16));
			}

			return sb.ToString();
		}

		internal static Process StartProcess(string cmd, string args)
		{
			var psi = new ProcessStartInfo(cmd, args);
			psi.CreateNoWindow = true;
			psi.ErrorDialog = false;

			return Process.Start(psi);
		}

		internal static double? Median<TColl, TValue>(
			this IEnumerable<TColl> source,
			Func<TColl, TValue> selector)
		{
			return source.Select(selector).Median();
		}

		internal static double? Median<T>(
			this IEnumerable<T> source)
		{
			if (Nullable.GetUnderlyingType(typeof(T)) != null)
				source = source.Where(x => x != null);

			source = source.OrderBy(n => n);

			var enumerable = source as T[] ?? source.ToArray();
			var count = enumerable.Count();
			if (count == 0)
				return null;

			var midpoint = count / 2;

			if (count % 2 == 0)
			{
				return (Convert.ToDouble(enumerable.ElementAt(midpoint - 1)) +
				        Convert.ToDouble(enumerable.ElementAt(midpoint))) / 2.0;
			}

			return Convert.ToDouble(enumerable.ElementAt(midpoint));
		}

		internal static Task RunAffinity(ulong affinity, Action action)
		{
			return Task.Run(() =>
			{
				var mask = 0xffffffffuL;

				try
				{
					Thread.BeginThreadAffinity();
					mask = ThreadAffinity.Set(affinity);

					action();
				}
				finally
				{
					ThreadAffinity.Set(mask);
					Thread.EndThreadAffinity();
				}
			});
		}

		internal static string FormatBytes(ulong bytes)
		{
			string[] Suffix = {"B", "KB", "MB", "GB", "TB"};
			int i;
			double dblSByte = bytes;
			for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
			{
				dblSByte = bytes / 1024.0;
			}

			return string.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
		}
	}
}