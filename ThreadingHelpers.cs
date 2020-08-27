using System;
using System.Threading;

namespace ThreadPoolExercises.Core
{
    public class ThreadingHelpers
    {
        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        public static void ExecuteOnThread(Action action, int repeats, CancellationToken token = default, Action<Exception>? errorAction = null)
        {
            // * Create a thread and execute there `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `Join` to wait until created Thread finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)

            var thread = new Thread(() => DoWorkOnThread(action, repeats, token, errorAction));
            thread.Start();
            thread.Join();
        }

        static void DoWorkOnThread(Action action, int repeats, CancellationToken token, Action<Exception>? errorAction)
        {
            if (action == null) return;

            try
            {
                if (repeats < 0) throw new ArgumentException("Number of repeats must be greater or equal to 0.", nameof(repeats));

                for (var repeated = 0; repeated < repeats; repeated++)
                {
                    token.ThrowIfCancellationRequested();
                    action();
                }
            }
            catch (Exception ex)
            {
                errorAction?.Invoke(ex);
            }
        }

        public static void ExecuteOnThreadPool(Action action, int repeats, CancellationToken token = default, Action<Exception>? errorAction = null)
        {
            // * Queue work item to a thread pool that executes `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `AutoResetEvent` to wait until the queued work item finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)

            ThreadPool.QueueUserWorkItem(DoWorkOnThreadPool, new DoWorkOnThreadPoolParameters(action, repeats, token, errorAction));
            autoResetEvent.WaitOne();
        }

        static void DoWorkOnThreadPool(object? arg)
        {
            if (arg == null) return;
            if (!(arg is DoWorkOnThreadPoolParameters doWorkOnThreadPoolParameters)) throw new ArgumentException($"Incorrect method's argument type ({arg.GetType()})", nameof(arg));

            try
            {
                DoWorkOnThread(doWorkOnThreadPoolParameters.Action, doWorkOnThreadPoolParameters.Repeats, doWorkOnThreadPoolParameters.Token, doWorkOnThreadPoolParameters.ErrorAction);
            }
            finally
            {
                autoResetEvent.Set();
            }
        }

        private class DoWorkOnThreadPoolParameters
        {
            public DoWorkOnThreadPoolParameters(Action action, int repeats, CancellationToken token, Action<Exception>? errorAction)
            {
                Action = action;
                Repeats = repeats;
                Token = token;
                ErrorAction = errorAction;
            }

            public Action Action { get; }
            public int Repeats { get; }
            public CancellationToken Token { get; }
            public Action<Exception>? ErrorAction { get; }
        }
    }
}
