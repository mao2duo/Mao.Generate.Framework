using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class CsAttributeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return this.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(x => x.Name == nameof(ConvertFrom)
                    && x.GetParameters().Length == 1
                    && x.GetParameters()[0].ParameterType.IsAssignableFrom(sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is Attribute attribute)
            {
                return ConvertFrom(attribute);
            }
            return base.ConvertFrom(context, culture, value);
        }

        protected CsAttribute ConvertFrom(Attribute attribute)
        {
            var csAttribute = new CsAttribute();
            var attributeType = attribute.GetType();
            csAttribute.Name = attributeType.Name;
            var attributeProperties = attributeType.GetProperties();
            if (attributeProperties != null && attributeProperties.Any())
            {
                var csAttributeArguments = new List<CsAttributeArgument>();
                foreach (var attributeProperty in attributeProperties)
                {
                    var csAttributeArgument = new CsAttributeArgument();
                    csAttributeArgument.Name = attributeProperty.Name;
                    csAttributeArgument.Value = attributeProperty.GetValue(attribute);
                    csAttributeArguments.Add(csAttributeArgument);
                }
                csAttribute.Arguments = csAttributeArguments.ToArray();
            }
            return csAttribute;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return this.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(x => x.Name == nameof(ConvertTo)
                    && destinationType.IsAssignableFrom(x.ReturnType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
