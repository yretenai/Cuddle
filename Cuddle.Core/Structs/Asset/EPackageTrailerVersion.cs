namespace Cuddle.Core.Structs.Asset;

public enum EPackageTrailerVersion {
    // The original trailer format when it was first added
    INITIAL = 0,

    // Access mode is now per payload and found in FLookupTableEntry
    ACCESS_PER_PAYLOAD = 1,

    // Added EPayloadAccessMode to FLookupTableEntry
    PAYLOAD_FLAGS = 2,
}
