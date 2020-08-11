using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPresentationFramework
{
    public abstract class Bindable<T>
    {
        public abstract T Value { get; }

        public static implicit operator T(Bindable<T> bindable) => bindable.Value;
        public static implicit operator Bindable<T>(T value) => new LiteralBindable<T>(value);
    }

    internal sealed class LiteralBindable<T> : Bindable<T>
    {
        public override T Value { get; }

        public LiteralBindable(T value) => Value = value;
    }
}
