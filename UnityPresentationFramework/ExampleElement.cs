using System;
using System.Collections.Generic;
using System.Text;

namespace UnityPresentationFramework
{
    public class ExampleElement : Element<ExampleElement>
    {
        public static readonly DependencyProperty<Bindable<string>> TextProperty = 
            Properties.Register(nameof(Text), (Bindable<string>)"", (e, v) => e.TextChanged(v));

        public static readonly DependencyProperty<bool> ScrollTargetProperty =
            Properties.RegisterAttached("ScrollTarget", false);

        private void TextChanged(string v)
        {

        }

        public Bindable<string> Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

    }
}
