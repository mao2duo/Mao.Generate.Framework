using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace System.Security.Claims
{
    public static class ClaimsIdentityExtension
    {
        /// <summary>
        /// 取得指定 Type 的第一個 Claim (如果沒有則回傳 null)
        /// </summary>
        public static Claim GetClaim(this IIdentity identity, string type)
        {
            if (identity.IsAuthenticated && identity is ClaimsIdentity claimsIdentity)
            {
                return claimsIdentity.FindFirst(type);
            }
            return null;
        }
        /// <summary>
        /// 取得指定 Type 的第一個 Claim 的值 (如果沒有則回傳 null)
        /// </summary>
        public static string GetClaimValue(this IIdentity identity, string type)
        {
            return identity.GetClaim(type)?.Value;
        }
        /// <summary>
        /// 取得所有 Claim
        /// </summary>
        public static IEnumerable<Claim> GetClaims(this IIdentity identity)
        {
            if (identity.IsAuthenticated && identity is ClaimsIdentity claimsIdentity)
            {
                return claimsIdentity.Claims;
            }
            return null;
        }
        /// <summary>
        /// 取得指定 Type 的所有 Claim
        /// </summary>
        public static IEnumerable<Claim> GetClaims(this IIdentity identity, string type)
        {
            if (identity.IsAuthenticated && identity is ClaimsIdentity claimsIdentity)
            {
                return claimsIdentity.FindAll(type);
            }
            return null;
        }
        /// <summary>
        /// 取得指定 Type 的所有 Claim 的值
        /// </summary>
        public static IEnumerable<string> GetClaimValues(this IIdentity identity, string type)
        {
            return identity.GetClaims(type)?.Select(x => x.Value).ToList();
        }
        /// <summary>
        /// 將所有 Claim 以 Type 作為索引轉換成 KeyValuePair<string, string>
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetClaimKeyValues(this IIdentity identity)
        {
            if (identity.IsAuthenticated && identity is ClaimsIdentity claimsIdentity)
            {
                if (claimsIdentity.Claims != null)
                {
                    foreach (var claim in claimsIdentity.Claims)
                    {
                        yield return new KeyValuePair<string, string>(claim.Type, claim.Value);
                    }
                }
            }
        }
    }
}
