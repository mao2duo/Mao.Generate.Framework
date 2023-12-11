using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public class SqlView
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
