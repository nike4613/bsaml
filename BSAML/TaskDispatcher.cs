using IPA.Utilities.Async;
using Knit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BSAML
{
    internal class TaskDispatcher : IDispatcher
    {
        public TaskFactory Factory { get; }
        public TaskDispatcher(TaskFactory factory)
            => Factory = factory;

        public DispatcherOperation BeginInvoke(Action action)
            => CreateOp(Factory.StartNew(Wrap(action)));

        public DispatcherOperation<T> BeginInvoke<T>(Func<T> action)
            => CreateOp(Factory.StartNew(action));

        public void Invoke(Action action)
            => Factory.StartNew(action).Wait();

        public T Invoke<T>(Func<T> action)
            => Factory.StartNew(action).Result;

        private static Func<VoidType> Wrap(Action a)
            => () => { a(); return default; };

        private static TaskOperation<T> CreateOp<T>(Task<T> task)
            => new TaskOperation<T>(task);

        private struct VoidType { }
        private class TaskOperation<T> : DispatcherOperation<T>
        {
            public Task<T> Task { get; }
            public TaskAwaiter<T> TaskAwaiter { get; }
            public TaskOperation(Task<T> task)
            {
                Task = task;
                TaskAwaiter = Task.GetAwaiter();
            }

            public override bool IsCompleted => TaskAwaiter.IsCompleted;

            public override T GetResult() => TaskAwaiter.GetResult();

            protected override void RegisterCompletionAction(Action continuation)
                => TaskAwaiter.OnCompleted(continuation);
        }
    }
}
