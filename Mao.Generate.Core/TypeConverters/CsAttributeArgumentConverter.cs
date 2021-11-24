using Mao.Core.Models;
using Mao.Generate.Core.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class CsAttributeArgumentConverter : BaseTypeConverter
    {
        /// <summary>
        /// AttributeArgumentSyntax To CsAttributeArgument
        /// </summary>
        protected CsAttributeArgument ConvertFrom(AttributeArgumentSyntax argumentSyntax)
        {
            CsAttributeArgument csAttributeArgument = new CsAttributeArgument();
            if (argumentSyntax.NameEquals != null)
            {
                csAttributeArgument.Name = argumentSyntax.NameEquals.Name.Identifier.Text;
            }
            if (argumentSyntax.Expression is LiteralExpressionSyntax argumentLiteralExpressionSyntax)
            {
                csAttributeArgument.Value = argumentLiteralExpressionSyntax.Token.Value;
            }
            else
            {
                csAttributeArgument.Value = argumentSyntax.Expression.ToString();
            }
            return csAttributeArgument;
        }
        /// <summary>
        /// PropertyInfo To CsAttributeArgument
        /// </summary>
        protected CsAttributeArgument ConvertFrom(PropertyInfo property, TypeDescriptorContext<PropertyInfo, CsAttributeArgument> context)
        {
            var csAttributeArgument = new CsAttributeArgument();
            csAttributeArgument.Name = property.Name;
            csAttributeArgument.Value = property.GetValue(context.Instance);
            return csAttributeArgument;
        }
        /// <summary>
        /// CsAttributeArgument To String
        /// </summary>
        protected string ConvertTo(CsAttributeArgument csAttributeArgument)
        {
            string left = csAttributeArgument.Name;
            string right;
            if (csAttributeArgument.Value == null)
            {
                right = "null";
            }
            else if (csAttributeArgument.Value is string @string)
            {
                right = $"\"{@string}\"";
            }
            else if (csAttributeArgument.Value is Type type)
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
