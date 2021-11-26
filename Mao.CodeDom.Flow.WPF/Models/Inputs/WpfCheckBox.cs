using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.WPF.Models.Inputs
{
    public class WpfCheckBox : UICheckbox
    {
        public override bool Checked
        {
            get => base.Checked;
            set => base.Checked = value;
        }
    }
}
