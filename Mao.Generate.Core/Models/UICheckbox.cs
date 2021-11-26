using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public abstract class UICheckbox : UIInput
    {
        public virtual bool Checked { get; set; }
    }
}
