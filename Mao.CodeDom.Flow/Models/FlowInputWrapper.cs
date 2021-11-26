using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.Models
{
    public class FlowInputWrapper<TInput> where TInput : UIInput
    {
        public TInput Input { get; }
    }
}
