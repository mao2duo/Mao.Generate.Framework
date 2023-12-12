using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public class SqlObjectDependency
    {
        public string ObjectDatabase { get; set; }
        public string ObjectSchemaName { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public string ObjectTypeDesc { get; set; }
        public string ReferencedDatabase { get; set; }
        public string ReferencedSchemaName { get; set; }
        public string ReferencedName { get; set; }
        public byte ReferencedClass { get; set; }
        public string ReferencedClassDesc { get; set; }
    }
}
