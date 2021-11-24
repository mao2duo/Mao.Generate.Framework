using Mao.Core.Models;
using Mao.Generate.Core.Models;
using Mao.Generate.Core.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class CsMethodConverter : BaseTypeConverter
    {
        /// <summary>
        /// MethodDeclarationSyntax To CsMethod
        /// </summary>
        protected CsMethod ConvertFrom(MethodDeclarationSyntax methodSyntax, TypeDescriptorContext<MethodDeclarationSyntax, CsEnumMember> context)
        {
            CsService csService = context.GetService(typeof(CsService)) as CsService;

            CsMethod csMethod = new CsMethod();
            csMethod.Name = methodSyntax.Identifier.Text;
            csMethod.FullCode = methodSyntax.ToFullString();

            #region Summary
            var methodSummarySyntax = methodSyntax.GetLeadingTrivia()
                .FirstOrDefault(x =>
                    x.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia
                    || x.RawKind == (int)SyntaxKind.MultiLineDocumentationCommentTrivia);
            csMethod.Summary = csService.GetSummary(methodSummarySyntax);
            #endregion
            #region Attribute
            csMethod.Attributes = methodSyntax.DescendantNodes()
                .OfType<AttributeSyntax>()?
                .Select(x => ObjectResolver.TypeConvert<AttributeSyntax, CsAttribute>(x))
                .ToArray();
            #endregion
            #region ReturnType
            csMethod.ReturnTypeName = methodSyntax.DescendantNodes()
                .OfType<TypeSyntax>()
                .First(x => x.Parent == methodSyntax)
                .ToString();
            #endregion
            #region Parameter
            csMethod.Parameters = methodSyntax.ParameterList?.Parameters
                .Select(x => ObjectResolver.TypeConvert<ParameterSyntax, CsParameter>(x))
                .ToArray();
            #endregion

            return csMethod;
        }
        /// <summary>
        /// MethodInfo To CsMethod
        /// </summary>
        protected CsMethod ConvertFrom(MethodInfo method)
        {
            var csMethod = new CsMethod();
            csMethod.Name = method.Name;
            // 執行階段無法取得 FullCode
            csMethod.FullCode = null;
            return csMethod;
        }
    }
}
