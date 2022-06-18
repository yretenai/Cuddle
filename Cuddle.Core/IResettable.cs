namespace Cuddle.Core;

public interface IResettable : IPoliteDisposable {
    void Reset();
}
