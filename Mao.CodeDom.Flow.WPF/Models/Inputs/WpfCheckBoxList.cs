using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.WPF.Models.Inputs
{
    public class WpfCheckBoxList<TValue> : UISelect<TValue>
    {
        public override bool Multiple
        {
            get => true;
            set => throw new NotSupportedException();
        }
    }
}
