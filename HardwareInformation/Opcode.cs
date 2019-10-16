#region using

using System;
using System.Runtime.InteropServices;
using Mono.Unix.Native;

#endregion

namespace HardwareInformation
{
	public static class Opcode
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate bool CpuidDelegate(out Result result, uint eax, uint ecx);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate ulong RdtscDelegate();

		[Flags]
		public enum AllocationType : uint
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
		public enum FreeType
		{
			DECOMMIT = 0x4000,
			RELEASE = 0x8000
		}

		[Flags]
		public enum MemoryProtection : uint
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

		private static IntPtr codeBuffer;
		private static ulong size;

		public static RdtscDelegate Rdtsc;

		// unsigned __int64 __stdcall rdtsc() {
		//   return __rdtsc();
		// }

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

		public static CpuidDelegate Cpuid;


		// void __stdcall cpuidex(unsigned int index, unsigned int ecxValue, 
		//   unsigned int* eax, unsigned int* ebx, unsigned int* ecx, 
		//   unsigned int* edx)
		// {
		//   int info[4];	
		//   __cpuidex(info, index, ecxValue);
		//   *eax = info[0];
		//   *ebx = info[1];
		//   *ecx = info[2];
		//   *edx = info[3];
		// }

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

		public static bool IsOpen { get; private set; }

		public static void Open()
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

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				// Unix   
				codeBuffer = Syscall.mmap(IntPtr.Zero, size,
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

		public static void Close()
		{
			Rdtsc = null;
			Cpuid = null;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// Unix
				Syscall.munmap(codeBuffer, size);
			}
			else
			{
				// Windows
				NativeMethods.VirtualFree(codeBuffer, UIntPtr.Zero,
					FreeType.RELEASE);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Result
		{
			public uint eax;
			public uint ebx;
			public uint ecx;
			public uint edx;

			public override string ToString()
			{
				return $"{eax:X} {ebx:X} {ecx:X} {edx:X}";
			}
		}

		private static class NativeMethods
		{
			private const string KERNEL = "kernel32.dll";

			[DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
			public static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize,
				AllocationType flAllocationType, MemoryProtection flProtect);

			[DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
			public static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize,
				FreeType dwFreeType);
		}
	}
}