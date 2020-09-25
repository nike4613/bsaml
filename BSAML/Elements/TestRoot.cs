using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BSAML.Elements
{
    public class TestRoot : RootElement<TestRoot>
    {
        public override Task<LayoutInformation> Measure(LayoutInformation? layout)
        {
            return Task.FromResult(layout.GetValueOrDefault());
        }

        public override GameObject RenderToObject(LayoutInformation layout)
        {
            throw new NotImplementedException();
        }

        public override Task<GameObject> Render(LayoutInformation size)
        {
            throw new NotImplementedException();
        }
    }
}
