using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knit
{
    public delegate object? ValueGetter(object thisObj);
    public delegate void ValueSetter(object thisObj, object? value);

    public interface IBindingReflector
    {
        Type MemberType(Type type, string member);
        ValueGetter FindGetter(Type type, string member);
        ValueSetter FindSetter(Type type, string member);
    }
}
