using System;
using System.Linq;
using UnityPresentationFramework;

namespace BasicApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var obj = DynamicParser.ParseXaml(Xaml);

            var dataCtx = obj.First().First().DataContext;
            dataCtx = obj.Skip(1).First().DataContext;
        }

        const string Xaml = @"
<ExampleElement xmlns=""upf""
                xmlns:a=""clr-namespace:BasicApp;assembly=BasicApp""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <ExampleElement Text=""{Binding DoTheThing}"" DataContext=""17"">
        <ExampleElement Text=""{Binding DataContextIsInherited}""/>
    </ExampleElement>
    <ExampleElement Text=""{Binding FirstThing.Thing}"" ExampleElement.ScrollTarget=""true""/>
    <ExampleElement Text=""{Binding Thing}"" DataContext=""{Binding FirstThing}""/>
</ExampleElement>
";
    }
}
