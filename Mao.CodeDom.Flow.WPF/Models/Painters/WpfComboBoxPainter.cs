using Mao.CodeDom.Flow.WPF.Models.Inputs;
using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.WPF.Models.Painters
{
    public class WpfComboBoxPainter<TValue> : UIPainter<WpfCanvas, WpfComboBox<TValue>>
    {
        public override WpfCanvas Canvas { get; }
        public WpfComboBoxPainter(WpfCanvas canvas)
        {
            Canvas = canvas;
        }

        public override void Add(WpfComboBox<TValue> input)
        {
            throw new NotImplementedException();
        }

        public override void Remove(WpfComboBox<TValue> input)
        {
            throw new NotImplementedException();
        }
    }
}
