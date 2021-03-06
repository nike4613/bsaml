﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class DoesNotReturnIfAttribute : Attribute
    {
        public bool ParameterValue { get; }
        public DoesNotReturnIfAttribute(bool value) => ParameterValue = value;
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        public bool ReturnValue { get; }
        public MaybeNullWhenAttribute(bool result) => ReturnValue = result;
    }

}
