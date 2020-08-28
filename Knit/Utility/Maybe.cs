using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knit.Utility
{
    internal class Maybe
    {
        private Maybe() { }

        public static Maybe<T> Some<T>(T value) => new Maybe<T>(value);
        public static Maybe None { get; } = new Maybe();
    }

    internal struct Maybe<T> : IEquatable<Maybe<T>>, IEquatable<T>
    {
        public T Value { get; }
        public bool HasValue { get; }

        public static Maybe<T> None => default;

        public Maybe(T value)
        {
            Value = value;
            HasValue = true;
        }

        public bool Equals(Maybe<T> other)
        {
            if (HasValue ^ other.HasValue) return false;
            return !HasValue || EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public bool Equals(T other)
        {
            if (!HasValue) return false;
            return EqualityComparer<T>.Default.Equals(Value, other);
        }

        public static bool operator ==(Maybe<T> a, Maybe<T> b) => a.Equals(b);
        public static bool operator !=(Maybe<T> a, Maybe<T> b) => !(a == b);

        public static bool operator true(Maybe<T> a) => a.HasValue;
        public static bool operator false(Maybe<T> a) => !a.HasValue;
        public static Maybe<T> operator |(Maybe<T> a, Maybe<T> b) => a.HasValue ? a : b;

        public static explicit operator Maybe<T>(T value) => new Maybe<T>(value);
        public static implicit operator Maybe<T>(Maybe _) => None;

        public override bool Equals(object obj)
            => (obj is T t && Equals(t))
            || (obj is Maybe<T> mt && Equals(mt));

        public override int GetHashCode()
        {
            int hashCode = 1816676634;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + HasValue.GetHashCode();
            return hashCode;
        }
    }
}
