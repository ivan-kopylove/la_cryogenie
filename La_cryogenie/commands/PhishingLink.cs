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
        private DataTable LinksTable;

        public DateTime LastReportToHoster
        {
            get
            {
                return Utilities.convertFromUnixTimestamp(LinksTable.Rows[0].Field<long>("last_report_to_hoster"));
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

        public bool HasDbRecord
        {
            get
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
            private set
            {
                ;
            }
        }

        public void insertNewLink(string reporter_to_chat, string reporter_to_hoster, string host, string original_url, string game_projects, long last_seen, long last_report_to_hoster)
        {
            Sqlite.executeVoid(string.Format(
            @"INSERT INTO [links] ('category', 'reporter_to_chat', 'reporter_to_hoster', 'host', 'original_url', 'game_projects', 'last_seen', 'last_report_to_hoster') VALUES ('phishing_page', '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');",
            reporter_to_chat, reporter_to_hoster, host, original_url, game_projects, last_seen, last_report_to_hoster));
        }

        public void markUnSent(string host)
        {
            Sqlite.executeVoid(string.Format(
            @"UPDATE [links] SET last_seen = {0}, last_report_to_hoster = 0 WHERE host = '{1}'",
            Utilities.getCurrentUnixTime(), host));
        }

        public bool IsReported
        {
            get
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
            private set
            {
                ;
            }
        }
    }
}
