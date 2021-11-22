using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public class UIContainer : UIInput
    {
        public UIContainerGenerateType GenerateType { get; set; }
        public List<UIInput> Children { get; } = new List<UIInput>();
    }

    public enum UIContainerGenerateType
    {
        None,
        Value,
        Object,
        Array
    }
}
