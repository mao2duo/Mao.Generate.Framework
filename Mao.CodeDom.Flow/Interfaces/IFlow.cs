using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.Interfaces
{
    public interface IFlow
    {
        void Previous();
        void Next();

        bool CanPrevious { get; }
        bool CanNext { get; }

        bool IsCompleted { get; }
    }
}
