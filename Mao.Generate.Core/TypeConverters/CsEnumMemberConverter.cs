using Mao.Core.Models;
using Mao.Generate.Core.Models;
using Mao.Generate.Core.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class CsEnumMemberConverter : BaseTypeConverter
    {
        /// <summary>
        /// EnumMemberDeclarationSyntax To CsEnumMember
        /// </summary>
        protected CsEnumMember Convert(EnumMemberDeclarationSyntax memberSyntax, TypeDescriptorContext<EnumMemberDeclarationSyntax, CsEnumMember> context)
        {
            CsService csService = context.GetService(typeof(CsService)) as CsService;

            CsEnumMember csEnumMember = new CsEnumMember();
            csEnumMember.Name = memberSyntax.Identifier.Text;

            // TODO: Namespace, BaseTypeName

            #region Summary
            var memberSummarySyntax = memberSyntax.GetLeadingTrivia()
                .FirstOrDefault(x =>
                    x.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia
                    || x.RawKind == (int)SyntaxKind.MultiLineDocumentationCommentTrivia);
            csEnumMember.Summary = csService.GetSummary(memberSummarySyntax);
            #endregion
            #region Attribute
            // Attribute
            csEnumMember.Attributes = memberSyntax.DescendantNodes()
                .OfType<AttributeSyntax>()?
                .Select(x => ObjectResolver.TypeConvert<AttributeSyntax, CsAttribute>(x))
                .ToArray();
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
