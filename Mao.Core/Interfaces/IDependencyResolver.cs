using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Core.Interfaces
{
    public interface IDependencyResolver
    {
        /// <summary>
        /// 取得指定類型的服務
        /// </summary>
        object GetService(Type serviceType);
        /// <summary>
        /// 取得指定類型的所有服務
        /// </summary>
        IEnumerable<object> GetServices(Type serviceType);
    }
}
