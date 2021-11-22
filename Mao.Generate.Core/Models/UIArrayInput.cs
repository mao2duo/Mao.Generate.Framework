using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public abstract class UIArrayInput<T> : UIInput
    {
        public UIArrayInputType Type { get; set; }
        public UIArrayInputOrientation Orientation { get; set; }

        public IEnumerable<T> Data { get; set; }
    }

    public enum UIArrayInputType
    {
        Table,
        Grid,
        Stack,
        Custom
    }

    public enum UIArrayInputOrientation
    {
        Horizontal,
        Vertical
    }
}
