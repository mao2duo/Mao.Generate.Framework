using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public abstract class UIInput
    {
        public virtual Guid Guid { get; } = Guid.NewGuid();
        public virtual string Name { get; set; }

        public virtual bool Visible { get; set; }
        public virtual bool Enabled { get; set; }

        public virtual object Tag { get; set; }
    }
}
