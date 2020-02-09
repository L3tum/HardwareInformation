#region using

using System;
using System.Runtime.InteropServices;

#endregion

namespace HardwareInformation
{
	internal static class Opcode
	{
		private static IntPtr codeBuffer;
		private static ulong size;

		internal static RdtscDelegate Rdtsc;

		private static readonly byte[] RDTSC_32 =
		{
			0x0F, 0x31, // rdtsc   
			0xC3 // ret  
		};

		private static readonly byte[] RDTSC_64 =
		{
			0x0F, 0x31, // rdtsc  
			0x48, 0xC1, 0xE2, 0x20, // shl rdx, 20h  
			0x48, 0x0B, 0xC2, // or rax, rdx  
			0xC3 // ret  
		};

		internal static CpuidDelegate Cpuid;

		private static readonly byte[] CPUID_32 =
		{
			0x53, // push   %ebx
			0x57, // push   %edi
			0x8b, 0x7c, 0x24, 0x0c, // mov    0xc(%esp),%edi
			0x8b, 0x44, 0x24, 0x10, // mov    0x10(%esp),%eax
			0x8b, 0x4c, 0x24, 0x14, // mov    0x14(%esp),%ecx
			0x0f, 0xa2, // cpuid
			0x89, 0x07, // mov    %eax,(%edi)
			0x89, 0x5f, 0x04, // mov    %ebx,0x4(%edi)
			0x89, 0x4f, 0x08, // mov    %ecx,0x8(%edi)
			0x89, 0x57, 0x0c, // mov    %edx,0xc(%edi)
			0x5f, // pop    %edi
			0x5b, // pop    %ebx
			0xc3 // ret
		};

		private static readonly byte[] CPUID_64_WINDOWS =
		{
			0x53, // push   %rbx
			0x89, 0xd0, // mov    %edx,%eax
			0x49, 0x89, 0xc9, // mov    %rcx,%r9
			0x44, 0x89, 0xc1, // mov    %r8d,%ecx
			0x0f, 0xa2, // cpuid
			0x41, 0x89, 0x01, // mov    %eax,(%r9)
			0x41, 0x89, 0x59, 0x04, // mov    %ebx,0x4(%r9)
			0x41, 0x89, 0x49, 0x08, // mov    %ecx,0x8(%r9)
			0x41, 0x89, 0x51, 0x0c, // mov    %edx,0xc(%r9)
			0x5b, // pop    %rbx
			0xc3 // retq
		};

		private static readonly byte[] CPUID_64_LINUX =
		{
			0x53, // push   %rbx
			0x89, 0xf0, // mov    %esi,%eax
			0x89, 0xd1, // mov    %edx,%ecx
			0x0f, 0xa2, // cpuid
			0x89, 0x07, // mov    %eax,(%rdi)
			0x89, 0x5f, 0x04, // mov    %ebx,0x4(%rdi)
			0x89, 0x4f, 0x08, // mov    %ecx,0x8(%rdi)
			0x89, 0x57, 0x0c, // mov    %edx,0xc(%rdi)
			0x5b, // pop    %rbx
			0xc3 // retq
		};

		internal static bool IsOpen { get; private set; }

		internal static void Open()
		{
			if (IsOpen) return;

			IsOpen = true;

			byte[] rdtscCode;
			byte[] cpuidCode;
			if (IntPtr.Size == 4)
			{
				rdtscCode = RDTSC_32;
				cpuidCode = CPUID_32;
			}
			else
			{
				rdtscCode = RDTSC_64;
				cpuidCode = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
				            RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
					? CPUID_64_LINUX
					: CPUID_64_WINDOWS;
			}

			size = (ulong) (rdtscCode.Length + cpuidCode.Length);

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// Unix   
				codeBuffer = NativeMethods.mmap(IntPtr.Zero, size,
					MmapProts.PROT_READ | MmapProts.PROT_WRITE | MmapProts.PROT_EXEC,
					MmapFlags.MAP_ANONYMOUS | MmapFlags.MAP_PRIVATE, -1, 0);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				// OSX   
				// OSX is NOT POSIX compliant, in the way that MAP_ANON is a different value on OSX (0x1000) than Unix (0x20), which breaks just about every program making use of it
				// Good job Apple. Good job.
				codeBuffer = NativeMethods.mmap(IntPtr.Zero, size,
					MmapProts.PROT_READ | MmapProts.PROT_WRITE | MmapProts.PROT_EXEC,
					MmapFlags.MAP_ANON | MmapFlags.MAP_PRIVATE, -1, 0);
			}
			else
			{
				// Windows
				codeBuffer = NativeMethods.VirtualAlloc(IntPtr.Zero,
					(UIntPtr) size, AllocationType.COMMIT | AllocationType.RESERVE,
					MemoryProtection.EXECUTE_READWRITE);
			}

