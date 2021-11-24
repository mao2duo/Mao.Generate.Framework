using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Models
{
    public class OperatingEnvironment : IDisposable
    {
        public List<UnitCompiled> Compileds { get; } = new List<UnitCompiled>();

        public OperatingEnvironment()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        private Assembly AssemblyResolve(object sender, ResolveEventArgs e)
        {
            string assemblyName = $"{new AssemblyName(e.Name)?.Name}.dll";
            var assemblyPath = Compileds
                .SelectMany(x => x.Assemblies)
                .Where(x => x.EndsWith(assemblyName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            return null;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
        }
    }
}
