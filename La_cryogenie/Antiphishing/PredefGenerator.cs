using System.Data;

namespace La_cryogenie
{
    class PredefGenerator
    {
        string hosterName;
        string hosterCountry;
        string hosterType;
        string host;

        DataTable predef;

        public PredefGenerator(string hosterName_, string hosterCountry_, string hosterType_, string host_)
        {
            this.hosterName = hosterName_;
            this.hosterCountry = hosterCountry_;
            this.hosterType = hosterType_;
            this.host = host_;

            predef = Sqlite.executeSearch(string.Format(
"SELECT predef FROM [predefs] WHERE language = '{0}' AND type = '{1}'", hosterCountry, hosterType));
        }

        public string getBody()
        {
            if (predef.Rows.Count != 0)
            {
                return string.Format(predef.Rows[0].Field<string>("predef"), "http://" + host);
            }
            else
            {
                return null;
            }
        }

        public string getSubject()
        {
            switch (hosterType)
            {
                case "page":
                    switch (hosterCountry)
                    {
                        case "en":
                            return string.Format("Phishing URL ( http://{0}/ ) ", host);
                        case "ru":
                            return string.Format("Мошеннический сайт ( http://{0}/ ) ", host);
                        default:
                            return string.Format("Malware ( http://{0}/ ) ", host);
                    }
                case "file":
                    switch (hosterCountry)
                    {
                        case "en":
                            return string.Format("Malware ( http://{0}/ ) ", host);
                        case "ru":
                            return string.Format("Мошеннический файл ( http://{0}/ ) ", host);
                        default:
                            return string.Format("Malware ( http://{0}/ ) ", host);
                    }
                default:
                    return string.Format("Мошеннический URL http://{0}");
            }
        }
    }
}
