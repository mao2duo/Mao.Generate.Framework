using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    public static class StringExtension
    {
        /// <summary>
        /// 將文字以行區格，每行縮進相同數量的空白 (預設 4)
        /// </summary>
        public static string Indent(this string value, int spaceCount = 4)
        {
            var space = new string(' ', spaceCount);
            var lines = (value ?? "").Replace("\r\n", "\n").Split('\n');
            return string.Join("\n", lines.Select(x => $"{space}{x}"));
        }
        /// <summary>
        /// 將文字以行區格，每行縮退相同數量的空白 (預設 4)
        /// </summary>
        public static string Unindent(this string value, int spaceCount = 4)
        {
            var space = new string(' ', spaceCount);
            var lines = (value ?? "").Replace("\r\n", "\n").Split('\n');
            return string.Join("\n", lines.Select(x =>
                x.StartsWith(space) ?
                    x.Substring(spaceCount) :
                    x.TrimStart(' ')));
        }
        public static Guid ToGuid(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return new Guid(value);
            }
            return default(Guid);
        }
        public static Guid? ToNullableGuid(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return new Guid(value);
            }
            return null;
        }
        /// <summary>
        /// 將字串以斷行符號分割為字串陣列
        /// </summary>
        public static string[] Lines(this string value)
        {
            return value.Replace("\r\n", "\n").Split('\n');
        }
        /// <summary>
        /// 以指定的分格符號串連字串集合
        /// </summary>
        public static string Join(this IEnumerable<string> lines, string separator = "")
        {
            return string.Join(separator, lines);
        }

        /// <summary>
        /// 轉換成以小寫並以指定符號分隔的命名
        /// </summary>
        public static string ToLowerSymbolCase(this string name, string symbol = "-")
        {
            Regex camelWordRegex = new Regex(@"(?<lower>[a-z0-9])(?<upper>[A-Z])");
            Regex symbolRegex = new Regex(@"[^A-Za-z0-9$@]");
            if (camelWordRegex.IsMatch(name))
            {
                name = camelWordRegex.Replace(name, match => $"{match.Groups["lower"].Value}{symbol}{match.Groups["upper"].Value}");
            }
            if (symbolRegex.IsMatch(name))
            {
                name = symbolRegex.Replace(name, match => match.Index == 0 ? match.Value : symbol);
            }
            return name.ToLower();
        }
        /// <summary>
        /// 轉換成以大寫並以指定符號分隔的命名
        /// </summary>
        public static string ToUpperSymbolCase(this string name, string symbol = "_")
        {
            Regex camelWordRegex = new Regex(@"(?<lower>[a-z0-9])(?<upper>[A-Z])");
            Regex symbolRegex = new Regex(@"[^A-Za-z0-9$@]");
            if (camelWordRegex.IsMatch(name))
            {
                name = camelWordRegex.Replace(name, match => $"{match.Groups["lower"].Value}{symbol}{match.Groups["upper"].Value}");
            }
            if (symbolRegex.IsMatch(name))
            {
                name = symbolRegex.Replace(name, match => match.Index == 0 ? match.Value : symbol);
            }
            return name.ToUpper();
        }
        /// <summary>
        /// 轉換成以小寫開頭的駝峰命名
        /// </summary>
        public static string ToLowerCamelCase(this string name)
        {
            Regex symbolWordRegex = new Regex(@"[^A-Za-z0-9]+(?<word>[A-Za-z0-9])");
            Regex firstWordRegex = new Regex(@"^[^A-Za-z]*(?<word>[A-Za-z0-9])");
            if (symbolWordRegex.IsMatch(name))
            {
                name = symbolWordRegex.Replace(name.ToLower(), match => match.Groups["word"].Value.ToUpper());
            }
            if (firstWordRegex.IsMatch(name))
            {
                name = firstWordRegex.Replace(name, match => match.Groups["word"].Value.ToLower());
            }
            return name;
        }
        /// <summary>
        /// 轉換成以大寫開頭的駝峰命名
        /// </summary>
        public static string ToUpperCamelCase(this string name)
        {
            Regex symbolWordRegex = new Regex(@"[^A-Za-z0-9]+(?<word>[A-Za-z0-9])");
            Regex firstWordRegex = new Regex(@"^[^A-Za-z]*(?<word>[A-Za-z0-9])");
            if (symbolWordRegex.IsMatch(name))
            {
                name = symbolWordRegex.Replace(name.ToLower(), match => match.Groups["word"].Value.ToUpper());
            }
            if (firstWordRegex.IsMatch(name))
            {
                name = firstWordRegex.Replace(name, match => match.Groups["word"].Value.ToUpper());
            }
            return name;
        }

        /// <summary>
        /// 將指定區間轉換成大寫
        /// </summary>
        public static string ToUpper(this string text, int startIndex, int length)
        {
            string prefix = null, suffix = null;
            if (startIndex > 0)
            {
                prefix = text.Substring(0, startIndex);
            }
            if (startIndex + length < text.Length)
            {
                suffix = text.Substring(startIndex + length);
            }
            return $"{prefix}{text.Substring(startIndex, length).ToUpper()}{suffix}";
        }
        /// <summary>
        /// 將指定區間的轉換成小寫
        /// </summary>
        public static string ToLower(this string text, int startIndex, int length)
        {
            string prefix = null, suffix = null;
            if (startIndex > 0)
            {
                prefix = text.Substring(0, startIndex);
            }
            if (startIndex + length < text.Length)
            {
                suffix = text.Substring(startIndex + length);
            }
            return $"{prefix}{text.Substring(startIndex, length).ToLower()}{suffix}";
        }

        /// <summary>
        /// 判斷字串是否可以作為檔案名稱
        /// </summary>
        public static bool IsFileName(this string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                return source.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1;
            }
            return true;
        }
        /// <summary>
        /// 將字串轉為可作為檔案名稱的字串
        /// </summary>
        public static string ToFileName(this string source, char exchange = '_')
        {
            if (!string.IsNullOrEmpty(source))
            {
                foreach (var c in System.IO.Path.GetInvalidFileNameChars())
                {
                    source = source.Replace(c, exchange);
                }
            }
            return source;
        }
    }
}
