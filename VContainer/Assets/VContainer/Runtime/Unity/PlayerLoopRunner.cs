using VContainer.Internal;

namespace VContainer.Unity
{
    interface IPlayerLoopItem
    {
        bool MoveNext();
    }

    sealed class PlayerLoopRunner
    {
        readonly FreeList<IPlayerLoopItem> runners = new FreeList<IPlayerLoopItem>(16);

        int running;

        public void Dispatch(IPlayerLoopItem item)
        {
            runners.Add(item);
        }

        public void Run()
        {
            var span = runners.AsSpan();
            for (var i = 0; i < span.Length; i++)
            {
                var item = span[i];
                if (item != null)
                {
                    var continued = item.MoveNext();
                    if (!continued)
                    {
                        runners.RemoveAt(i);
                    }
                }
            }
        }
    }
}
