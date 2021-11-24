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
    public class CsAttributeConverter : BaseTypeConverter
    {
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
            csAttribute.Arguments = attributeSyntax.ArgumentList?.Arguments
                .Select(x => ObjectResolver.TypeConvert<AttributeArgumentSyntax, CsAttributeArgument>(x))
                .ToArray();
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
            csAttribute.Arguments = attributeType.GetProperties()
                .Select(x => ObjectResolver.TypeConvert<Attribute, PropertyInfo, CsAttributeArgument>(attribute, x))
                .ToArray();
            return csAttribute;
        }
        /// <summary>
        /// CsAttribute To String
        /// </summary>
        protected string ConvertTo(CsAttribute csAttribute)
        {
            if (csAttribute.Arguments != null && csAttribute.Arguments.Any())
            {
                return $"[{csAttribute.Name}({string.Join(", ", csAttribute.Arguments.Select(x => ObjectResolver.TypeConvert<CsAttributeArgument, string>(x)))})]";
            }
            return $"[{csAttribute.Name}]";
        }
    }
}
