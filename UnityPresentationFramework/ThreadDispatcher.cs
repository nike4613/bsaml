using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityPresentationFramework
{
    internal sealed class ThreadDispatcher : IDispatcher, IDisposable
    {
        private readonly Thread thread;
        private readonly ILogger Logger;

        public ThreadDispatcher(ILogger logger)
        {
            Logger = logger;
            thread = new Thread(() => ExecutionThread());
            thread.IsBackground = true;
            thread.Start();
        }

        public DispatcherOperation BeginInvoke(Action action)
        {
            var operation = new Operation<VoidResult>(WrapVoid(action));
            WorkItems.Add(operation); // BeginInvoke is *always* async 
            return operation;
        }

        public DispatcherOperation<T> BeginInvoke<T>(Func<T> action)
        {
            var operation = new Operation<T>(action);
            WorkItems.Add(operation); // BeginInvoke is *always* async 
            return operation;
        }

        public void Invoke(Action action)
        {
            var operation = new Operation<VoidResult>(WrapVoid(action));
            StartOrInvokeInline(operation);
            operation.GetResult();
        }

        public T Invoke<T>(Func<T> action)
        {
            var operation = new Operation<T>(action);
            StartOrInvokeInline(operation);
            return operation.GetResult();
        }

        private static Func<VoidResult> WrapVoid(Action action)
            => () =>
            {
                action();
                return new VoidResult();
            };

        private void StartOrInvokeInline(IOperation operation)
        {
            if (Thread.CurrentThread.ManagedThreadId == thread.ManagedThreadId)
            {
                operation.Invoke(Logger);
            }
            else
            {
                WorkItems.Add(operation);
            }
        }

        private readonly BlockingCollection<IOperation> WorkItems = new BlockingCollection<IOperation>();

        private void ExecutionThread()
        {
            while (WorkItems.TryTake(out var operation, Timeout.Infinite))
            {
                operation.Invoke(Logger);
            }
        }

        public void Dispose()
        {
            try
            {
                WorkItems.CompleteAdding();
                thread.Join();
            }
            finally
            {
                WorkItems.Dispose();
            }
        }

        private interface IOperation
        {
            ManualResetEventSlim WaitHandle { get; }
            void Invoke(ILogger logger);
            IEnumerable<Action> Continuations { get; }
        }

        private struct VoidResult { }

        private class Operation<T> : DispatcherOperation<T>, IOperation
        {
            private bool isCompleted = false;
            public override bool IsCompleted => isCompleted;

            public ManualResetEventSlim WaitHandle { get; } = new ManualResetEventSlim(false);

            private readonly ConcurrentBag<Action> continuations = new ConcurrentBag<Action>();
            public IEnumerable<Action> Continuations => continuations;

            private readonly Func<T> action;
            private ExceptionDispatchInfo? exception = null;
            private T result = default!;

            public Operation(Func<T> action)
                => this.action = action;

            public void Invoke(ILogger logger)
            {
                try
                {
                    result = action();
                }
                catch (Exception e)
                {
                    exception = ExceptionDispatchInfo.Capture(e);
                    if (continuations.Count == 0)
                    {
                        logger.Error(exception.SourceException, "Exception in orphaned DispatcherOperation");
                    }
                }
                finally
                {
                    isCompleted = true;
                    WaitHandle.Set();
                    foreach (var @continue in Continuations)
                        @continue();
                }
            }

            public override T GetResult()
            {
                if (!IsCompleted)
                    WaitHandle.Wait();

                if (exception != null)
                    exception.Throw();

                return result;
            }

            protected override void RegisterCompletionAction(Action continuation)
            {
                continuations.Add(continuation); // we'll always add
                if (IsCompleted) // but if this is already completed, we will just call the completion directly
                    continuation();
            }
        }
    }
}
