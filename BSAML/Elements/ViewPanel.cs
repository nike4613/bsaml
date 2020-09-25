using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BSAML.Elements
{
    public class ViewPanel : RootElement<ViewPanel>
    {
        private readonly List<LayoutInformation> chosenLayouts = new List<LayoutInformation>();

        public override async Task<LayoutInformation> Measure(LayoutInformation? layout)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            chosenLayouts.Clear();

            foreach (var child in this)
            {
                var requested = await child.Measure(null);
                LayoutInformation? last = null;

            retry:
                if (last != null && requested == last.Value)
                {
                    chosenLayouts.Add(requested);
                    continue;
                }

                last = requested;

                if (requested.Width > layout.Value.Width)
                    requested = new LayoutInformation(layout.Value.Width, requested.Height, requested.PreferChangesAlong);
                if (requested.Height > layout.Value.Height)
                    requested = new LayoutInformation(requested.Width, layout.Value.Height, requested.PreferChangesAlong);

                if (requested == last)
                {
                    chosenLayouts.Add(requested);
                    continue;
                }

                requested = await child.Measure(requested);
                goto retry; // goto so its easier to continue the foreach
            }

            return layout.Value;
        }

        private GameObject? obj;
        private RectTransform? transform;

        public override GameObject RenderToObject(LayoutInformation layout)
        {
            if (obj == null)
                obj = new GameObject(GetType().FullName);
            if (transform == null)
                transform = obj.AddComponent<RectTransform>();

            foreach (var child in transform.Cast<Transform>())
            {
                // unparent (it should be on the children to destroy them)
                child.SetParent(null, false);
            }

            foreach (var (child, clayout) in this.Zip(chosenLayouts, (e, l) => (e, l)))
            {
                var rendered = child.RenderToObject(clayout);

                var ctrans = rendered.GetComponent<Transform>();
                if (ctrans != null)
                    ctrans.SetParent(transform, false);
                var rtrans = rendered.GetComponent<RectTransform>();
                if (rtrans != null)
                    rtrans.anchoredPosition = Vector2.zero;
            }

            return obj;
        }

        public override async Task<GameObject> Render(LayoutInformation size)
            => RenderToObject(await Measure(size));
    }
}
