using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    class DomainTools
    {
        public string xml { get; set; }
        public List<string> nServers = new List<string>();

        public DomainTools(string host)
        {
            xml = setXML(host);
            if (xml != null)
            {
                setNservers(xml);
            }

        }

        private String timestamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        private String sign(String api_username, String key, String timestamp, String uri)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] data = encoder.GetBytes(api_username + timestamp + uri);
            HMACSHA1 hmac = new HMACSHA1(encoder.GetBytes(key));
            CryptoStream cs = new CryptoStream(Stream.Null, hmac, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();
            String hex = BitConverter.ToString(hmac.Hash);
            return hex.Replace("-", "").ToLower();
        }

        private string stringBuilder(string domain)
        {
            String api_username = "devstealthwar";
            String key = "760d8-0a6d1-d598e-aa06d-3f74b";
            String uri = string.Format("/v1/{0}/whois", domain);
            String host = "freeapi.domaintools.com";

            String timestamp = this.timestamp();
            String signature = this.sign(api_username, key, timestamp, uri);

            string url = string.Format("http://{0}{1}?format=xml&api_username={2}&signature={3}&timestamp={4}", host, uri, api_username, signature, timestamp);
            return url;
        }

        private string setXML(string domain)
        {
            string fullURL = stringBuilder(domain);
            SkypeSingleton.Instance.sendChatMessage(Chats.AutoReports, fullURL);
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(fullURL);
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        return reader.ReadToEnd();
                    }
                }

            }
            catch (Exception)
            {
                return null;
            }
        }

        public void setNservers(string xml)
        {
            using (StringReader reader = new StringReader(xml))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("nserver"))// || line.Contains("name_servers") || line.Contains("Name Server"))
                    {
                        string[] splittedLine = line.Split(' ');
                        string ns = splittedLine[splittedLine.Length - 1];
                        nServers.Add(ns);
                    }
                }
            }
        }

        public string getEmails(string xml)
        {
            string retunLine = null;
            using (StringReader reader = new StringReader(xml))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("@"))
                    {
                        retunLine += line + Environment.NewLine;
                    }
                }
            }
            return retunLine;

        }

        public bool hasXml
        {
            get
            {
                if (xml == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            private set { ; }
        }
    }
}
