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
    public abstract class BaseTypeConverter : TypeConverter
    {
        private IEnumerable<MethodInfo> FindConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            var converterType = this.GetType();
            return converterType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.DeclaringType == converterType)
                .Where(x => x.Name.StartsWith(nameof(ConvertFrom))
                    && Invoker.Using(x.GetParameters(), parameters =>
                        (parameters.Length == 1
                            && parameters[0].ParameterType.IsAssignableFrom(sourceType))
                        || (parameters.Length == 2
                            && parameters[0].ParameterType.IsAssignableFrom(sourceType)
                            && (context == null || parameters[1].ParameterType.IsAssignableFrom(context.GetType())))));
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => FindConvertFrom(context, sourceType).Any();
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value != null)
            {
                // 取得對應類型的轉換方法
                var convert = FindConvertFrom(context, value.GetType()).FirstOrDefault();
                if (convert != null)
                {
                    var convertParameters = convert.GetParameters();
                    var convertInvokeParameters = new object[convertParameters.Length];
                    // 來源物件
                    convertInvokeParameters[0] = value;
                    // ITypeDescriptorContext
                    if (convertInvokeParameters.Length == 2)
                    {
                        convertInvokeParameters[1] = context;
                    }
                    return convert.Invoke(this, convertInvokeParameters);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
        private IEnumerable<MethodInfo> FindConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            var converterType = this.GetType();
            return converterType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.DeclaringType == converterType)
                .Where(x => x.Name.StartsWith(nameof(ConvertTo))
                    && Invoker.Using(x.GetParameters(), parameters =>
                        (parameters.Length == 1
                            && destinationType.IsAssignableFrom(parameters[0].ParameterType))
                        || (parameters.Length == 2
                            && destinationType.IsAssignableFrom(parameters[0].ParameterType)
                            && (context == null || parameters[1].ParameterType.IsAssignableFrom(context.GetType())))));
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => FindConvertTo(context, destinationType).Any();
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null)
            {
                // 取得對應類型的轉換方法
                var convert = FindConvertTo(context, value.GetType()).FirstOrDefault();
                if (convert != null)
                {
                    var convertParameters = convert.GetParameters();
                    var convertInvokeParameters = new object[convertParameters.Length];
                    // 來源物件
                    convertInvokeParameters[0] = value;
                    // ITypeDescriptorContext
                    if (convertInvokeParameters.Length == 2)
                    {
                        convertInvokeParameters[1] = context;
                    }
                    return convert.Invoke(this, convertInvokeParameters);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
