namespace Cuddle.Core.Structs.Asset;

public enum EObjectVersionUE5 {
    INITIAL_VERSION_UE5 = 1000,

    // Support stripping names that are not referenced from export data
    NAMES_REFERENCED_FROM_EXPORT_DATA,

    // Added a payload table of contents to the package summary
    PAYLOAD_TOC,

    // Added data to identify references from and to optional package
    OPTIONAL_RESOURCES,

    // Large world coordinates converts a number of core types to double components by default.
    LARGE_WORLD_COORDINATES,

    // Remove package GUID from FObjectExport
    REMOVE_OBJECT_EXPORT_PACKAGE_GUID,

    // Add IsInherited to the FObjectExport entry
    TRACK_OBJECT_EXPORT_IS_INHERITED,

    NEWEST_LOADABLE_PACKAGE = TRACK_OBJECT_EXPORT_IS_INHERITED,
}
