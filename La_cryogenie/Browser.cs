using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    class Browser
    {
        public async Task<string> navigate(string URL)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.UserAgent = "Mozilla/4.0+(compatible;+MSIE+5.01;+Windows+NT+5.0)";
                HttpWebResponse result1 = (HttpWebResponse)request.GetResponse();
                Stream ReceiveStream1 = result1.GetResponseStream();
                StreamReader sr1 = new StreamReader(ReceiveStream1, Encoding.UTF8);
                string html = sr1.ReadToEnd();
                result1.Close();
                return html;
            });
        }
    }
}
