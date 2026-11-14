using System.Threading;
using System.Threading.Tasks;

namespace HttpStatusExtention.Models
{
    internal static class TaskUtils
    {
        /// <summary>
        /// Waits for <paramref name="task"/> and returns whether it completed before the
        /// cancellation token was cancelled. Awaiting this task never blocks the caller.
        /// Faults from the task are observed by awaiting it before returning.
        /// </summary>
        public static async Task<bool> WaitAsync(Task task, CancellationToken token)
        {
            if (task.IsCompleted) {
                await task;
                return true;
            }

            if (!token.CanBeCanceled) {
                await task;
                return true;
            }

            var cancelled = new TaskCompletionSource<bool>();
            using (token.Register(() => _ = cancelled.TrySetResult(true))) {
                if (await Task.WhenAny(task, cancelled.Task) == task) {
                    await task;
                    return true;
                }
                return false;
            }
        }
    }
}
