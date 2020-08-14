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

        public static DataObject GlobalDataContext { get; } = new DataObject();

        public class DataObject
        {
            public string DoTheThing => "Hello!";
            public string DataContextIsInherited => "The data context is inherited!";

            public struct FirstThing_
            {
                public string Thing => "This is FirstThing.Thing";
            }
            public FirstThing_ FirstThing => new FirstThing_();
        }

        const string Xaml = @"
<ExampleElement xmlns=""upf""
                xmlns:a=""clr-namespace:BasicApp;assembly=BasicApp""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                DataContext=""{x:Static a:Program.GlobalDataContext}"">
    <ExampleElement Text=""{Binding DoTheThing}"">
        <ExampleElement Text=""{Binding DataContextIsInherited}""/>
    </ExampleElement>
    <ExampleElement Text=""{Binding FirstThing.Thing}"" ExampleElement.ScrollTarget=""true""/>
    <ExampleElement Text=""{Binding Thing}"" DataContext=""{Binding FirstThing}""/>
</ExampleElement>
";
    }
}
