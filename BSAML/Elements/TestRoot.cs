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
        protected override Task<LayoutInformation> Measure(LayoutInformation? layout)
        {
            return Task.FromResult(layout.GetValueOrDefault());
        }

        protected override GameObject RenderToObject(LayoutInformation layout)
        {
            throw new NotImplementedException();
        }
    }
}
