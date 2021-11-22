﻿using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(CsMethodConverter))]
    public class CsMethod
    {
        public string Summary { get; set; }
        public string ReturnTypeName { get; set; }
        public string Name { get; set; }
        public CsAttribute[] Attributes { get; set; }
        public CsParameter[] Parameters { get; set; }
        public string FullCode { get; set; }
    }
}
