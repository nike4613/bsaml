using IPA.Utilities;
using Knit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSAML
{
    internal class BSIPAAccessorReflector : IBindingReflector
    {
        private readonly Dictionary<(Type type, string member), (ValueGetter get, ValueSetter set)> accessCache = new Dictionary<(Type type, string member), (ValueGetter get, ValueSetter set)>();

        public ValueGetter FindGetter(Type type, string member)
            => GetAccessors(type, member).get;

        public ValueSetter FindSetter(Type type, string member)
            => GetAccessors(type, member).set;

        private (ValueGetter get, ValueSetter set) GetAccessors(Type type, string member)
        {
            if (!accessCache.TryGetValue((type, member), out var accessors))
            {
                var memberInfo = FindMember(type, member) ?? throw new MissingMemberException(type.FullName, member);

                var accessGenericType = memberInfo.isProp ? typeof(GetPropertyAccessors<,>) : typeof(GetFieldAccessors<,>);
                var accessType = accessGenericType.MakeGenericType(memberInfo.member.DeclaringType, memberInfo.type);
                var access = (IGetAccessors)Activator.CreateInstance(accessType);

                accessCache.Add((type, member), accessors = access.GetAccessors(member));
            }

            return accessors;
        }

        private interface IGetAccessors
        {
            (ValueGetter get, ValueSetter set) GetAccessors(string member);
        }

        private class GetFieldAccessors<TOwning, TMember> : IGetAccessors
        {
            public (ValueGetter get, ValueSetter set) GetAccessors(string member)
            {
                var access = FieldAccessor<TOwning, TMember>.GetAccessor(member);
                return (
                    o =>
                    {
                        var obj = (TOwning)o;
                        return access(ref obj);
                    }, 
                    (o, v) =>
                    {
                        var obj = (TOwning)o;
                        var val = (TMember)v;
                        access(ref obj) = val!;
                    }
                );
            }
        }
        private class GetPropertyAccessors<TOwning, TMember> : IGetAccessors
        {
            public (ValueGetter get, ValueSetter set) GetAccessors(string member)
            {
                var get = PropertyAccessor<TOwning, TMember>.GetGetter(member);
                var set = PropertyAccessor<TOwning, TMember>.GetSetter(member);
                return (
                    o =>
                    {
                        var obj = (TOwning)o;
                        return get(ref obj);
                    },
                    (o, v) =>
                    {
                        var obj = (TOwning)o;
                        var val = (TMember)v;
                        set(ref obj, val!);
                    }
                );
            }
        }

        public Type MemberType(Type type, string member)
        {
            var result = FindMember(type, member);
            if (result == null)
                throw new MissingMemberException(type.FullName, member);
            return result.Value.type;
        }

        private static (MemberInfo member, Type type, bool isProp)? FindMember(Type type, string member)
        {
            var prop = type.GetProperty(member, AccessFlags);
            if (prop != null)
                return (prop, prop.PropertyType, true);
            var field = type.GetField(member, AccessFlags);
            if (field != null)
                return (field, field.FieldType, false);
            return null;
        }

        private const BindingFlags AccessFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
    }
}
