using System;
using System.ComponentModel;
using System.Linq;
using BSAML;

namespace BasicApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var obj = BSAMLCore.Parser.ParseXaml(Xaml);

            var dataCtx = obj.First().First().DataContext;
            dataCtx = obj.Skip(1).First().DataContext;

            BSAMLCore.Close();
        }

        public static DataObject GlobalDataContext { get; } = new DataObject();

        public class DataObject : INotifyPropertyChanged
        {
            public string DoTheThing => "Hello!";
            public string DataContextIsInherited => "The data context is inherited!";

            public struct FirstThing_ : INotifyPropertyChanged
            {
                public string Thing => "This is FirstThing.Thing";

                public event PropertyChangedEventHandler? PropertyChanged;
            }
            public FirstThing_ FirstThing => new FirstThing_();

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        const string Xaml = @"
<ExampleElement xmlns=""bsaml""
                xmlns:k=""knit""
                xmlns:a=""clr-namespace:BasicApp;assembly=BasicApp""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                DataContext=""{x:Static a:Program.GlobalDataContext}"">
    <ExampleElement Text=""{k:Binding DoTheThing}"">
        <ExampleElement Text=""{k:Binding DataContextIsInherited}""/>
    </ExampleElement>
    <ExampleElement Text=""{k:Binding FirstThing.Thing}"" ExampleElement.ScrollTarget=""true""/>
    <ExampleElement DataContext=""{k:Binding FirstThing}"" Text=""{k:Binding Thing}"" />
</ExampleElement>
";
    }
}
