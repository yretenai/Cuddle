using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Cuddle.Core;

public static class Oodle {
    // this has va_args, which is not possible to process in c# so you should probably use a native trampoline instead.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OodleCore_Plugin_Printf(int verboseLevel, [MarshalAs(UnmanagedType.LPStr)] string file, int line, [MarshalAs(UnmanagedType.LPStr)] string fmt);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void* OodleCore_Plugins_SetPrintf([MarshalAs(UnmanagedType.FunctionPtr)] OodleCore_Plugin_Printf fp_rrRawPrintf);

    public unsafe delegate OodleDecompressCallbackRet OodleDecompressCallback(void* userdata, void* rawBuf, int rawLen, void* compBuf, int compBufferSize, int rawDone, int compUsed);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate int OodleLZ_Decompress(void* srcBuf, int srcSize, void* rawBuf, int rawSize, int fuzzSafe = 1, int checkCRC = 0, OodleLZ_Verbosity verbosity = OodleLZ_Verbosity.None, void* decBufBase = null, int decBufSize = 0, void* fpCallback = null, void* callbackUserData = null, void* decoderMemory = null, int decoderMemorySize = 0, OodleLZ_Decode_ThreadPhase threadPhase = OodleLZ_Decode_ThreadPhase.Unthreaded);

    public enum OodleDecompressCallbackRet {
        Continue = 0,
        Cancel = 1,
        Invalid = 2,
    }

    public enum OodleLZ_Decode_ThreadPhase {
        ThreadPhase1 = 1,
        ThreadPhase2 = 2,
        ThreadPhaseAll = 3,
        Unthreaded = ThreadPhaseAll,
    }

    public enum OodleLZ_Verbosity {
        None = 0,
        Minimal = 1,
        Some = 2,
        Lots = 3,
    }

    public static bool IsReady => DecompressDelegate != null;

    public static OodleLZ_Decompress? DecompressDelegate { get; set; }
    public static OodleCore_Plugins_SetPrintf? SetPrintfDelegate { get; set; }

    public static int Decompress(Memory<byte> input, Memory<byte> output) {
        if (DecompressDelegate == null) {
            return -1;
        }

        using var inPin = input.Pin();
        using var outPin = output.Pin();

        unsafe {
            return DecompressDelegate.Invoke(inPin.Pointer, input.Length, outPin.Pointer, output.Length);
        }
    }

    public static string OodleLibName {
        get {
            if (OperatingSystem.IsWindows()) {
                return "oo2core_*_win64.dll";
            }

            if (OperatingSystem.IsLinux()) {
                return "oo2core_*_linux64.so";
            }

            if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst()) {
                return "oo2core_*_mac64.dylib";
            }

            throw new PlatformNotSupportedException();
        }
    }

    public static bool Load(string? path) {
        if (!OperatingSystem.IsWindows()) { // todo: investigate oodle decompressors for linux (ooz?)
            return false;
        }

        if (Directory.Exists(path)) {
            var files = Directory.GetFiles(path, OodleLibName, SearchOption.TopDirectoryOnly);
            if (files.Length == 0) {
                return false;
            }

            path = files[0];
        }

        if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
            return false;
        }

        var handle = NativeLibrary.Load(path);
        if (handle == IntPtr.Zero) {
            return false;
        }

        var address = NativeLibrary.GetExport(handle, nameof(OodleLZ_Decompress));
        if (address == IntPtr.Zero) {
            return false;
        }

        DecompressDelegate = Marshal.GetDelegateForFunctionPointer<OodleLZ_Decompress>(address);

        address = NativeLibrary.GetExport(handle, nameof(OodleCore_Plugins_SetPrintf));
        if (address == IntPtr.Zero) {
            return true;
        }

#pragma warning disable CA1420
        SetPrintfDelegate = Marshal.GetDelegateForFunctionPointer<OodleCore_Plugins_SetPrintf>(address);
#pragma warning restore CA1420
        return true;
    }
}
