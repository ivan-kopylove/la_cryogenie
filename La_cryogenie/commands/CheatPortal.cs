using FishLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    class CheatPortal
    {
        private DataTable LinksTable;
        string urlHost;
        public CheatPortal(string _urlHost)
        {
            this.urlHost = _urlHost;
            LinksTable = Sqlite.executeSearch(string.Format(@"SELECT * FROM [links] WHERE host = '{0}' AND category = 'cheat_portal';", urlHost));
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
            @"INSERT INTO [links] ('category', 'reporter_to_chat', 'reporter_to_hoster', 'host', 'original_url', 'game_projects', 'last_seen', 'last_report_to_hoster') VALUES ('cheat_portal', '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');",
            reporter_to_chat, reporter_to_hoster, host, original_url, game_projects, last_seen, last_report_to_hoster));
        }

        public void updateLink(string gameProjects)
        {
            Sqlite.executeVoid(string.Format(
            @"UPDATE [links] SET last_seen = {0}, game_projects = game_projects || ',{1}' WHERE host = '{2}' AND category = 'cheat_portal';",
            Utilities.getCurrentUnixTime(), gameProjects, urlHost));
        }

    }
}
