using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnityPresentationFramework
{
    internal class SystemReflectionReflector : IBindingReflector
    {
        public Type MemberType(Type type, string member)
        {
            var prop = type.GetProperty(member, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (prop != null)
                return prop.PropertyType;
            var field = type.GetField(member, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (field != null)
                return field.FieldType;

            throw new MissingMemberException(type.FullName, member);
        }

        public ValueGetter FindGetter(Type type, string member)
        {
            var prop = type.GetProperty(member, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (prop != null)
                return GetterForProperty(prop);
            var field = type.GetField(member, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (field != null)
                return GetterForField(field);

            throw new MissingMemberException(type.FullName, member);
        }

        public ValueSetter FindSetter(Type type, string member)
        {
            var prop = type.GetProperty(member, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (prop != null)
                return SetterForProperty(prop);
            var field = type.GetField(member, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (field != null)
                return SetterForField(field);

            throw new MissingMemberException(type.FullName, member);
        }

        private static ValueGetter GetterForProperty(PropertyInfo prop)
            => self => prop.GetValue(self);
        private static ValueSetter SetterForProperty(PropertyInfo prop)
            => (self, value) => prop.SetValue(self, value);
        private static ValueGetter GetterForField(FieldInfo field)
            => self => field.GetValue(self);
        private static ValueSetter SetterForField(FieldInfo field)
            => (self, value) => field.SetValue(self, value);

    }
}
