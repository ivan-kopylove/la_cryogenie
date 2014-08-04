using FishLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace La_cryogenie
{
    class Hoster
    {
        URLTools uri;

        public string Name { get; private set; }
        public string AbuseEmail { get; private set; }
        public string Country { get; private set; }
        public string AbusePage { get; private set; }
        public string HomePage { get; private set; }
        public string Type { get; private set; }

        private DataTable HostersTable { get; set; }

        public Hoster(URLTools _uri)
        {
            this.uri = _uri;

            HostersTable = Sqlite.executeSearch(string.Format(
            "SELECT * FROM [hosters-info] WHERE hoster = (SELECT hoster FROM [free-hosters-domains] WHERE domain = '{0}');",
            uri.secondLevelDomain));
        }

        public Hoster(string dnsRecord)
        {
            HostersTable = Sqlite.executeSearch(string.Format(
            "SELECT * FROM [hosters-info] WHERE hoster = (SELECT hoster FROM [hosters-dnses] WHERE dns = '{0}');",
            dnsRecord));
        }

        public bool hasRecord()
        {
            //spike            
            if (uri != null)
            {
                switch (uri.firstleveldomain)
                {
                    case "tk":
                    case "ml":
                    case "cf":
                    case "ga":
                        this.Name = "freenom.com";
                        this.AbuseEmail = "abuse@freenom.com";
                        this.Country = "en";
                        this.AbusePage = "-";
                        this.HomePage = "http://freenom.com";
                        this.Type = "page";
                        return true;
                    default:
                        break;
                }

            }

            if (HostersTable.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                this.Name = HostersTable.Rows[0].Field<string>("hoster");
                this.AbuseEmail = HostersTable.Rows[0].Field<string>("abuseemail");
                this.Country = HostersTable.Rows[0].Field<string>("country");
                this.AbusePage = HostersTable.Rows[0].Field<string>("abusepage");
                this.HomePage = HostersTable.Rows[0].Field<string>("homepage");
                this.Type = HostersTable.Rows[0].Field<string>("type");

                return true;
            }
        }

        public bool hasEmail()
        {
            if (AbuseEmail.Contains("@"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
