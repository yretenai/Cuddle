using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Cuddle.Core;

public sealed class Oodle : IDisposable {
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

    public OodleLZ_Decompress DecompressDelegate { get; set; }
    public IntPtr OodleNative { get; private set; }

    public int Decompress(Memory<byte> input, Memory<byte> output) {
        using var inPin = input.Pin();
        using var outPin = output.Pin();

        unsafe {
            return DecompressDelegate.Invoke(inPin.Pointer, input.Length, outPin.Pointer, output.Length);
        }
    }

    ~Oodle() {
        NativeLibrary.Free(OodleNative);
    }

    public static string OodleLibName {
        get {
            if (OperatingSystem.IsWindows()) {
                return "oo2core*win64.dll";
            }

            if (OperatingSystem.IsLinux()) {
                return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "oo2core*linuxarm64.so" : "oo2core*linux64.so";
            }

            if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst()) {
                return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "oo2core*macarm64.dylib" : "oo2core*mac64.dylib";
            }

            throw new PlatformNotSupportedException();
        }
    }

    public Oodle(string? path) {
        if (Directory.Exists(path)) {
            var files = Directory.GetFiles(path, OodleLibName, SearchOption.TopDirectoryOnly);
            if (files.Length == 0) {
                throw new FileNotFoundException("Could not find Oodle library in path", path);
            }

            path = files[0];
        }

        if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
            throw new FileNotFoundException("Could not find Oodle library", path);
        }

        OodleNative = NativeLibrary.Load(path);

#pragma warning disable CA1420
        DecompressDelegate = Marshal.GetDelegateForFunctionPointer<OodleLZ_Decompress>(NativeLibrary.GetExport(OodleNative, nameof(OodleLZ_Decompress)));
#pragma warning restore CA1420
    }

    public void Dispose() {
        if (OodleNative != IntPtr.Zero) {
            NativeLibrary.Free(OodleNative);
            OodleNative = IntPtr.Zero;
        }

        GC.SuppressFinalize(this);
    }
}
