using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKYPE4COMLib;
using System.Data;
using System.Windows;
using System.Text.RegularExpressions;

namespace La_cryogenie
{
    public class Notices
    {
        

        ChatMessage msg;
        string[] commandArguments = null;
        public Notices(ChatMessage message, string[] args)
        {
            this.msg = message;
            this.commandArguments = args;
        }
        public Notices(ChatMessage message)
        {
            this.msg = message;
        }
        public Notices()
        {

        }

        public void refreshListOfActiveNotices()
        {
            Globals.listOfActiveNotices = Sqlite.executeSearch("SELECT * FROM [notices-maintenance]");
        }

        public void postListOfNotices()
        {
            refreshListOfActiveNotices();
            if (Globals.listOfActiveNotices.Rows.Count == 0)
            {
                SkypeStatic.sendMessage(msg.ChatName, "Нет активных уведомлений.");
            }
            else
            {
                string result = "# | Проект | Время запуска | Осталось запусков | Дни недели | Текст\n";
                foreach (DataRow row in Globals.listOfActiveNotices.Rows)
                {
                    int rowid = (int)row.Field<long>("id");
                    string project = row.Field<string>("project");
                    string start = row.Field<string>("start");
                    string text = row.Field<string>("text");
                    string daysOfWeek = row.Field<string>("daysofweek");
                    int runtimes = (int)row.Field<long>("runtimes");
                    result += string.Format("{0} | {1} | {2} | {3} | {4} | {5}\n", rowid, project, start, runtimes, daysOfWeek, text);
                    result += "========================\n";
                }
                SkypeStatic.sendMessage(msg.ChatName, result);
            }
        }

        public void decrementRunTimes(int id)
        {
            Sqlite.executeVoid(string.Format("UPDATE [notices-maintenance] SET runtimes=runtimes-1 WHERE id = {0}", id));
            refreshListOfActiveNotices();
        }

        public void addNewNotice()
        {
            string project = commandArguments[1];
            string time = commandArguments[2];
            int runtimes = Convert.ToInt32(commandArguments[3]);
            string daysOfWeek = commandArguments[4];
            string text = null;
            for (int i = 5; i < commandArguments.Count(); i++)
            {
                text += commandArguments[i] + " ";
            }
            if (text == null)
            {
                SkypeStatic.sendMessage(msg.ChatName, "Некорректный формат команды");
            }
            Sqlite.executeVoid(string.Format("INSERT INTO [notices-maintenance] ('project', 'start', 'runtimes', 'daysofweek', 'text') VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", project, time, runtimes, daysOfWeek, text));
            refreshListOfActiveNotices();
        }

        public void deleteNotice()
        {
            int id = Convert.ToInt32(commandArguments[1]);
            Sqlite.executeVoid(string.Format("DELETE FROM [notices-maintenance] WHERE ID = {0}", id));
            refreshListOfActiveNotices();
        }



    }
}
