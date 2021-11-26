using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Mao.CodeDom.Flow.WPF.Models
{
    public class WpfCanvas : UICanvas
    {
        public Grid Container { get; }
        public WpfCanvas(Grid container)
        {
            Container = container;
        }

        protected override IUIPainter<UICanvas, TInput> GetPainter<TInput>()
        {


            throw new NotImplementedException();
        }
    }
}
