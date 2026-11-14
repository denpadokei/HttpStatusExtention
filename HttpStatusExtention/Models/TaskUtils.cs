using System.Threading;
using System.Threading.Tasks;

namespace HttpStatusExtention.Models
{
    internal static class TaskUtils
    {
        /// <summary>
        /// Waits for <paramref name="task"/> and returns whether it completed before the
        /// cancellation token was cancelled. Awaiting this task never blocks the caller.
        /// </summary>
        public static async Task<bool> WaitAsync(Task task, CancellationToken token)
        {
            if (task.IsCompleted) {
                return true;
            }

            if (!token.CanBeCanceled) {
                await task;
                return true;
            }

            var cancelled = new TaskCompletionSource<bool>();
            using (token.Register(() => _ = cancelled.TrySetResult(true))) {
                return await Task.WhenAny(task, cancelled.Task) == task;
            }
        }
    }
}
