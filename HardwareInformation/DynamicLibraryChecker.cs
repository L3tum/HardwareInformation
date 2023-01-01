#region using

using System;
using System.Runtime.InteropServices;

#endregion

namespace HardwareInformation
{
    /// <summary>
    ///     Class to check whether a dynamic library exists
    /// </summary>
    internal static class DynamicLibraryChecker
    {
        /// <summary>
        ///     Checks whether a dynamic library with the name <paramref name="libraryName" /> is available.
        /// </summary>
        /// <param name="libraryName"></param>
        /// <returns></returns>
        public static bool CheckLibrary(string libraryName)
        {
            var present = false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var handle = NativeMethods.LoadLibrary(libraryName);

                if (handle != IntPtr.Zero)
                {
                    present = true;
                    NativeMethods.FreeLibrary(handle);
                }
            }
            else
            {
                // RTLD_LAZY has the value 0 (https://code.woboq.org/userspace/glibc/dlfcn/dlfcn.h.html)
                var handle = NativeMethods.dlopen(libraryName, 0);

                if (handle != IntPtr.Zero)
                {
                    present = true;
                    NativeMethods.dlclose(handle);
                }
            }

            return present;
        }

        private static class NativeMethods
        {
            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibrary(string fileName);

            [DllImport("kernel32", SetLastError = true)]
            internal static extern void FreeLibrary(IntPtr handle);

            [DllImport("libc", CharSet = CharSet.Unicode)]
            internal static extern IntPtr dlopen(string fileName, int flag);

            [DllImport("libc")]
            internal static extern int dlclose(IntPtr handle);
        }
    }
}