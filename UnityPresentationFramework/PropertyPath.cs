using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            for (int i = 0; i < components.Length; i++) // this will always execute at least once
            {
                var membGet = reflector.FindGetter(type, components[i]);
                getter = getter == null ? membGet : ComposeGet(getter, membGet);
                if (i == components.Length - 1)
                { // this is our last one
                    var membSet = reflector.FindSetter(type, components[i]);
                    setter = getter == null ? membSet : ComposeSet(getter, membSet);
                }
            }

            return new CacheEntry
            { // i know that both will be set here
                RootType = type,
                Getter = getter!,
                Setter = setter!
            };
        }

        private static ValueGetter ComposeGet(ValueGetter parent, ValueGetter member)
            => self =>
            {
                var parentObj = parent(self);
                if (parentObj == null)
                    throw new NullReferenceException();
                return member(parentObj);
            };
        private static ValueSetter ComposeSet(ValueGetter parent, ValueSetter member)
            => (self, value) =>
            {
                var parentObj = parent(self);
                if (parentObj == null)
                    throw new NullReferenceException();
                member(parentObj, value);
            };
    }
}
