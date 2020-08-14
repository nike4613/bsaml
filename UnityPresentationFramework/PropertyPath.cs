using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPresentationFramework
{
    [DebuggerDisplay("{System.String.Join('.', components)}")]
    public class PropertyPath
    {
        private readonly string[] components;
        private readonly IBindingReflector reflector;

        public PropertyPath(IEnumerable<string> components, IBindingReflector reflector)
        {
            this.components = components.ToArray();
            if (this.components.Length < 1)
                throw new ArgumentException("PropertyPath must have at least one component", nameof(components));
            this.reflector = reflector;
        }

        private const int maxCacheSize = 4;
        private readonly Queue<CacheEntry> typeCache = new Queue<CacheEntry>(maxCacheSize + 1);
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

        // TODO: maybe just store a list of objects that we've bound a handler to so we can always unsub from all of them?
        private readonly ConditionalWeakTable<Action<object?>, ConditionalWeakTable<object, PropertyChangedEventHandler>> handlerDelegates
            = new ConditionalWeakTable<Action<object?>, ConditionalWeakTable<object, PropertyChangedEventHandler>>();

        public bool AddChangedHandler(object target, Action<object?> handler)
        {
            var entry = ForTargetObject(target);
            var handlerMap = handlerDelegates.GetOrCreateValue(handler);

            bool result = false;
            object? obj = target;
            for (int i = 0; i < components.Length; i++)
            {
                result = TryAddLayerChangedHandler(entry, handlerMap, i, obj, handler) || result;
                obj = Propagate(obj, entry.Stages[i]);
                if (obj == null)
                    break;
            }

            return result;
        }

        private bool TryAddLayerChangedHandler(
            in CacheEntry entry, 
            ConditionalWeakTable<object, PropertyChangedEventHandler> handlerMap, 
            int layer, 
            object obj, 
            Action<object?> handler
        )
        {
            if (obj is INotifyPropertyChanged notify)
            {
                var propName = components[layer];

                if (handlerMap.TryGetValue(obj, out _))
                {
                    // TODO: warn
                    return true;
                }

                var stages = entry.Stages;
                var propHandler = new PropertyChangedEventHandler((sender, args) =>
                {
                    if (args.PropertyName == propName)
                    {
                        var obj = sender;
                        for (int i = layer + 1; i < components.Length; i++)
                        {
                            obj = Propagate(obj, stages[i]);
                        }
                        handler(obj);
                    }
                });
                handlerMap.Add(obj, propHandler);
                notify.PropertyChanged += propHandler;

                return true;
            }

            return false;
        }

        public void RemoveChangedHandler(object target, Action<object?> handler)
        {
            var entry = ForTargetObject(target);
            var handlerMap = handlerDelegates.GetOrCreateValue(handler);

            object? obj = target;
            for (int i = 0; i < components.Length - 1; i++)
            {
                TryRemoveLayerChangedHandler(handlerMap, i, obj);
                obj = Propagate(obj, entry.Stages[i]);
                if (obj == null)
                    break;
            }
        }

        private bool TryRemoveLayerChangedHandler(
            ConditionalWeakTable<object, PropertyChangedEventHandler> handlerMap,
            int layer,
            object obj
        )
        {
            if (obj is INotifyPropertyChanged notify)
            {
                var propName = components[layer];

                if (!handlerMap.TryGetValue(obj, out var handler))
                    return false;

                handlerMap.Remove(obj);
                notify.PropertyChanged += handler;

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
            typeCache.Enqueue(entry);
            if (typeCache.Count > maxCacheSize)
                typeCache.Dequeue();
        }

        private CacheEntry GetOrCreateEntry(Type type)
        {
            foreach (var entry in typeCache)
            {
                if (type == entry.RootType)
                {
                    // we move the gotten entry to the front of the queue
                    var allEntries = typeCache.Where(e => e.RootType != type).ToList();
                    typeCache.Clear();
                    foreach (var e in allEntries)
                        AddCacheItem(e);
                    AddCacheItem(entry);

                    // then return
                    return entry;
                }
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
