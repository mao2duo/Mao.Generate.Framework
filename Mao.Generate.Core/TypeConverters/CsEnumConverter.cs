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
    public class CsEnumConverter : TypeConverter
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
        /// EnumDeclarationSyntax To CsEnum
        /// </summary>
        protected CsEnum ConvertFrom(EnumDeclarationSyntax enumSyntax)
        {
            CsEnum csEnum = new CsEnum();
            List<CsEnumMember> csEnumMembers = new List<CsEnumMember>();

            var enumSummarySyntax = enumSyntax.GetLeadingTrivia()
                .FirstOrDefault(x =>
                    x.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia
                    || x.RawKind == (int)SyntaxKind.MultiLineDocumentationCommentTrivia);
            Invoker.Using(new Services.CsService(), csService =>
            {
                csEnum.Summary = csService.GetSummary(enumSummarySyntax);
            });

            IEnumerable<EnumMemberDeclarationSyntax> membersSyntax = enumSyntax.DescendantNodes()
                .OfType<EnumMemberDeclarationSyntax>()
                .Where(x => x.Parent == enumSyntax);

            int value = 0;
            foreach (var memberSyntax in membersSyntax)
            {
                var csEnumMember = ObjectResolver.TypeConvert<CsEnumMember>(memberSyntax);
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
