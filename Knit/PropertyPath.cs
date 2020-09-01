using Knit.Utility;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Knit
{
    [DebuggerDisplay("{System.String.Join(\".\", Components)}")]
    public class PropertyPath
    {
        private readonly ComponentContainer components;
        private readonly IBindingReflector reflector;
        private readonly ILogger logger;

        public PropertyPath(IEnumerable<string> components, IServiceProvider services)
        {
            var parts = components.ToArray();
            if (parts.Length < 1)
                throw new ArgumentException("PropertyPath must have at least one component", nameof(components));
            reflector = services.GetRequiredService<IBindingReflector>();
            logger = services.GetRequiredService<ILogger>().ForContext<PropertyPath>();
            this.components = new ComponentContainer(parts, logger);
        }


        private struct ComponentContainer
        {
            private readonly string[] components;
            private readonly string[] componentNames;
            private readonly uint[] componentNullProp;
            public ComponentContainer(string[] parts, ILogger logger)
            {
                components = parts;
                (componentNames, componentNullProp) = PreprocessComponents(parts, logger.ForContext<ComponentContainer>());
            }

            private static (string[], uint[]) PreprocessComponents(string[] parts, ILogger logger)
            {
                var names = new string[parts.Length];
                var nullParts = new uint[(parts.Length + 7) / 8];

                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];
                    if (part[part.Length - 1] == '?')
                    {
                        names[i] = part.Substring(0, part.Length - 1);

                        if (i != parts.Length - 1)
                        {
                            nullParts[(i + 1) / 8] |= 1u >> ((i + 1) % 8);
                        }
                        else
                        {
                            logger.Warning("Last component {FullName} has null propagation operator", part);
                        }
                    }
                    else
                    {
                        names[i] = part;
                    }
                }

                return (names, nullParts);
            }

            public IEnumerable<string> Components => components;

            public int Length => components.Length;

            public (string name, bool nullProp) GetForIdx(int idx)
                => (componentNames[idx], (componentNullProp[idx / 8] & (1u >> (idx % 8))) != 0);
        }

        public IEnumerable<string> Components => components.Components;

        private const int maxCacheSize = 4;
        private readonly LinkedList<CacheEntry> typeCache = new LinkedList<CacheEntry>();
        private CacheEntry lastEntry;
    
        private struct CacheEntry
        {
            public Type RootType;
            public Type TargetType;
            public ValueGetter Getter;
            public ValueSetter Setter;
            public ValueGetter[] Stages;
            public object? DefaultValue;
        }
        
        public object? GetValue(object target, bool defaultIfNull = true)
        {
            var entry = ForTargetObject(target);
            var result = entry.Getter(target);
            if (defaultIfNull) result ??= entry.DefaultValue;
            return result;
        }

        public void SetValue(object target, object? value, bool defaultIfNull = true)
        {
            var entry = ForTargetObject(target);
            if (defaultIfNull) value ??= entry.DefaultValue;
            entry.Setter(target, value);
        }

        private struct Empty { }
        private readonly ConditionalWeakTable<object, ConcurrentDictionary<string, SubscribedProperty>> subscribedProperties
            = new ConditionalWeakTable<object, ConcurrentDictionary<string, SubscribedProperty>>();

        private class SubscribedProperty
        {
            public readonly ConcurrentDictionary<ChangeHandler, Empty> Set = new ConcurrentDictionary<ChangeHandler, Empty>();
            public PropertyChangedEventHandler? ExecutingHandler;
        }

        public delegate void ChangeHandler(object source, object? value);

        public bool AddChangedHandler(object target, ChangeHandler handler)
        {
            var entry = ForTargetObject(target);

            bool result = false;
            object? obj = target;
            for (int i = 0; i < components.Length; i++)
            {
                result |= TryAddLayerChangedHandler(entry, i, obj, handler);
                var (_, nullProp) = components.GetForIdx(i);
                obj = nullProp ? PropagateN(obj, entry.Stages[i]) : PropagateNR(obj, entry.Stages[i]);
                if (obj == null)
                    break;
            }

            return result;
        }

        private bool TryAddLayerChangedHandler(
            in CacheEntry entry,
            int layer, 
            object obj,
            ChangeHandler handler
        )
        {
            if (obj is INotifyPropertyChanged notify)
            {
                var (propName, nullProp) = components.GetForIdx(layer);
                var map = subscribedProperties.GetOrCreateValue(obj);
                var handlers = map.GetOrAdd(propName, _ => new SubscribedProperty());

                if (handlers.Set.Count == 0)
                {
                    var stages = entry.Stages;
                    handlers.ExecutingHandler = (sender, args) =>
                    {
                        if (map.TryGetValue(args.PropertyName, out var prop))
                        {
                            var obj = sender;
                            for (int i = layer; i < components.Length; i++)
                            {
                                var (_, nullProp) = components.GetForIdx(i);
                                obj = nullProp ? PropagateN(obj, stages[i]) : PropagateNR(obj, stages[i]);
                            }
                            foreach (var handler in prop.Set)
                                handler.Key(sender, obj);
                        }
                    };
                    notify.PropertyChanged += handlers.ExecutingHandler;
                }
                handlers.Set.AddOrUpdate(handler, new Empty(), (h, e) => e);

                return true;
            }

            return false;
        }

        public void RemoveChangedHandler(object target, ChangeHandler handler)
        {
            var entry = ForTargetObject(target);

            object? obj = target;
            for (int i = 0; i < components.Length - 1; i++)
            {
                TryRemoveLayerChangedHandler(handler, i, obj);
                var (_, nullProp) = components.GetForIdx(i);
                obj = nullProp ? PropagateN(obj, entry.Stages[i]) : PropagateNR(obj, entry.Stages[i]);
                if (obj == null)
                    break;
            }
        }

        private bool TryRemoveLayerChangedHandler(
            ChangeHandler handler,
            int layer,
            object obj
        )
        {
            if (obj is INotifyPropertyChanged notify)
            {
                var (propName, _) = components.GetForIdx(layer);

                if (!subscribedProperties.TryGetValue(obj, out var dict))
                    return false;
                if (!dict.TryGetValue(propName, out var subProp))
                    return false;

                subProp.Set.TryRemove(handler, out _);
                if (subProp.Set.Count == 0)
                {
                    notify.PropertyChanged -= subProp.ExecutingHandler;
                }

                return true;
            }

            return false;
        }

        private CacheEntry ForTargetObject(object target)
        {
            var objType = target.GetType();
            if (lastEntry.RootType != objType)
                lastEntry = GetOrCreateEntry(objType);
            return lastEntry;
        }

        private void AddCacheItem(CacheEntry entry)
        {
            typeCache.AddFirst(entry);
            if (typeCache.Count > maxCacheSize)
                typeCache.RemoveLast();
        }

        private CacheEntry GetOrCreateEntry(Type type)
        {
            var node = typeCache.First;
            while (node != null)
            {
                if (node.Value.RootType == type)
                {
                    typeCache.Remove(node);
                    typeCache.AddFirst(node);
                    return node.Value;
                }
                node = node.Next;
            }

            var newEntry = CreateEntry(type);
            AddCacheItem(newEntry);
            return newEntry;
        }

        private CacheEntry CreateEntry(Type type)
        {
            ValueGetter? getter = null;
            ValueSetter? setter = null;
            Type currentType = type;

            var stages = new ValueGetter[components.Length];

            for (int i = 0; i < components.Length; i++) // this will always execute at least once
            {
                var (name, nullProp) = components.GetForIdx(i);
                var membGet = reflector.FindGetter(currentType, name);
                if (i == components.Length - 1)
                { // this is our last one
                    var membSet = reflector.FindSetter(currentType, name);
                    setter = getter == null ? membSet : ComposeSet(getter, membSet);
                }
                getter = getter == null 
                    ? membGet 
                    : nullProp
                        ? ComposeGetN(getter, membGet) 
                        : ComposeGetNR(getter, membGet);
                currentType = reflector.MemberType(currentType, name);

                stages[i] = membGet;
            }

            return new CacheEntry
            { // i know that both will be set here
                RootType = type,
                TargetType = currentType,
                Getter = getter!,
                Setter = setter!,
                Stages = stages,
                DefaultValue = Helpers.DefaultForType(currentType),
            };
        }

        private delegate object? GetPropagator(object? parent, ValueGetter getter);

        private static object? PropagateNR(object? parent, ValueGetter getter)
        {
            if (parent == null)
                throw new NullReferenceException();
            return getter(parent);
        }
        private static object? PropagateN(object? parent, ValueGetter getter)
        {
            if (parent == null)
                return null;
            return getter(parent);
        }

        private static ValueGetter ComposeGetNR(ValueGetter parent, ValueGetter member)
            => self => PropagateNR(parent(self), member);
        private static ValueGetter ComposeGetN(ValueGetter parent, ValueGetter member)
            => self => PropagateN(parent(self), member);
        private static ValueSetter ComposeSet(ValueGetter parent, ValueSetter member)
            => (self, value) =>
            {
                var pobj = parent(self);
                if (pobj == null)
                    throw new NullReferenceException();
                member(pobj, value);
            };
    }
}
