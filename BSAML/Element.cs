using Knit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Markup;
using UnityEngine;
using ILogger = Serilog.ILogger;

[assembly: XmlnsDefinition("bsaml", nameof(BSAML))]
[assembly: XmlnsDefinition("knit", nameof(Knit))]

namespace BSAML
{
    public abstract class Element : DependencyObject, ICollection<Element>
    {
        private readonly List<Element> children = new List<Element>();

        protected override sealed DependencyObject? ParentObject => Parent;

        public virtual Element? Parent { get; set; }

        protected IDispatcher Dispatcher => services?.GetRequiredService<IDispatcher>() ?? throw new InvalidOperationException();

        private IServiceProvider? services;
        private ILogger? logger;

        protected virtual void ChildNeedsRedraw(Element child) 
        {
            if (Parent != null)
            {
                RequestRedraw();
            }
            else
            {
                logger?.Warning("During child redraw request on {$Element} for {$Child}: Could not queue redraw becase there is no parent!", this, child);
            }
        }
        protected virtual void RequestRedraw()
        {
            if (Parent == null)
                throw new InvalidOperationException();

            Parent.ChildNeedsRedraw(this);
        }

        protected virtual void RenderTo(GameObject parent) { }

        internal bool Constructed { get; private set; } = false;
        internal void Attach(IServiceProvider services)
        {
            this.services = services;
            logger = services.GetRequiredService<ILogger>().ForContext<Element>();
            Constructed = true;
            RequestBindingRefresh(true, false);
            foreach (var child in this)
                child.Attach(services);
            GotServices(services);
        }

        protected virtual void GotServices(IServiceProvider serivces) { }

        protected override sealed void RequestBindingRefresh(bool includeOut)
            => RequestBindingRefresh(includeOut, true);

        protected void RequestBindingRefresh(bool includeOut, bool refreshChildren = true)
        {
            if (!Constructed) return;

            base.RequestBindingRefresh(includeOut);

            if (refreshChildren)
            {
                foreach (var child in this)
                    child.RequestBindingRefresh(includeOut, refreshChildren);
            }
        }
        
        #region Implement ICollection<Element>
        public virtual int Count => children.Count;

        public virtual bool IsReadOnly => ((ICollection<Element>)children).IsReadOnly;

        public virtual void Add(Element item)
        {
            item.Parent = this;
            children.Add(item);
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
