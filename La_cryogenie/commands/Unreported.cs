using FishLib;
using SKYPE4COMLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        private void checkRedzHostringAutoClosing(DataTable table)
        {
            //spike for redz hosting
            int countOfAutoClosed = new int();
            foreach (DataRow row in table.Rows)
            {
                URLTools url = new URLTools("http://" + row.Field<string>("host"));
                Browser br = new Browser();
                
                switch (url.secondLevelDomain)
                {
                    case "hdd1.ru":
                    case "je1.ru":
                    case "xclan.ru":
                        string html;
                        try
                        {
                            html = br.navigate(url.sourceUrl);
                            File.WriteAllText(url.host + ".txt", html);
                            if (html.Contains("Red Z Group.<br>"))
                            {
                                Sqlite.executeVoid(string.Format("UPDATE [links] SET last_report_to_hoster = '{0}', reporter_to_hoster = '{1}' WHERE host = '{2}' AND (category = 'phishing_page' OR category = 'malware');", Utilities.getCurrentUnixTime(), SkypeSingleton.Instance.skype.CurrentUserHandle, url.host));
                                countOfAutoClosed++;
                            }
                        }
                        catch (Exception)
                        {
                            SkypeSingleton.Instance.sendPrivateMessageBySkypeLogin("ivan.kopilov", "Ошибка в функции checkRedzHostringAutoClosing при загрузке html");
                        }
                        break;
                    default:
                        break;
                }
            }

            if (countOfAutoClosed > 0)
            {
                SkypeSingleton.Instance.sendChatMessage(msg.ChatName, string.Format("Сайтов было автоматически закрыто ботом хостера http://redz.ru/: {0}", countOfAutoClosed.ToString()));
            }

        }

        public void postUnreported()
        {
            DataTable phishingUrlsRedzHostring = Sqlite.executeSearch(string.Format("SELECT * FROM [links] WHERE last_report_to_hoster = 0 AND (category = 'phishing_page' OR category = 'malware');"));
            checkRedzHostringAutoClosing(phishingUrlsRedzHostring);

            DataTable phishingUrls = Sqlite.executeSearch(string.Format("SELECT * FROM [links] WHERE last_report_to_hoster = 0 AND (category = 'phishing_page' OR category = 'malware');"));
            if (phishingUrls.Rows.Count == 0)
            {
                SkypeSingleton.Instance.sendChatMessage(msg.ChatName, string.Format("Нет неотправленных жалоб"));
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
                SkypeSingleton.Instance.sendChatMessage(msg.ChatName, result);
                return;
            }
        }
    }
}
