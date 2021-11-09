using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    /// <summary>
    /// 泛型參數
    /// </summary>
    public class CsGenericArgument
    {
        public string Name { get; set; }
        /// <summary>
        /// 條件約束
        /// </summary>
        public string[] Constraints { get; set; }
    }
}
