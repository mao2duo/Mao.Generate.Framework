using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(CsAttributeArgumentConverter))]
    public class CsAttributeArgument
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            string left = this.Name;
            string right;
            if (this.Value == null)
            {
                right = "null";
            }
            else if (this.Value is string @string)
            {
                right = $"\"{@string}\"";
            }
            else if (this.Value is Type type)
            {
                right = $"typeof({type.Name})";
            }
            else
            {
                throw new NotSupportedException();
            }
            if (string.IsNullOrEmpty(left))
            {
                return right;
            }
            return $"{left} = {right}";
        }
    }
}
