using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public class WinDirectory
    {
        public string Name { get; set; }
        public List<WinDirectory> Directories { get; set; }
        public List<WinTextFile> Files { get; set; }
    }
}
