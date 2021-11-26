using Mao.CodeDom.Flow.WPF.Models.Inputs;
using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.WPF.Models.Painters
{
    public class WpfCheckBoxListPainter<TValue> : UIPainter<WpfCanvas, WpfCheckBoxList<TValue>>
    {
        public override WpfCanvas Canvas { get; }
        public WpfCheckBoxListPainter(WpfCanvas canvas)
        {
            Canvas = canvas;
        }

        public override void Add(WpfCheckBoxList<TValue> input)
        {
            throw new NotImplementedException();
        }

        public override void Remove(WpfCheckBoxList<TValue> input)
        {
            throw new NotImplementedException();
        }
    }
}