			Marshal.Copy(rdtscCode, 0, codeBuffer, rdtscCode.Length);

			Rdtsc = Marshal.GetDelegateForFunctionPointer(
				codeBuffer, typeof(RdtscDelegate)) as RdtscDelegate;

			var cpuidAddress = (IntPtr) ((long) codeBuffer + rdtscCode.Length);
			Marshal.Copy(cpuidCode, 0, cpuidAddress, cpuidCode.Length);

			Cpuid = Marshal.GetDelegateForFunctionPointer(
				cpuidAddress, typeof(CpuidDelegate)) as CpuidDelegate;
		}

		internal static void Close()
		{
			Rdtsc = null;
			Cpuid = null;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				// Unix
				NativeMethods.munmap(codeBuffer, size);
			}
			else
			{
				// Windows
				NativeMethods.VirtualFree(codeBuffer, UIntPtr.Zero,
					FreeType.RELEASE);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		internal delegate bool CpuidDelegate(out Result result, uint eax, uint ecx);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		internal delegate ulong RdtscDelegate();

		[Flags]
		internal enum AllocationType : uint
		{
			COMMIT = 0x1000,
			RESERVE = 0x2000,
			RESET = 0x80000,
			LARGE_PAGES = 0x20000000,
			PHYSICAL = 0x400000,
			TOP_DOWN = 0x100000,
			WRITE_WATCH = 0x200000
		}

		[Flags]
		internal enum FreeType
		{
			DECOMMIT = 0x4000,
			RELEASE = 0x8000
		}

		[Flags]
		internal enum MemoryProtection : uint
		{
			EXECUTE = 0x10,
			EXECUTE_READ = 0x20,
			EXECUTE_READWRITE = 0x40,
			EXECUTE_WRITECOPY = 0x80,
			NOACCESS = 0x01,
			READONLY = 0x02,
			READWRITE = 0x04,
			WRITECOPY = 0x08,
			GUARD = 0x100,
			NOCACHE = 0x200,
			WRITECOMBINE = 0x400
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct Result
		{
			internal uint eax;
			internal uint ebx;
			internal uint ecx;
			internal uint edx;

			public override string ToString()
			{
				return $"{eax:X} {ebx:X} {ecx:X} {edx:X}";
			}
		}

		[Map]
		[Flags]
		internal enum MmapFlags
		{
			MAP_SHARED = 0x01, // Share changes.
			MAP_PRIVATE = 0x02, // Changes are private.
			MAP_TYPE = 0x0f, // Mask for type of mapping.
			MAP_FIXED = 0x10, // Interpret addr exactly.
			MAP_FILE = 0,
			MAP_ANONYMOUS = 0x20, // Don't use a file.
			MAP_ANON = 0x1000, // OSX specific MAP_ANON

			// These are Linux-specific.
			MAP_GROWSDOWN = 0x00100, // Stack-like segment.
			MAP_DENYWRITE = 0x00800, // ETXTBSY
			MAP_EXECUTABLE = 0x01000, // Mark it as an executable.
			MAP_LOCKED = 0x02000, // Lock the mapping.
			MAP_NORESERVE = 0x04000, // Don't check for reservations.
			MAP_POPULATE = 0x08000, // Populate (prefault) pagetables.
			MAP_NONBLOCK = 0x10000 // Do not block on IO.
		}

		[Map]
		[Flags]
		internal enum MmapProts
		{
			PROT_READ = 0x1, // Page can be read.
			PROT_WRITE = 0x2, // Page can be written.
			PROT_EXEC = 0x4, // Page can be executed.
			PROT_NONE = 0x0, // Page can not be accessed.
			PROT_GROWSDOWN = 0x01000000, // Extend change to start of

			//   growsdown vma (mprotect only).
			PROT_GROWSUP = 0x02000000 // Extend change to start of
			//   growsup vma (mprotect only).
		}

		private static class NativeMethods
		{
			private const string KERNEL = "kernel32.dll";
			private const string LIBC = "libc";

			[DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
			internal static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize,
				AllocationType flAllocationType, MemoryProtection flProtect);

			[DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
			internal static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize,
				FreeType dwFreeType);

			[DllImport(LIBC, SetLastError = true)]
			internal static extern IntPtr mmap(IntPtr start, ulong length,
				MmapProts prot, MmapFlags flags, int fd, long offset);

			[DllImport(LIBC, SetLastError = true)]
			internal static extern int munmap(IntPtr start, ulong length);
		}
	}
}