using Mao.Generate.Core.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class CsParameterConverter : BaseTypeConverter
    {
        /// <summary>
        /// ParameterSyntax To CsParameter
        /// </summary>
        protected CsParameter ConvertFrom(ParameterSyntax parameterSyntax)
        {
            CsParameter csParameter = new CsParameter();
            csParameter.Name = parameterSyntax.Identifier.Text;
            csParameter.TypeName = parameterSyntax.DescendantNodes()
                .OfType<TypeSyntax>()
                .First(x => x.Parent == parameterSyntax)
                .ToString();
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
