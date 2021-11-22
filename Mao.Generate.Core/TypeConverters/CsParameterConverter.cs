﻿using Mao.Generate.Core.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    public class CsParameterConverter : TypeConverter
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

        /// <summary>
        /// ParameterSyntax To CsParameter
        /// </summary>
        protected CsParameter ConvertFrom(ParameterSyntax parameterSyntax)
        {
            CsParameter csParameter = new CsParameter();
            csParameter.Name = parameterSyntax.Identifier.Text;

            #region Type
            TypeSyntax typeSyntax = parameterSyntax.DescendantNodes().OfType<TypeSyntax>().First(x => x.Parent == parameterSyntax);
            csParameter.TypeName = typeSyntax.ToString();
            #endregion
            #region DefaultValue
            if (parameterSyntax.Default != null)
            {
                if (parameterSyntax.Default.Value is LiteralExpressionSyntax parameterValueSyntax)
                {
                    csParameter.DefaultValue = parameterValueSyntax.Token.Value;
                }
                else
                {
                    csParameter.DefaultValue = parameterSyntax.Default.Value.ToString();
                }
            }
            #endregion

            return csParameter;
        }
    }
}
