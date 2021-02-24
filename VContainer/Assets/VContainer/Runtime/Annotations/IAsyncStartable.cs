#if VCONTAINER_UNITASK_INTEGRATION
using System.Threading;
using Cysharp.Threading.Tasks;

namespace VContainer.Unity
{
    public interface IAsyncStartable
    {
        UniTask StartAsync(CancellationToken cancellation);
    }
}
#endif
