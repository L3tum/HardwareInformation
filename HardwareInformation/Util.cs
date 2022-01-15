#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
            if (hexString == null)
            {
                throw new ArgumentException();
            }

            var sb = new StringBuilder();
            for (var i = 0; i < hexString.Length; i += 2)
            {
                string hexChar;

                if (i == hexString.Length - 1)
                {
                    hexChar = hexString.Substring(i, 1);
                }
                else
                {
                    hexChar = hexString.Substring(i, 2);
                }

                var cha = (char) Convert.ToByte(hexChar, 16);

                if (cha == '\u0000')
                {
                    sb.Append(" ");
                }
                else
                {
                    sb.Append(cha);
                }
            }

            return sb.ToString();
        }

        internal static Process StartProcess(string cmd, string args)
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                CreateNoWindow = true,
                ErrorDialog = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

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
            {
                source = source.Where(x => x != null);
            }

            source = source.OrderBy(n => n);

            var enumerable = source as T[] ?? source.ToArray();
            var count = enumerable.Count();
            if (count == 0)
            {
                return null;
            }

            var midpoint = count / 2;

            if (count % 2 == 0)
            {
                return (Convert.ToDouble(enumerable.ElementAt(midpoint - 1)) +
                        Convert.ToDouble(enumerable.ElementAt(midpoint))) / 2.0;
            }

            return Convert.ToDouble(enumerable.ElementAt(midpoint));
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
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

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        internal static string FormatBytes(ulong bytes)
        {
            ReadOnlySpan<string> suffix = new[] {"B", "KiB", "MiB", "GiB", "TiB", "PiB"};
            int i;
            float dblSByte = bytes;
            for (i = 0; i < suffix.Length && dblSByte >= 1024.0f; i++)
            {
                dblSByte /= 1024.0f;
            }

            dblSByte = MathF.Round(dblSByte, 2);

            return $"{dblSByte:F2} {suffix[i]}";
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static uint ExtractBits(uint number, int start, int end)
        {
            var numBits = end - start + 1;
            var result = number >> start;
            result &= CreateBitMask(numBits);

            return result;
        }

        internal static uint CreateBitMask(int width)
        {
            return (1u << width) - 1;
        }

        internal static int GetNumberOfSetBits(int i)
        {
            i -= (i >> 1) & 0x55555555;
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }
    }

    internal static class TypeExtension
    {
        private static readonly ConcurrentDictionary<Type, object> TypeDefaults = new();

        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType
                ? TypeDefaults.GetOrAdd(type, Activator.CreateInstance)
                : null;
        }
    }
}