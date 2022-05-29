using System;

namespace Cuddle.Core;

public interface IPoliteDisposable : IDisposable {
    bool Disposed { get; }
}
