using BSAML;
using BSAML.Elements;
using HMUI;
using IPA.Utilities.Async;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace _BSAML_Test
{
    internal class PresenterVC : ViewController
    {
        public ViewPanel Panel { get; set; } = null!;

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);

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
    }
}
