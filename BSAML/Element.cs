using Knit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using UnityEngine;
using ILogger = Serilog.ILogger;

namespace BSAML
{
    public abstract class Element : DependencyObject
    {
        protected override sealed DependencyObject? ParentObject => Parent;

        public Element? Parent { get; private set; }

        protected static void SetParent(Element target, Element? e) => target.SetParent(e);
        protected virtual void SetParent(Element? e) => Parent = e;

        protected IDispatcher Dispatcher => services?.GetRequiredService<IDispatcher>() ?? throw new InvalidOperationException();

        private IServiceProvider? services;
        internal ILogger? logger;

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

        /// <summary>
        /// Renders this <see cref="Element"/> to a <see cref="GameObject"/> that will be parented by this element's parent.
        /// It should be sized appropriately according to <paramref name="layout"/>.
        /// </summary>
        /// <param name="layout">The <see cref="LayoutInformation"/> to use when rendering.</param>
        /// <returns>A <see cref="GameObject"/> that contains the rendered element.</returns>
        public abstract GameObject RenderToObject(LayoutInformation layout);

        /// <summary>
        /// Measure's this element's <see cref="LayoutInformation"/> 
        /// </summary>
        /// <param name="layout"></param>
        /// <returns></returns>
        public abstract Task<LayoutInformation> Measure(LayoutInformation? layout);

        protected internal bool Constructed { get; private set; } = false;
        internal virtual void Attach(IServiceProvider services, bool refreshBindings = true)
        {
            this.services = services;
            logger = services.GetRequiredService<ILogger>().ForContext<Element>();
            Constructed = true;

            if (refreshBindings)
                RequestBindingRefresh(true);

            GotServices(services);
        }

        protected static void RequestBindingRefreshOn(Element element, bool includeOut)
            => element.RequestBindingRefresh(includeOut);

        protected virtual void GotServices(IServiceProvider serivces) { }

        protected virtual void BindingsRefreshed(bool refreshOutBindings) { }

        protected override void RequestBindingRefresh(bool includeOut)
        {
            if (!Constructed) return;

            base.RequestBindingRefresh(includeOut);
            
            BindingsRefreshed(includeOut);
        }
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
