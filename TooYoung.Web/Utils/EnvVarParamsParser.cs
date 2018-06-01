using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TooYoung.Web.Utils
{
    public static class EnvVarParamsParser
    {
        public static List<string> ParseEnvVarParams(this string str)
        {
            var regex = new Regex(@"\$\((.+?)\)");
            var matches = regex.Matches(str);
            var result = matches.Select(m => m.Groups[1].Value).ToList();
            return result;
        }
    }
}