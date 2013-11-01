using SKYPE4COMLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    class Badresource
    {
        Skype skype = new Skype();
        string[] commandArguments = null;
        ChatMessage msg = null;
        string dbTable = null;
        //commandArguments[1] - URL, //commandArguments[2] - projects
        public Badresource(ChatMessage message = null, string[] args = null, string table = null)
        {
            this.msg = message;
            this.commandArguments = args;
            this.dbTable = table;
        }

        public void addNewValue()
        {
            string originallyPostedUrl = commandArguments[1];
            string host = NetTools.getHostFromInputUrl(originallyPostedUrl).Trim();
            if (host == null)
            {
                SkypeStatic.sendMessage(msg.ChatName, "Ссылка некорректная.");
                return;
            }
            host = host.Replace("www.", "");

            string projects = commandArguments[2];
            projects = formatProjects(projects);

            DataTable cheatPortals = Sqlite.executeSearch(string.Format("SELECT * FROM [{0}] WHERE host like '{1}'", dbTable, host));
            double currentUnixTime = Utilities.getCurrentUnixTime();
            if (cheatPortals.Rows.Count == 0)
            {
                Sqlite.executeVoid(string.Format
                    (
                    "INSERT INTO [{0}] ('host', 'originalurl', 'projects', 'countofreports', 'firstmention', 'lastmention')" +
                    "VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}');",
                    dbTable, host, originallyPostedUrl, projects, "1", currentUnixTime, currentUnixTime
                    ));
                SkypeStatic.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! Похоже ты первый добавил {1} в список (hug)", msg.Sender.FullName, host));
            }
            else
            {
                Sqlite.executeVoid(string.Format
                    (
                    "UPDATE [{0}] SET countofreports=countofreports+1, lastmention='{1}' WHERE host like '{2}'",
                    dbTable, currentUnixTime, host
                    ));
                SkypeStatic.sendMessage(msg.ChatName, string.Format("Похоже {0} уже есть в списке, но все равно спасибо, {1} (inlove)", host, msg.Sender.FullName));
            }
        }

        private string formatProjects(string projects)
        {
            /*идея следующая: 
             * 1 заменяем все левые символы на запятые
             * 2 делим по запятой
             * 3 запиливаем обратно все элементы массива с разделенными элементами чья длина больше 1
             * (2 и более - минимальная длина аббревиатуры проекта)
            */
            projects = projects.Replace(" ", "").Replace("[", ",").Replace("]", ",");
            string[] splittedProject = projects.Split(',');

            projects = null;
            foreach (string item in splittedProject)
            {
                if (item.Length > 1)
                {
                    projects += item + ",";
                }
            }

            return projects.ToUpper();
        }


    }

}
