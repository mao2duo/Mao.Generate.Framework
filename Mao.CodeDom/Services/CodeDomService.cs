using Mao.CodeDom.Models;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Services
{
    public class CodeDomService
    {
        public virtual UnitCompiled Compile(UnitSource source)
        {
            UnitCompiled compilerResult = new UnitCompiled();
            compilerResult.SourceCode = source.SourceCode;

            // 預設載入這個專案的組件
            List<string> assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic)
                .Select(x => x.Location)
                .ToList();

            // 加入指定的組件
            if (source.Assemblies != null && source.Assemblies.Any())
            {
                foreach (var assembly in source.Assemblies.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                {
                    FileInfo file = new FileInfo(assembly);
                    // 不存在相同名稱的組件才需要加入
                    if (!assemblies.Any(x => x.EndsWith(file.Name)))
                    {
                        // 先找程式執行檔的目錄
                        string appAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assembly);
                        if (File.Exists(appAssemblyPath))
                        {
                            assemblies.Add(appAssemblyPath);
                            continue;
                        }
                        // 沒有的話交給系統去找
                        assemblies.Add(assembly);
                    }
                }
            }

            // 將組件名稱放到編譯參數內
            CompilerParameters compilerParameters = new CompilerParameters()
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
            };
            compilerParameters.ReferencedAssemblies.AddRange(assemblies.ToArray());
            // 把編譯時的組件記下來
            compilerResult.Assemblies = compilerParameters.ReferencedAssemblies.Cast<string>().ToArray();

            // 進行編譯
            CodeDomProvider codeProvider = new CSharpCodeProvider(new ProviderOptions(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roslyn\\csc.exe"),
                15));
            CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(compilerParameters, source.SourceCode);
            if (compilerResults.Errors.HasErrors)
            {
                // 將編譯錯誤記錄下來
                foreach (CompilerError error in compilerResults.Errors)
                {
                    compilerResult.CompilerErrors.Add(error);
                }
            }
            else
            {
                // 編譯出來的類別
                compilerResult.ResultType = compilerResults.CompiledAssembly.ExportedTypes.First();
            }
            return compilerResult;
        }

        /// <summary>
        /// 將編譯出來的類別建立成執行個體
        /// </summary>
        public object CreateInstance(UnitCompiled compiled, params object[] args)
        {
            return Activator.CreateInstance(compiled.ResultType, args);
        }
    }
}
