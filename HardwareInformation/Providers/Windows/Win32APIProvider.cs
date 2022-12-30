using System.Runtime.InteropServices;
using System.Security;

namespace HardwareInformation.Providers.Windows;

#region Win32API

// Taken from https://github.com/pruggitorg/detect-windows-version
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IdentifierTypo
internal enum NTSTATUS : uint
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    STATUS_SUCCESS = 0x00000000
}

// Taken from https://github.com/pruggitorg/detect-windows-version
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct OSVERSIONINFOEX
{
    // The OSVersionInfoSize field must be set to Marshal.SizeOf(typeof(OSVERSIONINFOEX))
    public int OSVersionInfoSize;
    public int MajorVersion;
    public int MinorVersion;
    public int BuildNumber;
    public int PlatformId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string CSDVersion;

    public ushort ServicePackMajor;
    public ushort ServicePackMinor;
    public ushort SuiteMask;
    public byte ProductType;
    public byte Reserved;
}

internal static class Win32APIProvider
{
    private const string NTDLL = "ntdll.dll";

    [SecurityCritical]
    [DllImport(NTDLL, EntryPoint = "RtlGetVersion", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern NTSTATUS ntdll_RtlGetVersion(ref OSVERSIONINFOEX versionInfo);
}
// ReSharper restore InconsistentNaming
// ReSharper restore FieldCanBeMadeReadOnly.Global
// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore IdentifierTypo

#endregion