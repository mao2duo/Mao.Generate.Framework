using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public abstract class UIContainer : UIInput
    {
        public List<UIInput> Children { get; } = new List<UIInput>();
    }
}
