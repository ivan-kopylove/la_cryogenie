using FishLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    class PhishingLink
    {
        URLTools uri;
        private DataTable LinksTable;

        public string LastReportToHosterString
        {
            get
            {
                return Utilities.convertFromUnixTimestamp(LinksTable.Rows[0].Field<long>("last_report_to_hoster")).ToString();
            }
            private set
            {
                ;
            }
        }
        public PhishingLink(string urlHost)
        {
            LinksTable = Sqlite.executeSearch(string.Format(
            @"SELECT * FROM [links] WHERE host = '{0}' and category = 'phishing_page';",
            urlHost));
        }

        public bool alreadyInDb()
        {
            if (LinksTable.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void insertNewLink(string reporter_to_chat, string reporter_to_hoster, string host, string original_url, string game_projects, long last_seen, long last_report_to_hoster)
        {
            Sqlite.executeVoid(string.Format(
            @"INSERT INTO [links] ('category', 'reporter_to_chat', 'reporter_to_hoster', 'host', 'original_url', 'game_projects', 'last_seen', 'last_report_to_hoster') VALUES ('phishing_page', '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');",
            reporter_to_chat, reporter_to_hoster, host, original_url, game_projects, last_seen, last_report_to_hoster));
        }

        public bool reported()
        {
            if (LinksTable.Rows[0].Field<long>("last_report_to_hoster") == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
