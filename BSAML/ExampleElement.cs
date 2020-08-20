using Knit;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSAML
{
    public class ExampleElement : Element<ExampleElement>
    {
        public static readonly DependencyProperty<string> TextProperty = 
            Properties.Register(nameof(Text), "", (e, v) => e.TextChanged(v));

        public static readonly DependencyProperty<bool> ScrollTargetProperty =
            Properties.RegisterAttached("ScrollTarget", false);

        private void TextChanged(string v)
        {
            foreach (var c in this)
            {
                if (c.GetValue(ScrollTargetProperty))
                {

                }
            }
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

    }
}
