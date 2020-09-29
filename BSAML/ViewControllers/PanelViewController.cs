using HMUI;
using Zenject;
using UnityEngine;
using BSAML.Elements;
using System.Collections;
using IPA.Utilities.Async;

namespace BSAML.ViewControllers
{
    public abstract class PanelViewController : ViewController
    {
        [Inject] protected internal DynamicParser parser = null!;

        public abstract string? XAML { get; }
        public ViewPanel Panel { get; set; } = null!;

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);
            Panel = (ViewPanel)parser.ParseXaml(XAML is null ? DefaultPanel : XAML); // An error page can also be rendered here if the parse fails.
            Panel.DataContext = this;
            var width = rectTransform.rect.width;
            var height = rectTransform.rect.height;
            StartCoroutine(RenderAndParent(width, height));
        }

        private IEnumerator RenderAndParent(float w, float h)
        {
            var renderTask = Panel.Render(new LayoutInformation(w, h));

            yield return Coroutines.WaitForTask(renderTask);

            var obj = renderTask.Result;

            var rt = obj.GetComponent<RectTransform>();
            rt.SetParent(rectTransform, false);
            rt.sizeDelta = Vector2.zero;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
        }

        const string DefaultPanel = @"
<ViewPanel xmlns=""bsaml""
           xmlns:k=""knit""
           xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <Text Value=""No Content"" />
</ViewPanel>
";
    }
}
