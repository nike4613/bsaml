using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Knit.Utility
{
    internal class Helpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Assert([DoesNotReturnIf(false)] bool value)
        {
            if (!value) throw new InvalidOperationException("Assertion failed");
        }
    }
}
