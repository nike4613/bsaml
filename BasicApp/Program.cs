using System;
using UnityPresentationFramework;

namespace BasicApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var obj = DynamicParser.ParseXaml(Xaml);
        }

        const string Xaml = @"
<ExampleElement xmlns=""upf""
                xmlns:a=""clr-namespace:BasicApp;assembly=BasicApp"">
    <ExampleElement />
</ExampleElement>
";
    }
}
