using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Knit
{
    public abstract class DispatcherOperation
    {
        public abstract bool IsCompleted { get; }
        protected abstract void RegisterCompletionAction(Action continuation);
        public abstract void CheckResult();

        public Awaiter GetAwaiter() => new Awaiter(this);

        public struct Awaiter : INotifyCompletion
        {
            private readonly DispatcherOperation operation;

            public Awaiter(DispatcherOperation operation)
                => this.operation = operation;

            public bool IsCompleted => operation.IsCompleted;

            public void OnCompleted(Action continuation)
                => operation.RegisterCompletionAction(continuation);

            public void GetResult()
                => operation.CheckResult();
        }
    }

    public abstract class DispatcherOperation<T> : DispatcherOperation
    {
        public sealed override void CheckResult()
            => GetResult();

        public abstract T GetResult();

        public new Awaiter GetAwaiter() => new Awaiter(this);

        public new struct Awaiter : INotifyCompletion
        {
            private readonly DispatcherOperation<T> operation;

            public Awaiter(DispatcherOperation<T> operation)
                => this.operation = operation;

            public bool IsCompleted => operation.IsCompleted;

            public void OnCompleted(Action continuation)
                => operation.RegisterCompletionAction(continuation);

            public T GetResult()
                => operation.GetResult();
        }
    }

}
