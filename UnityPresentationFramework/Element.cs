﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using GameObject = System.Object;

namespace UnityPresentationFramework
{
    public abstract class Element : DependencyObject, ICollection<Element>
    {
        private readonly List<Element> children = new List<Element>();

        public Element? Parent { get; private set; }

        protected virtual void ChildNeedsRedraw(Element child) 
        {
            if (Parent != null)
                RequestRedraw();

            // TODO: warn here
        }
        protected virtual void RequestRedraw()
        {
            if (Parent == null)
                throw new InvalidOperationException();

            Parent.ChildNeedsRedraw(this);
        }
        
        #region Implement ICollection<Element>
        public virtual int Count => children.Count;

        public virtual bool IsReadOnly => ((ICollection<Element>)children).IsReadOnly;

        public virtual void Add(Element item)
        {
            children.Add(item);
            item.Parent = this;
        }

        public virtual void Clear()
        {
            foreach (var c in children)
                c.Parent = null;
            children.Clear();
        }

        public virtual bool Contains(Element item) => children.Contains(item);

        public virtual void CopyTo(Element[] array, int arrayIndex) => children.CopyTo(array, arrayIndex);

        public virtual IEnumerator<Element> GetEnumerator() => children.GetEnumerator();

        public virtual bool Remove(Element item)
        {
            if (children.Remove(item))
            {
                item.Parent = null;
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }

    public abstract class Element<T> : Element
        where T : Element<T>
    { 
        protected static class Properties
        {
            public static DependencyProperty<TValueType> Register<TValueType>(
                    string name,
                    TValueType defaultValue,
                    PropertyChanged<T, TValueType>? onChange = null,
                    ValidateValue<T, TValueType>? validate = null
                )
                => DependencyProperty.Register(name, defaultValue, onChange, validate);
            public static DependencyProperty<TValueType> RegisterAttached<TValueType>(
                    string name,
                    TValueType defaultValue,
                    PropertyChanged<DependencyObject, TValueType>? onChange = null,
                    ValidateValue<DependencyObject, TValueType>? validate = null
                )
                => DependencyProperty.RegisterAttached<T, TValueType>(name, defaultValue, onChange, validate);
        }
    }
}
