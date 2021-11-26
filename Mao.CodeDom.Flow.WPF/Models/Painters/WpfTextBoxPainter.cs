using Mao.CodeDom.Flow.WPF.Models.Inputs;
using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.WPF.Models.Painters
{
    public class WpfTextBoxPainter : UIPainter<WpfCanvas, WpfTextBox>
    {
        public override WpfCanvas Canvas { get; }
        public WpfTextBoxPainter(WpfCanvas canvas)
        {
            Canvas = canvas;
        }

        public override void Add(WpfTextBox input)
        {
            throw new NotImplementedException();
        }

        public override void Remove(WpfTextBox input)
        {
            throw new NotImplementedException();
        }
    }
}
