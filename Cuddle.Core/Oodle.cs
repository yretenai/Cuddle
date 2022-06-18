using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Cuddle.Core;

public static class Oodle {
    public static bool IsReady => DecompressDelegate != null;

    private static OodleLZ_Decompress? DecompressDelegate { get; set; }

    private static class NativeMethods {

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr LoadLibraryW(string dllname);

#pragma warning disable CA2101
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procname);
#pragma warning restore CA2101
    }

    public static int Decompress(Memory<byte> input, Memory<byte> output) {
        if (DecompressDelegate == null) {
            return -1;
        }

        using var inPin = input.Pin();
        using var outPin = output.Pin();

        unsafe {
            return DecompressDelegate.Invoke((IntPtr) inPin.Pointer, input.Length, (IntPtr) outPin.Pointer, output.Length, 0, 0, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, 3);
        }
    }

    public static bool Load(string? path) {
        if (!OperatingSystem.IsWindows()) { // todo: investigate oodle decompressors for linux (ooz?)
            return false;
        }

        if (Directory.Exists(path)) {
            var files = Directory.GetFiles(path, "oo2core_*_win64.dll");
            if (files.Length == 0) {
                return false;
            }

            path = files[0];
        }

        if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
            return false;
        }

        var handle = NativeMethods.LoadLibraryW(path);
        if (handle == IntPtr.Zero) {
            return false;
        }

        var address = NativeMethods.GetProcAddress(handle, "OodleLZ_Decompress");
        if (address == IntPtr.Zero) {
            return false;
        }

        DecompressDelegate = Marshal.GetDelegateForFunctionPointer<OodleLZ_Decompress>(address);
        return true;
    }

    private delegate int OodleLZ_Decompress(IntPtr srcBuf, int srcSize, IntPtr dstBuf, int dstSize, int fuzz, int crc, int verbose, IntPtr dstBase, int dstBaseSize, IntPtr cb, IntPtr cbContext, IntPtr scratch, uint scratchSize, uint threadPhase);
}
