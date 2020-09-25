using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Knit.Utility
{
    internal static class Helpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Assert([DoesNotReturnIf(false)] bool value)
        {
            if (!value) throw new InvalidOperationException("Assertion failed");
        }

        public static object? DefaultForType(Type t)
        {
            var create = Activator.CreateInstance(typeof(DefaultValue<>).MakeGenericType(t)) as IDefaultValue;
            return create?.Default;
        }


        private interface IDefaultValue { object? Default { get; } }
        private class DefaultValue<T> : IDefaultValue
        {
            private static readonly T defVal = default!;
            private static readonly object? defObj = defVal;
            public object? Default => defObj;
        }
    }
}
