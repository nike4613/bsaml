using Knit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BSAML
{
    public abstract class RootElement : ContainerElement
    {
        protected override void SetParent(Element? e)
            => throw new InvalidOperationException("RootElement cannot be added as a child");

        public abstract Task<GameObject> Render(LayoutInformation size);

    }

    public abstract class RootElement<T> : RootElement
        where T : RootElement<T>
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
