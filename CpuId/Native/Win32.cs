﻿namespace RJCP.Diagnostics.Native
{
    using System;
    using System.IO;
    using static Kernel32;

    internal static class Win32
    {
        public static SafeLibraryHandle LoadLibrary<T>(string fileName)
        {
            Uri assemblyLocation = new Uri(typeof(T).Assembly.Location);
            string libraryPath = Path.GetDirectoryName(assemblyLocation.LocalPath);

            if (IntPtr.Size == 4) {
                libraryPath = Path.Combine(libraryPath, "x86", fileName);
            } else {
                libraryPath = Path.Combine(libraryPath, "x64", fileName);
            }

            return LoadLibraryEx(libraryPath, IntPtr.Zero, LoadLibraryFlags.None);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2589:Boolean expressions should not be gratuitous", Justification = "False Positive")]
        public static OSArchitecture GetArchitecture()
        {
            OSArchitecture architecture;
            bool nativeSystemInfo;

            SYSTEM_INFO lpSystemInfo = new SYSTEM_INFO();

            // GetNativeSystemInfo is independent if we're 64-bit or not But it needs _WIN32_WINNT 0x0501
            try {
                GetNativeSystemInfo(ref lpSystemInfo);
                architecture = (OSArchitecture)lpSystemInfo.uProcessorInfo.wProcessorArchitecture;
                nativeSystemInfo = true;
            } catch {
                architecture = OSArchitecture.Unknown;
                nativeSystemInfo = false;
            }

            if (architecture == OSArchitecture.Unknown || !nativeSystemInfo) {
                try {
                    GetSystemInfo(ref lpSystemInfo);
                    architecture = (OSArchitecture)lpSystemInfo.uProcessorInfo.wProcessorArchitecture;
                } catch {
                    architecture = OSArchitecture.Unknown;
                }
            }

            // We try to determine if we're a WOW64 process if we don't know the architecture or if we're x86 and
            // NativeSystemInfo didn't work.
            switch (architecture) {
            case OSArchitecture.x64:
            case OSArchitecture.x86:
            case OSArchitecture.x86_x64:
                if (IsWow64())
                    architecture = OSArchitecture.x86_x64;
                break;
            }

            return architecture;
        }

        private static bool IsWow64()
        {
            try {
                bool wow64 = false;
                if (IsWow64Process(GetCurrentProcess(), ref wow64))
                    return wow64;
                return false;
            } catch (EntryPointNotFoundException) {
                return false;
            }
        }
    }
}
