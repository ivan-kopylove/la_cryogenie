using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace La_cryogenie
{
    class NetTools
    {
        public static string getHostFromInputUrl(string inputUrl)
        {
            string httpUrlPattern = @"((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)";
            string withoutHttpUrlPattern = @"([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$";

            Regex httpUrl = new Regex(httpUrlPattern, RegexOptions.IgnoreCase);
            Regex withoutHttpUrl = new Regex(withoutHttpUrlPattern, RegexOptions.IgnoreCase);

            Match httpMatch = httpUrl.Match(inputUrl);
            Match withoutHttpMatch = withoutHttpUrl.Match(inputUrl);

            if (httpUrl.IsMatch(inputUrl))
            {
                Uri uri = new Uri(inputUrl);
                return uri.Host;
            }
            else if (withoutHttpUrl.IsMatch(inputUrl))
            {
                return withoutHttpMatch.Value;
            }
            else
            {
                return null;
            }
        }

        public static MatchCollection getHostsFromString(string line)
        {
            string hostPattern = @"([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,6}";
            Regex host = new Regex(hostPattern, RegexOptions.Singleline);
            MatchCollection matches = host.Matches(line);

            if (host.IsMatch(line))
            {
                return matches;
            }
            else
            {
                return null;
            }
        }



        public static string getsecondleveldomain(string domain)
        {
            string[] splittedDomain = domain.Split('.');
            if (splittedDomain.Count() < 2)
            {
                return null;
            }
            else
            {
                string secondLevelDomain = splittedDomain[splittedDomain.Count() - 2] + "." + splittedDomain[splittedDomain.Count() - 1];
                return secondLevelDomain;
            }
        }
    }
}
