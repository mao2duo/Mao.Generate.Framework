using Mao.CodeDom.Flow.WPF.Models.Inputs;
using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.WPF.Models.Painters
{
    public class WpfRadioButtonListPainter<TValue> : UIPainter<WpfCanvas, WpfRadioButtonList<TValue>>
    {
        public override WpfCanvas Canvas { get; }
        public WpfRadioButtonListPainter(WpfCanvas canvas)
        {
            Canvas = canvas;
        }

        public override void Add(WpfRadioButtonList<TValue> input)
        {
            throw new NotImplementedException();
        }

        public override void Remove(WpfRadioButtonList<TValue> input)
        {
            throw new NotImplementedException();
        }
    }
}
