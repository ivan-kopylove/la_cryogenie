using SKYPE4COMLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    public class Goodboys
    {
        ChatMessage msg;
        string[] commandArguments;
        public Goodboys(ChatMessage msg_, string[] commandArguments_)
        {
            this.msg = msg_;
            this.commandArguments = commandArguments_;
        }

        public void run()
        {
            int days = 7;
            long now = Utilities.getCurrentUnixTime();
            long from = now - (86400 * days);

            DataTable query = Sqlite.executeSearch(string.Format("SELECT count(*), reporter_to_chat FROM links WHERE last_seen > 1408911839 GROUP BY reporter_to_chat ORDER BY count(*) DESC LIMIT 10;", from));
            if (0 != query.Rows.Count)
            {
                string msgToGo = "Топ-10 сдавших ссылки за последние 7 дней:\n";
                foreach (DataRow row in query.Rows)
                {
                    ;
                    msgToGo += string.Format
                        (
                        "{0,3} ссылок : {1}\n",
                        row.Field<long>(0), SkypeSingleton.Instance.skype.get_User(row.Field<string>(1)).FullName
                        );
                }

                SkypeSingleton.Instance.sendChatMessage(msg.ChatName, msgToGo);
            }
        }
    }
}
