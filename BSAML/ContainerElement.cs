using Knit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSAML
{
    public abstract class ContainerElement : Element, ICollection<Element>
    {

        internal override void Attach(IServiceProvider services, bool refreshBindings = true)
        {
            base.Attach(services, false);

            if (refreshBindings)
                RequestBindingRefresh(true, false);

            foreach (var child in this)
                child.Attach(services, refreshBindings);
        }

        protected override void RequestBindingRefresh(bool includeOut)
            => RequestBindingRefresh(includeOut, true);

        protected void RequestBindingRefresh(bool includeOut, bool refreshChildren = true)
        {
            if (!Constructed) return;

            base.RequestBindingRefresh(includeOut);

            if (refreshChildren)
            {
                foreach (var child in this)
                    RequestBindingRefreshOn(child, includeOut);
            }
        }

        #region Implement ICollection<Element>

        private readonly List<Element> children = new List<Element>();

        public virtual int Count => children.Count;

        public virtual bool IsReadOnly => ((ICollection<Element>)children).IsReadOnly;

        public virtual void Add(Element item)
        {
            SetParent(item, this);
            children.Add(item);
        }

        public virtual void Clear()
        {
            foreach (var c in children)
                SetParent(c, null);
            children.Clear();
        }

        public virtual bool Contains(Element item) => children.Contains(item);

        public virtual void CopyTo(Element[] array, int arrayIndex) => children.CopyTo(array, arrayIndex);

        public virtual IEnumerator<Element> GetEnumerator() => children.GetEnumerator();

        public virtual bool Remove(Element item)
        {
            if (children.Remove(item))
            {
                SetParent(item, null);
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }

    public abstract class ContainerElement<T> : ContainerElement
        where T : ContainerElement<T>
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
