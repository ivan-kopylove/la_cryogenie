using SKYPE4COMLib;
using System;
using System.Data;
using System.Windows;

namespace La_cryogenie
{
    class Stat
    {
        ChatMessage msg;
        string[] commandArguments;
        public Stat(ChatMessage msg_, string[] commandArguments_)
        {
            this.msg = msg_;
            this.commandArguments = commandArguments_;
        }

        public void continueStatCommand()
        {

            if (1 >= commandArguments.Length)
            {
                SkypeSingleton.Instance.sendChatMessage(msg.ChatName, string.Format("Ошибка в синтаксисе"));
                return;
            }

            int days = new int();

            try
            {
                days = Convert.ToInt32(commandArguments[1]);
            }
            catch (Exception)
            {
                SkypeSingleton.Instance.sendChatMessage(msg.ChatName, string.Format("По всей видимости, «{0}» - не числовое значение", commandArguments[1]));
                return;
            }

            long now = Utilities.getCurrentUnixTime();
            long from = now - (86400 * days);

            if (2 == commandArguments.Length)
            {
                DataTable phishingUrlsAll = Sqlite.executeSearch(string.Format("SELECT * FROM [links] WHERE (category = 'phishing_page' OR category = 'malware') AND last_report_to_hoster > {0} ORDER BY last_report_to_hoster DESC;", from));
                if (phishingUrlsAll.Rows.Count == 0)
                {
                    SkypeSingleton.Instance.sendChatMessage(msg.ChatName, string.Format("Пусто"));
                    return;
                }
                else
                {
                    int count = 0;

                    SkypeSingleton.Instance.sendChatMessage(msg.ChatName, string.Format("Всего фишинговых ссылок и мошеннических файлов за последние {0} дней: {1}", days.ToString(), phishingUrlsAll.Rows.Count.ToString()));


                    string result = "Возможно, все ссылки не уместятся в одно сообщение.\nпроект || дата || оригинальная ссылка" + Environment.NewLine;
                    foreach (DataRow row in phishingUrlsAll.Rows)
                    {
                        count++;
                        result += string.Format
                            (
                            "|| {0} || {1} || {2} || http://{3} \n",
                            count, row.Field<string>("game_projects"), Utilities.convertFromUnixTimestamp(row.Field<long>("last_report_to_hoster")).ToShortDateString(), row.Field<string>("original_url")
                            );
                    }
                    SkypeSingleton.Instance.sendChatMessage(msg.ChatName, result);
                    return;
                }
            }

            if (3 == commandArguments.Length)
            {
                DataTable phishingUrlsByReporterToHoster = Sqlite.executeSearch(string.Format("SELECT * FROM [links] WHERE (category = 'phishing_page' OR category = 'malware') AND last_report_to_hoster > {0} AND reporter_to_hoster ='{1}' ORDER BY last_report_to_hoster DESC;", from, commandArguments[2]));
                if (phishingUrlsByReporterToHoster.Rows.Count == 0)
                {
                    SkypeSingleton.Instance.sendChatMessage(msg.ChatName, string.Format("Пусто"));
                    return;
                }
                else
                {
                    int count = 0;

                    SkypeSingleton.Instance.sendChatMessage(msg.ChatName, string.Format("Фишинговых ссылок и мошеннических файлов, отправленных {0} за последние {1} дней: {2}", commandArguments[2], days.ToString(), phishingUrlsByReporterToHoster.Rows.Count.ToString()));

                    string result = "Возможно, все ссылки не уместятся в одно сообщение.\nпроект || дата || оригинальная ссылка" + Environment.NewLine;
                    foreach (DataRow row in phishingUrlsByReporterToHoster.Rows)
                    {
                        count++;
                        result += string.Format
                            (
                            "|| {0} || {1} || {2} || http://{3} \n",
                            count, row.Field<string>("game_projects"), Utilities.convertFromUnixTimestamp(row.Field<long>("last_report_to_hoster")).ToShortDateString(), row.Field<string>("original_url")
                            );
                    }
                    SkypeSingleton.Instance.sendChatMessage(msg.ChatName, result);
                    return;
                }
            }
        }
    }
}
