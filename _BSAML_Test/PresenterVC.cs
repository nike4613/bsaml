using BSAML.Elements;
using BSAML.ViewControllers;

namespace _BSAML_Test
{
    internal class PresenterVC : PanelViewController<ViewPanel>
    {
        public override string? XAML => @"
<ViewPanel xmlns=""bsaml""
           xmlns:k=""knit""
           xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <Text Value=""Hello, default text!"" />
    <Text Value=""Big font!"" FontSize=""20"" />
</ViewPanel>
";
    }
}
