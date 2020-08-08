using System;
using System.Collections.Generic;
using System.Text;

namespace UnityPresentationFramework
{
    public abstract class DependencyObject
    {
        public object GetValue(DependencyProperty prop)
        {
            throw new NotImplementedException();
        }

        public void SetValue(DependencyProperty prop, object value)
        {
            throw new NotImplementedException();
        }
        public T GetValue<T>(DependencyProperty<T> prop)
        {
            throw new NotImplementedException();
        }

        public void SetValue<T>(DependencyProperty<T> prop, T value)
        {
            throw new NotImplementedException();
        }
    }
}
