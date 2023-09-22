using System.Threading;
using System.Threading.Tasks;

namespace MusicX.Helpers;

public static class WaitHandleExtensions
{
    public static Task WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken = default)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();

        var reg = ThreadPool.RegisterWaitForSingleObject(handle,
            static (state, timedOut) =>
            {
                if (state is not TaskCompletionSource<bool> completionSource)
                    return;
                
                if (timedOut)
                    completionSource.TrySetCanceled();

                completionSource.TrySetResult(true);
            }, taskCompletionSource, -1, true);

        if (cancellationToken != default)
            cancellationToken.Register(() =>
            {
                reg.Unregister(handle);
                taskCompletionSource.TrySetCanceled();
            });

        return taskCompletionSource.Task;
    }
}