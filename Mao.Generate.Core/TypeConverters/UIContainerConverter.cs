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
    public class UIContainerConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => false;
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
            if (value != null)
            {
                var convert = this.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(x => x.Name == nameof(ConvertFrom)
                        && x.GetParameters().Length == 1
                        && x.GetParameters()[0].ParameterType.IsAssignableFrom(value.GetType()));
                if (convert != null)
                {
                    convert.Invoke(this, new object[] { value });
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        protected UIContainer ConvertFrom(CsType csType)
        {
            UIContainer uiContainer = new UIContainer();
            uiContainer.GenerateType = UIContainerGenerateType.Object;
            //foreach (var property in properties)
            //{
            //    IEnumerable<UIInput> inputs;
            //    if (property.PropertyType.IsArray)
            //    {
            //        inputs = GenerateArrayInputs(property);
            //    }
            //    else if (!typeof(string).IsAssignableFrom(property.PropertyType) && property.PropertyType.IsClass)
            //    {
            //        inputs = GenerateObjectInputs(property);
            //    }
            //    else
            //    {
            //        inputs = GenerateValueInputs(property);
            //    }
            //    if (inputs != null && inputs.Any())
            //    {
            //        uiContainer.Children.AddRange(inputs);
            //    }
            //}
            return uiContainer;
        }
    }
}
