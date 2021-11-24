using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Models
{
    public class CodeBlock : Code, IDisposable
    {
        public Code Parent { get; }
        public CodeBlock(Code parent) : base()
        {
            Parent = parent;
        }

        public virtual void Dispose()
        {
            Parent.Blocks.Pop();
            Parent.Write(this);
        }
    }
}
