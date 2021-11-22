﻿using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(CsParameterConverter))]
    public class CsParameter
    {
        public string TypeName { get; set; }
        public string Name { get; set; }
        public object DefaultValue { get; set; }
    }
}
