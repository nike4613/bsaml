using Knit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BSAML.Elements
{
    public class Text : Element<Text>
    {
        public static readonly DependencyProperty<string> ValueProperty
            = Properties.Register(nameof(Value), "", (e, v) => e.ValueChanged(v));
        public static readonly DependencyProperty<int> FontSizeProperty
            = Properties.Register(nameof(FontSize), 4, (e, v) => e.FontSizeChanged(v));
        
        public string Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        private void ValueChanged(string text)
        {
            if (tmp != null)
                tmp.text = text;
            RequestRedraw(); // redraw needs to be requested because the measures may be changed
        }

        private void FontSizeChanged(int size)
        {
            if (tmp != null)
                tmp.fontSize = size;
            RequestRedraw();
        }

        private TextMeshProUGUI? tmp;
        private GameObject? rendered;

        private (TextMeshProUGUI tmp, GameObject obj) GetOrCreateObjects()
        {
        retry:
            if (tmp != null && rendered != null)
                return (tmp, rendered);

            if (tmp == null && rendered == null)
            {
                rendered = new GameObject(GetType().FullName);

                tmp = rendered.AddComponent<TextMeshProUGUI>();

                // TODO: set font
                tmp.fontSize = FontSize;
                tmp.color = Color.white; // TODO: make color a property
                tmp.alignment = TextAlignmentOptions.TopLeft; // TODO: make alignment a property

                var transform = tmp.rectTransform;

                transform.anchorMin = transform.anchorMax = new Vector2(0.5f, 0.5f);
                transform.pivot = Vector2.zero;

                tmp.text = Value;

                return (tmp, rendered);
            }

            logger?.Warning("Unexpected state when setting up objects: " +
                "(tmp == null) is {TmpIsNull} (rendered == null) is {RenderedIsNull}", tmp == null, rendered == null);

            if (rendered != null)
                GameObject.Destroy(rendered);

            rendered = null;
            tmp = null;
            goto retry;
        }

        protected override Task<LayoutInformation> Measure(LayoutInformation? layout)
        {
            var (tmp, _) = GetOrCreateObjects();

            tmp.fontSize = FontSize;
            tmp.text = Value;
            // TODO: expose other properties

            // TODO: try to make it a sane layoutelement????
            //   that means figuring out Unity's normal layout protocol so I can ask TMP about it
            var preferAlong = Axis.None;
            if (layout != null)
            {
                var lay = layout.Value;

                var sizeDeltaVec = Vector2.zero;
                if (lay.Width != null)
                    sizeDeltaVec += new Vector2(lay.Width.Value, 0f);
                if (lay.Height != null)
                    sizeDeltaVec += new Vector2(0f, lay.Height.Value);

                // TODO: is this the right way to set sizing?
                tmp.rectTransform.sizeDelta = sizeDeltaVec;
                preferAlong = lay.PreferChangesAlong;
            }

        tryLayout:
            tmp.SetAllDirty();
            tmp.ForceMeshUpdate();

            var w = tmp.renderedWidth;
            var h = tmp.renderedHeight;

            if (layout != null)
            {
                var lay = layout.Value;

                if (preferAlong == Axis.Horizontal && h != lay.Height)
                {
                    var delta = tmp.rectTransform.sizeDelta;
                    delta.x *= 1.1f; // some arbitrary expansion, so we can try to grow
                    tmp.rectTransform.sizeDelta = delta;
                    goto tryLayout; // retry our layout step
                }

                // if preferAlong is Vertical and the width is wrong, i'm pretty sure that means I fucked up
            }

            return Task.FromResult(new LayoutInformation(w, h));
        }

        protected override GameObject RenderToObject(LayoutInformation layout)
        {
            var (tmp, obj) = GetOrCreateObjects();

            var sizeDeltaVec = Vector2.zero;
            if (layout.Width != null)
                sizeDeltaVec += new Vector2(layout.Width.Value, 0f);
            if (layout.Height != null)
                sizeDeltaVec += new Vector2(0f, layout.Height.Value);

            // TODO: is this the right way to set sizing?
            tmp.rectTransform.sizeDelta = sizeDeltaVec;

            tmp.SetAllDirty();
            tmp.ForceMeshUpdate();

            return obj;
        }
    }
}
