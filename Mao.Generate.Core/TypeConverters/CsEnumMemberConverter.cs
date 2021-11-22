using Mao.Generate.Core.Models;
using Microsoft.CodeAnalysis.CSharp;
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
    public class CsEnumMemberConverter : TypeConverter
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
        /// EnumMemberDeclarationSyntax To CsEnumMember
        /// </summary>
        protected CsEnumMember Convert(EnumMemberDeclarationSyntax memberSyntax)
        {
            CsEnumMember csEnumMember = new CsEnumMember();
            csEnumMember.Name = memberSyntax.Identifier.Text;

            // TODO: Namespace, BaseTypeName

            #region Summary
            var memberSummarySyntax = memberSyntax.GetLeadingTrivia()
                .FirstOrDefault(x =>
                    x.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia
                    || x.RawKind == (int)SyntaxKind.MultiLineDocumentationCommentTrivia);
            Invoker.Using(new Services.CsService(), csService =>
            {
                csEnumMember.Summary = csService.GetSummary(memberSummarySyntax);
            });
            #endregion
            #region Attribute
            var attributesSyntax = memberSyntax.DescendantNodes().OfType<AttributeSyntax>();
            if (attributesSyntax != null && attributesSyntax.Any())
            {
                List<CsAttribute> csAttributes = new List<CsAttribute>();
                foreach (var attributeSyntax in attributesSyntax)
                {
                    csAttributes.Add(ObjectResolver.TypeConvert<CsAttribute>(attributeSyntax));
                }
                csEnumMember.Attributes = csAttributes.ToArray();
            }
            #endregion
            #region Value
            if (memberSyntax.EqualsValue != null)
            {
                if (memberSyntax.EqualsValue.Value is LiteralExpressionSyntax memberValueSyntax)
                {
                    csEnumMember.Value = (int)memberValueSyntax.Token.Value;
                }
                else
                {
                    if (int.TryParse(memberSyntax.EqualsValue.Value.ToString(), out int value))
                    {
                        csEnumMember.Value = value;
                    }
                }
            }
            #endregion
            return csEnumMember;
        }
    }
}
