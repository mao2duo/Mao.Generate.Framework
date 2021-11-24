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
    public class CsEnumConverter : BaseTypeConverter
    {
        /// <summary>
        /// EnumDeclarationSyntax To CsEnum
        /// </summary>
        protected CsEnum ConvertFrom(EnumDeclarationSyntax enumSyntax, TypeDescriptorContext<EnumDeclarationSyntax, CsEnum> context)
        {
            CsService csService = context.GetService(typeof(CsService)) as CsService;

            CsEnum csEnum = new CsEnum();

            var enumSummarySyntax = enumSyntax.GetLeadingTrivia()
                .FirstOrDefault(x =>
                    x.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia
                    || x.RawKind == (int)SyntaxKind.MultiLineDocumentationCommentTrivia);
            csEnum.Summary = csService.GetSummary(enumSummarySyntax);

            IEnumerable<EnumMemberDeclarationSyntax> memberSyntaxes = enumSyntax.DescendantNodes()
                .OfType<EnumMemberDeclarationSyntax>()
                .Where(x => x.Parent == enumSyntax);

            int value = 0;
            List<CsEnumMember> csEnumMembers = new List<CsEnumMember>();
            foreach (var memberSyntax in memberSyntaxes)
            {
                var csEnumMember = ObjectResolver.TypeConvert<EnumMemberDeclarationSyntax, CsEnumMember>(memberSyntax);
                if (csEnumMember.Value == 0)
                {
                    csEnumMember.Value = value;
                }
                else
                {
                    value = csEnumMember.Value;
                }
                csEnumMembers.Add(csEnumMember);
                value++;
            }

            csEnum.Name = enumSyntax.Identifier.Text;
            csEnum.Members = csEnumMembers.ToArray();
            return csEnum;
        }
    }
}
