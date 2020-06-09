using System;

namespace IdeaShare.Extensions.System
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if(!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }

            return str;
        }

        public static string FillRouteParameters(this string str, params object[] parameters)
        {
            foreach(var param in parameters)
            {
                var openingBracketLocation = str.IndexOf("{");

                if(openingBracketLocation == -1)
                {
                    throw new Exception("Route does not contain this many parameters.");
                }

                var closingBracketLocation = str.IndexOf("}");

                if(closingBracketLocation == -1)
                {
                    throw new Exception($"Route is invalid -- does not have a matching closing bracket for {param} parameter.");
                }

                str = str.Substring(0, openingBracketLocation) + param + str.Substring(closingBracketLocation + 1);
            }

            return str;
        }
    }
}
