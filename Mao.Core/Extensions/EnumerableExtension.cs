using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// 讓字串的判斷可以套用 StringComparison
        /// </summary>
        public static bool Contains(this IEnumerable<string> enumerable, string value, StringComparison comparisonType)
        {
            return enumerable.Any(x => x.Equals(value, comparisonType));
        }
    }
}
