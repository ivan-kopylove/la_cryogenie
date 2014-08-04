using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace La_cryogenie
{
    class Utilities
    {

        public static long getCurrentUnixTime()
        {
            return (long) (DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static DateTime convertFromUnixTimestamp(double unixtime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(unixtime);
        }

        private const string HTML_TAG_PATTERN = "<.*?>";
        public static string StripHTML(string inputString)
        {
            return Regex.Replace(inputString, HTML_TAG_PATTERN, string.Empty);
        }
    }
}
