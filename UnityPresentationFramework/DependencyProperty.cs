using System;
using System.Collections.Generic;
using System.Text;

namespace UnityPresentationFramework
{
    public delegate bool ValidateValue<TOwner, T>(TOwner owner, T value) where TOwner : DependencyObject;
    public delegate void PropertyChanged<TOwner, T>(DependencyProperty<T> property, TOwner owner, T value) where TOwner : DependencyObject;

    public abstract class DependencyProperty
    {
        public static DependencyProperty<TValueType> Register<TOwningType, TValueType>(
                string name,
                TValueType defaultValue,
                PropertyChanged<TOwningType, TValueType>? onChange = null,
                ValidateValue<TOwningType, TValueType>? validate = null
            )
            where TOwningType : Element
        {
            throw new NotImplementedException();
        }
        public static DependencyProperty<TValueType> RegisterAttached<TOwningType, TValueType>(
                string name,
                TValueType defaultValue,
                PropertyChanged<DependencyObject, TValueType>? onChange = null,
                ValidateValue<DependencyObject, TValueType>? validate = null
            )
            where TOwningType : Element
        {
            throw new NotImplementedException();
        }
    }

    public class DependencyProperty<T> : DependencyProperty
    {

    }
}
