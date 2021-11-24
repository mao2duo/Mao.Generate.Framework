using Mao.Generate.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Services
{
    public class CsService
    {
        /// <summary>
        /// 從程式碼解析特定類型的節點
        /// </summary>
        public ICollection<T> GetSyntaxNodesFromCode<T>(string programText) where T : SyntaxNode
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetRoot() as CompilationUnitSyntax;
            return root.DescendantNodes().OfType<T>().ToList();
        }

        /// <summary>
        /// 從程式碼解析出 class 的結構
        /// </summary>
        public ICollection<CsType> GetCsTypesFromCode(string programText)
        {
            return GetSyntaxNodesFromCode<ClassDeclarationSyntax>(programText)
                .Select(x => ObjectResolver.TypeConvert<ClassDeclarationSyntax, CsType>(x))
                .ToList();
        }
        /// <summary>
        /// 從程式碼解析出 enum 的結構
        /// </summary>
        public ICollection<CsEnum> GetCsEnumsFromCode(string programText)
        {
            return GetSyntaxNodesFromCode<EnumDeclarationSyntax>(programText)
                .Select(x => ObjectResolver.TypeConvert<EnumDeclarationSyntax, CsEnum>(x))
                .ToList();
        }

        /// <summary>
        /// 解析 &lt;summary&gt;&lt;/summary&gt; 內的文字
        /// </summary>
        public string GetSummary(SyntaxTrivia trivia)
        {
            if (trivia != null)
            {
                string xml = trivia.ToString().Trim();
                return GetSummaryFromXml(xml);
            }
            return null;
        }

        public string GetSummaryFromXml(string xml)
        {
            throw new NotImplementedException();
        }
    }
}
