using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.System;

namespace NurirobotMotorSimulator.Helpers
{
    public static class DispatcherQueueExtensions
    {
        private static readonly bool IsHasThreadAccessPropertyAvailable = ApiInformation.IsMethodPresent("Windows.System.DispatcherQueue", "HasThreadAccess");

        public static Task EnqueueAsync(this DispatcherQueue dispatcher, Action function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            // Run the function directly when we have thread access.
            // Also reuse Task.CompletedTask in case of success,
            // to skip an unnecessary heap allocation for every invocation.
            if (IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
            {
                try
                {
                    function();

                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    return Task.FromException(e);
                }
            }

            return TryEnqueueAsync(dispatcher, function, priority);
        }

        private static Task TryEnqueueAsync(DispatcherQueue dispatcher, Action function, DispatcherQueuePriority priority)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            if (!dispatcher.TryEnqueue(priority, () =>
            {
                try
                {
                    function();

                    taskCompletionSource.SetResult(null);
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            }))
            {
                taskCompletionSource.SetException(GetEnqueueException("Failed to enqueue the operation"));
            }

            return taskCompletionSource.Task;
        }

        private static InvalidOperationException GetEnqueueException(string message)
        {
            return new InvalidOperationException(message);
        }
    }
}
