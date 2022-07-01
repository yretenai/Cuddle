namespace Cuddle.Core.Structs.Asset;

public enum EPayloadAccessMode : byte {
    /**
     * The payload is stored in the Payload Data segment of the trailer and the offsets in FLookupTableEntry will be relative to the start of this segment
     */
    Local = 0,

    /**
     * The payload is stored in another package trailer (most likely the workspace domain package file) and the offsets in FLookupTableEntry are absolute offsets in that external file
     */
    Referenced,

    /**
     * The payload is virtualized and needs to be accessed via IVirtualizationSystem
     */
    Virtualized,
}
