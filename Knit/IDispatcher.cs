using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knit
{
    public interface IDispatcher
    {
        void Invoke(Action action);
        T Invoke<T>(Func<T> action);

        DispatcherOperation BeginInvoke(Action action);
        DispatcherOperation<T> BeginInvoke<T>(Func<T> action);
    }
}
