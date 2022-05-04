using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Cuddle.Core;

public static class Oodle {
    public static bool IsReady => DecompressDelegate != null;

    private static OodleLZ_Decompress? DecompressDelegate { get; set; }

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryW(string dllname);

    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procname);

    public static Memory<byte> Decompress(Memory<byte> input, int size) {
        if (DecompressDelegate == null) {
            return Memory<byte>.Empty;
        }

        Memory<byte> buffer = new byte[size];
        using var src = input.Pin();
        using var pinned = buffer.Pin();

        unsafe {
            var outSize = DecompressDelegate.Invoke((IntPtr) src.Pointer, input.Length, (IntPtr) pinned.Pointer, buffer.Length, 0, 0, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, 3);
            return buffer[..outSize];
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

        var handle = LoadLibraryW(path);
        if (handle == IntPtr.Zero) {
            return false;
        }

        var address = GetProcAddress(handle, "OodleLZ_Decompress");
        if (address == IntPtr.Zero) {
            return false;
        }

        DecompressDelegate = Marshal.GetDelegateForFunctionPointer<OodleLZ_Decompress>(address);
        return true;
    }

    private delegate int OodleLZ_Decompress(IntPtr srcBuf, int srcSize, IntPtr dstBuf, int dstSize, int fuzz, int crc, int verbose, IntPtr dstBase, int dstBaseSize, IntPtr cb, IntPtr cbContext, IntPtr scratch, uint scratchSize, uint threadPhase);
}
