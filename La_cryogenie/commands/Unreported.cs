using SKYPE4COMLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    class Unreported
    {
        ChatMessage msg;
        string[] commandArguments;
        public Unreported(ChatMessage msg_, string[] commandArguments_)
        {
            this.msg = msg_;
            this.commandArguments = commandArguments_;
        }

        public void postUnreported()
        {
            DataTable phishingUrls = Sqlite.executeSearch(string.Format("SELECT * FROM [links] WHERE last_report_to_hoster = 0 AND (category = 'phishing_page' OR category = 'malware');"));
            if (phishingUrls.Rows.Count == 0)
            {
                SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Нет неотправленных жалоб"));
                return;
            }
            else
            {
                string result = "проект || оригинальная ссылка" + Environment.NewLine;
                foreach (DataRow row in phishingUrls.Rows)
                {
                    result += string.Format
                        (
                        "|| {0} || http://{1} \n",
                        row.Field<string>("game_projects"), row.Field<string>("original_url")
                        );
                }
                SkypeSingleton.Instance.sendMessage(msg.ChatName, result);
                return;
            }
        }
    }
}
