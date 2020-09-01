using Knit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BSAML.Elements
{
    public class ExampleElement : ContainerElement<ExampleElement>
    {
        public static readonly DependencyProperty<string> TextProperty = 
            Properties.Register(nameof(Text), "", (e, v) => e.TextChanged(v));

        public static readonly DependencyProperty<bool> ScrollTargetProperty =
            Properties.RegisterAttached("ScrollTarget", false);

        private void TextChanged(string v)
        {
            logger?.Information("{@Property} on {@Element} set to {Value}", TextProperty, this, v);
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }


        protected override GameObject RenderToObject(LayoutInformation layout)
        {
            throw new NotImplementedException();
        }

        protected override Task<LayoutInformation> Measure(LayoutInformation? layout)
        {
            return Task.FromResult(layout.GetValueOrDefault());
        }

    }
}
