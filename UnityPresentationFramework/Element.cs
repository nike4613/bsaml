using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace UnityPresentationFramework
{
    public abstract class Element : DependencyObject
    {

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
