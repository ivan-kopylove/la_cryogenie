using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace La_cryogenie
{
    public static class Utilities
    {
        public static DateTime convertFromUnixTimestamp(double unixtime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(unixtime);
        }

        public static double getCurrentUnixTime()
        {
            return (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private const string HTML_TAG_PATTERN = "<.*?>";
        public static string StripHTML(string inputString)
        {
            return Regex.Replace(inputString, HTML_TAG_PATTERN, string.Empty);
        }
        
    }
}
