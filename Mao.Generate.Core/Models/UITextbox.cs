using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public abstract class UITextbox : UIInput
    {
        public virtual string Text
        {
            get => Value as string;
            set => Value = value;
        }
    }
}
