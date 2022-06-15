using System;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.Structs.Engine;
using Cuddle.Core.Structs.Math;
using Cuddle.Core.Structs.Property;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Objects.Engine;

public record UTexture : UStreamableRenderAsset {
    public UTexture(FArchiveReader data, FObjectExport export) : base(data, export) {
        LightingGuid = GetProperty<Guid>(nameof(LightingGuid));
        LODBias = GetProperty<int>(nameof(LODBias));
        CompressionSettings = GetProperty(nameof(CompressionSettings), defaultValue: FName.Null);
        Filter = GetProperty(nameof(Filter), defaultValue: FName.Null);
        MipLoadOptions = GetProperty(nameof(MipLoadOptions), defaultValue: FName.Null);
        LODGroup = GetProperty(nameof(LODGroup), defaultValue: FName.Null);
        Downscale = GetProperty<FPerPlatformFloat>(nameof(Downscale)) ?? new FPerPlatformFloat(1.0f);
        DownscaleOptions = GetProperty(nameof(DownscaleOptions), defaultValue: FName.Null);
        SRGB = GetProperty<bool>(nameof(SRGB));
        bNoTiling = GetProperty<bool>(nameof(bNoTiling));
        VirtualTextureStreaming = GetProperty<bool>(nameof(VirtualTextureStreaming));
        CompressionYCoCg = GetProperty<bool>(nameof(CompressionYCoCg));
        AssetUserData = GetProperty<ArrayProperty>(nameof(AssetUserData)); // todo: unwrap this.

        // todo.
    }

    public Guid LightingGuid { get; init; }
    public int LODBias { get; init; }
    public FName CompressionSettings { get; init; }
    public FName Filter { get; init; }
    public FName MipLoadOptions { get; init; }
    public FName LODGroup { get; init; }
    public FPerPlatformFloat Downscale { get; init; }
    public FName DownscaleOptions { get; init; }
    public bool SRGB { get; init; }
    public bool bNoTiling { get; init; }
    public bool VirtualTextureStreaming { get; init; }
    public bool CompressionYCoCg { get; init; }
    public ArrayProperty? AssetUserData { get; init; }
}

public record UARTexture : UTexture {
    public UARTexture(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo.
    }
}

public record UCurveLinearColorAtlas : UTexture {
    public UCurveLinearColorAtlas(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo.
    }
}

public record UTexture2D : UTexture {
    public UTexture2D(FArchiveReader data, FObjectExport export) : base(data, export) {
        FirstResourceMemMip = GetProperty<int>(nameof(FirstResourceMemMip));
        AddressX = GetProperty(nameof(AddressX), defaultValue: FName.Null);
        AddressY = GetProperty(nameof(AddressY), defaultValue: FName.Null);
        ImportedSize = GetProperty<FIntPoint>(nameof(ImportedSize));

        // todo.
    }

    public FName AddressX { get; init; }
    public FName AddressY { get; init; }

    public int FirstResourceMemMip { get; init; }
    public FIntPoint ImportedSize { get; init; }
}

public record ULightMapTexture2D : UTexture2D {
    public ULightMapTexture2D(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo.
    }
}

public record UShadowMapTexture2D : UTexture2D {
    public UShadowMapTexture2D(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo.
    }
}

public record UTextureLightProfile : UTexture2D {
    public UTextureLightProfile(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo.
    }
}

public record UVirtualTexture2D : UTexture2D {
    public UVirtualTexture2D(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo.
    }
}

public record ULightMapVirtualTexture2D : UTexture2D {
    public ULightMapVirtualTexture2D(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UTexture2DArray : UTexture {
    public UTexture2DArray(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UTextureCube : UTexture {
    public UTextureCube(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UVolumeTexture : UTexture {
    public UVolumeTexture(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UTextureRenderTarget : UTexture {
    public UTextureRenderTarget(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UTextureRenderTarget2D : UTextureRenderTarget {
    public UTextureRenderTarget2D(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UTextureRenderTarget2DArray : UTextureRenderTarget {
    public UTextureRenderTarget2DArray(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UTextureRenderTargetCube : UTextureRenderTarget {
    public UTextureRenderTargetCube(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UTextureRenderTargetVolume : UTextureRenderTarget {
    public UTextureRenderTargetVolume(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UMediaTexture : UTexture {
    public UMediaTexture(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}

public record UWebBrowserTexture : UTexture {
    public UWebBrowserTexture(FArchiveReader data, FObjectExport export) : base(data, export) {
        // todo
    }
}
