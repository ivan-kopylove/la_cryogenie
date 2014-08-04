using SKYPE4COMLib;
using System.Data;

namespace La_cryogenie
{
    class Ireport
    {
        ChatMessage msg;
        string[] commandArguments;
        public Ireport(ChatMessage msg_, string[] commandArguments_)
        {
            this.msg = msg_;
            this.commandArguments = commandArguments_;
        }

        private void markAllAsReported()
        {
            SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! Все мошеннические страницы отмечены как отправленные хостерам", msg.Sender.FullName));
            long nowTime = Utilities.getCurrentUnixTime();
            Sqlite.executeVoid(string.Format("UPDATE [links] SET last_report_to_hoster = '{0}', reporter_to_hoster = '{1}' WHERE last_report_to_hoster = 0 AND (category = 'phishing_page' OR category = 'malware');", nowTime, msg.Sender.Handle));
        }

        public void report()
        {
            if (commandArguments[1].ToLower() == "все" | commandArguments[1].ToLower() == "all")
            {
                markAllAsReported();
                return;
            }

            if (commandArguments.Length > 2)
            {
                SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Команда «{0}» теперь принимает только 1 аргумент: «все» или «all» («!{0} все»). Используй «!фишинг» или «!файл» для сдачи сайтов", commandArguments[0]));
            }
        }
    }
}
