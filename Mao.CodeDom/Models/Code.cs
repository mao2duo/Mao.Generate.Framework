using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Models
{
    public class Code
    {
        public Stack<CodeBlock> Blocks { get; } = new Stack<CodeBlock>();
        public CodeBlock CurrentBlock => Blocks.Count > 0 ? Blocks.Peek() : null;

        private readonly StringBuilder stringBuilder = new StringBuilder();
        public virtual void Write(object code)
        {
            if (CurrentBlock == null)
            {
                stringBuilder.Append(code);
            }
            else
            {
                CurrentBlock.Write(code);
            }
        }
        public virtual void Clear() => stringBuilder.Clear();

        public override string ToString() => stringBuilder.ToString();
    }
}
