using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public class UICheckbox : UIInput
    {
        public object Value { get; set; }
        public bool Checked { get; set; }
    }
}
