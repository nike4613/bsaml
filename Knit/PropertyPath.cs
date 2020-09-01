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
    [DebuggerDisplay("{System.String.Join(\".\", components)}")]
    public class PropertyPath
    {
        private readonly string[] components;
        private readonly IBindingReflector reflector;
        private readonly ILogger logger;

        public PropertyPath(IEnumerable<string> components, IServiceProvider services)
        {
            this.components = components.ToArray();
            if (this.components.Length < 1)
                throw new ArgumentException("PropertyPath must have at least one component", nameof(components));
            reflector = services.GetRequiredService<IBindingReflector>();
            logger = services.GetRequiredService<ILogger>().ForContext<PropertyPath>();
        }

        public IEnumerable<string> Components => components;

        private const int maxCacheSize = 4;
        private readonly LinkedList<CacheEntry> typeCache = new LinkedList<CacheEntry>();
        private CacheEntry lastEntry;
    
        private struct CacheEntry
        {
            public Type RootType;
            public ValueGetter Getter;
            public ValueSetter Setter;
            public ValueGetter[] Stages;
        }
        
        public object? GetValue(object target)
        {
            var entry = ForTargetObject(target);
            return entry.Getter(target);
        }

        public void SetValue(object target, object? value)
        {
            var entry = ForTargetObject(target);
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
                obj = Propagate(obj, entry.Stages[i]);
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
                var propName = components[layer];
                var map = subscribedProperties.GetOrCreateValue(obj);
                var handlers = map.GetOrAdd(propName, _ => new SubscribedProperty());

                var stages = entry.Stages;
                if (handlers.Set.Count == 0)
                {
                    handlers.ExecutingHandler = (sender, args) =>
                    {
                        if (map.TryGetValue(args.PropertyName, out var prop))
                        {
                            var obj = sender;
                            for (int i = layer; i < components.Length; i++)
                            {
                                obj = Propagate(obj, stages[i]);
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
                obj = Propagate(obj, entry.Stages[i]);
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
                var propName = components[layer];

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
                var membGet = reflector.FindGetter(currentType, components[i]);
                if (i == components.Length - 1)
                { // this is our last one
                    var membSet = reflector.FindSetter(currentType, components[i]);
                    setter = getter == null ? membSet : ComposeSet(getter, membSet);
                }
                getter = getter == null ? membGet : ComposeGet(getter, membGet);
                currentType = reflector.MemberType(currentType, components[i]);

                stages[i] = membGet;
            }

            return new CacheEntry
            { // i know that both will be set here
                RootType = type,
                Getter = getter!,
                Setter = setter!,
                Stages = stages,
            };
        }

        private static object? Propagate(object? parent, ValueGetter getter)
        {
            if (parent == null)
                throw new NullReferenceException();
            return getter(parent);
        }
        private static void Propagate(object? parent, ValueSetter setter, object? value)
        {
            if (parent == null)
                throw new NullReferenceException();
            setter(parent, value);
        }
        private static ValueGetter ComposeGet(ValueGetter parent, ValueGetter member)
            => self => Propagate(parent(self), member);
        private static ValueSetter ComposeSet(ValueGetter parent, ValueSetter member)
            => (self, value) => Propagate(parent(self), member, value);
    }
}
