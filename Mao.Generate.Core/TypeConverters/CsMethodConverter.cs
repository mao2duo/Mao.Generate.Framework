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
    public class CsMethodConverter : TypeConverter
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
        /// MethodDeclarationSyntax To CsMethod
        /// </summary>
        protected CsMethod ConvertFrom(MethodDeclarationSyntax methodSyntax)
        {
            CsMethod csMethod = new CsMethod();
            csMethod.Name = methodSyntax.Identifier.Text;
            csMethod.FullCode = methodSyntax.ToFullString();

            #region Summary
            var methodSummarySyntax = methodSyntax.GetLeadingTrivia()
                .FirstOrDefault(x =>
                    x.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia
                    || x.RawKind == (int)SyntaxKind.MultiLineDocumentationCommentTrivia);
            Invoker.Using(new Services.CsService(), csService =>
            {
                csMethod.Summary = csService.GetSummary(methodSummarySyntax);
            });
            #endregion
            #region Attribute
            var attributesSyntax = methodSyntax.DescendantNodes().OfType<AttributeSyntax>();
            if (attributesSyntax != null && attributesSyntax.Any())
            {
                List<CsAttribute> csAttributes = new List<CsAttribute>();
                foreach (var attributeSyntax in attributesSyntax)
                {
                    csAttributes.Add(ObjectResolver.TypeConvert<CsAttribute>(attributeSyntax));
                }
                csMethod.Attributes = csAttributes.ToArray();
            }
            #endregion
            #region ReturnType
            TypeSyntax typeSyntax = methodSyntax.DescendantNodes().OfType<TypeSyntax>().First(x => x.Parent == methodSyntax);
            csMethod.ReturnTypeName = typeSyntax.ToString();
            #endregion
            #region Parameter
            if (methodSyntax.ParameterList != null && methodSyntax.ParameterList.Parameters.Any())
            {
                List<CsParameter> csParameters = new List<CsParameter>();
                foreach (var parameterSyntax in methodSyntax.ParameterList.Parameters)
                {
                    csParameters.Add(ObjectResolver.TypeConvert<CsParameter>(parameterSyntax));
                }
                csMethod.Parameters = csParameters.ToArray();
            }
            #endregion

            return csMethod;
        }
    }
}
