using Mao.Generate.Core.Models;
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
    public class CsAttributeConverter : TypeConverter
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
        /// AttributeSyntax To CsAttribute
        /// </summary>
        protected CsAttribute ConvertFrom(AttributeSyntax attributeSyntax)
        {
            IdentifierNameSyntax attributeNameSyntax = null;
            if (attributeSyntax.Name is IdentifierNameSyntax)
            {
                attributeNameSyntax = attributeSyntax.Name as IdentifierNameSyntax;
            }
            else if (attributeSyntax.Name is QualifiedNameSyntax qualifiedNameSyntax)
            {
                // 如果有包含 Namespace
                attributeNameSyntax = qualifiedNameSyntax.Right as IdentifierNameSyntax;
            }
            else
            {
                throw new NotImplementedException();
            }
            CsAttribute csAttribute = new CsAttribute();
            csAttribute.Name = attributeNameSyntax.Identifier.Text;
            #region Argument
            if (attributeSyntax.ArgumentList != null && attributeSyntax.ArgumentList.Arguments.Any())
            {
                List<CsAttributeArgument> csAttributeArguments = new List<CsAttributeArgument>();
                foreach (var argumentSyntax in attributeSyntax.ArgumentList.Arguments)
                {
                    csAttributeArguments.Add(ObjectResolver.TypeConvert<CsAttributeArgument>(argumentSyntax));
                }
                csAttribute.Arguments = csAttributeArguments.ToArray();
            }
            #endregion

            return csAttribute;
        }
        /// <summary>
        /// Attribute To CsAttribute
        /// </summary>
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
    }
}
