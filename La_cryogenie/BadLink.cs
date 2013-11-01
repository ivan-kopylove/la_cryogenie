using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKYPE4COMLib;
using System.IO;
using System.Data;
using System.Windows;

namespace La_cryogenie
{
    class BadLink
    {
        Skype skypeBotBusters = new Skype();
        ChatMessage msg = null;
        string[] commandArguments = null;

        //commandArguments[1] - URL, //commandArguments[2] - projects
        public BadLink(ChatMessage message, string[] args)
        {
            this.msg = message;
            this.commandArguments = args;
            
        }

        public BadLink(ChatMessage message)
        {
            this.msg = message;
        }

        public void postUnreported()
        {
            DataTable phishingUrls = Sqlite.executeSearch(string.Format("SELECT * FROM [links] WHERE last_report_to_hoster = 0 AND category like 'phishing_page'"));
            if (phishingUrls.Rows.Count == 0)
            {
                SkypeStatic.sendMessage(msg.ChatName, string.Format("Нет неотправленных жалоб {0}", SkypeStatic.getRandomSmile()));
                return;
            }
            else
            {
                string result = "|| Проект || Сайт || Оригинальная ссылка\n";
                foreach (DataRow row in phishingUrls.Rows)
                {
                    result += String.Format
                        (
                        "|| {0,3} || http://{1}/ || {2}\n",
                        row.Field<string>("game_project"), row.Field<string>("host"), row.Field<string>("original_url")
                        );

                }
                SkypeStatic.sendMessage(msg.ChatName, result);
                return;
            }
        }

        private void addNewLink(string category, string host, string original_url, string game_project, int count_of_posts_to_chat, double last_post_to_chat, double last_report_to_hoster)
        {
            Sqlite.executeVoid(string.Format(
"INSERT INTO [links] ('category', 'host', 'original_url', 'game_project', 'count_of_posts_to_chat', 'last_post_to_chat', 'last_report_to_hoster') " +
"VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');",
category, host, original_url, game_project, count_of_posts_to_chat, last_post_to_chat, last_report_to_hoster));
        }

        public void processPhishinLink()
        {
            //cleanUri - URL без http://, без get-параметров после слэша и прочего мусора
            //одновременно проверка на корректность
            string original_url = commandArguments[1];
            string project = commandArguments[2];
            string host = NetTools.getHostFromInputUrl(original_url);

            //домен второго уровня (учавствует в дальнейшем поиске в базе)
            string secondLevelDomain = NetTools.getsecondleveldomain(host);

            //проверки (самые элементарные)
            if (host == null | secondLevelDomain == null)
            {
                SkypeStatic.sendMessage(msg.ChatName, string.Format("{0}, поправь ссылочку ({1}) и попробуй ещё раз {2}", msg.Sender.FullName, original_url, SkypeStatic.getRandomSmile()));
                return;
            }

            if (project.Length > 3)
            {
                SkypeStatic.sendMessage(msg.ChatName, string.Format("{0}, длина аббревиатуры проекта ({1}) не должна превышать трёх символов {2}", msg.Sender.FullName, project, SkypeStatic.getRandomSmile()));
                return;
            }

            double nowTime = Utilities.getCurrentUnixTime();

            ////проверка есть ли уже фишинг в базе
            DataTable phishing = Sqlite.executeSearch(
                string.Format("SELECT * FROM [links] WHERE host like '{0}';", host));
            //если сайта нет
            if (phishing.Rows.Count == 0)
            {
                //то ищем совпадения сначала в базе бесплатных хостеров по домену второго уровня

                DataTable free_hoster = Sqlite.executeSearch(string.Format(
                 "SELECT * FROM [free-hosters-info] WHERE hoster like (SELECT hoster FROM [free-hosters-domains] WHERE domain like '{0}');", secondLevelDomain));
                //если есть совпадение в базе бесплатных хостингов
                if (free_hoster.Rows.Count != 0)
                {
                    string hosterName = free_hoster.Rows[0].Field<string>("hoster");
                    string abuseEmail = free_hoster.Rows[0].Field<string>("abuseemail");
                    string hosterCountry = free_hoster.Rows[0].Field<string>("country");

                    //если есть почтовый адрес
                    if (abuseEmail.Contains('@'))
                    {
                        EmailToHoster email = new EmailToHoster(free_hoster, host);
                        email.processSending();
                        SkypeStatic.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! Я написала письмо хостеру {1} на адрес {2} с жалобой на этот ресурс {3} (тестовый режим, по факту письмо уходит на заглушку);", msg.Sender.FullName, hosterName, abuseEmail, SkypeStatic.getRandomSmile()));
                        addNewLink("phishing_page", host, original_url, project, 1, nowTime, nowTime);
                    }
                    else
                    {
                        SkypeStatic.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! {1} передан на обработку {2}", msg.Sender.FullName, host, SkypeStatic.getRandomSmile()));
                        addNewLink("phishing_page", host, original_url, project, 1, nowTime, 0);
                        return;
                    }
                    
                }
                else
                {
                    SkypeStatic.sendMessage(msg.ChatName, string.Format("Записей о {0} не найдено, придется репортить руками самостоятельно {1}", secondLevelDomain, SkypeStatic.getRandomSmile()));
                    addNewLink("phishing_page", host, original_url, project, 1, nowTime, 0);
                    return;
                }

                return;
            }
            //если сайт уже есть в списке
            else
            {
                if (phishing.Rows[0].Field<long>("last_report_to_hoster") == 0)
                {
                    SkypeStatic.sendMessage(msg.ChatName, string.Format("{0}, {1} уже есть в списке и он ещё не зарепорчен хостеру {2}", msg.Sender.FullName, host, SkypeStatic.getRandomSmile()));
                    return;
                }
                else
                {
                    DateTime last_report_to_hoster = Utilities.convertFromUnixTimestamp(phishing.Rows[0].Field<long>("last_report_to_hoster"));
                    SkypeStatic.sendMessage(msg.ChatName, string.Format("{0}, {1} уже есть в списке и его последний раз отправляли хостеру {2}. Если сайт активен больше суток, следует зарепортить ещё раз вручную {3}", msg.Sender.FullName, host, last_report_to_hoster.ToString(), SkypeStatic.getRandomSmile()));
                    return;
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            return;
            //если нету в базе, берем с api:
            string xml = DomainTools.getXML(secondLevelDomain);
            if (xml == null)
            {
                SkypeStatic.sendMessage(msg.ChatName, "Не удалось получить никакой информации.");
                return;
            }
            File.WriteAllText("xml/xml-" + host + ".txt", xml);

            string nserverLines = DomainTools.getHoster(xml);
            if (nserverLines != null)
            {
                SkypeStatic.sendMessage(msg.ChatName, string.Format(
                    "записи DNS-серверов, по ним можно определить контактные данные адресата для жалобы:\n{0}", nserverLines
                    ));
            }

            string emailLines = DomainTools.getEmails(xml);
            if (emailLines != null)
            {
                SkypeStatic.sendMessage(msg.ChatName, string.Format
                    (
                    "Найденные почтовые адреса. Если в списке есть abuse@, support@, info@, то можно попытаться написать туда:\n{0}", emailLines
                    ));
            }
        }

        public void markAsReported()//сделать эту функцию под таблицу links, а не phishing-urls
        {
            string originallyPostedUrl = commandArguments[1];
            string project = commandArguments[2];
            string fullCleanUrl = NetTools.getHostFromInputUrl(originallyPostedUrl);
            if (fullCleanUrl == null)
            {
                SkypeStatic.sendMessage(msg.ChatName, "Некорректный URL [0].");
                return;
            }

            #region history
            DataTable phishingUrls = Sqlite.executeSearch(string.Format("SELECT * FROM [phishing-urls] WHERE host like '{0}';", fullCleanUrl));
            if (phishingUrls.Rows.Count == 0)
            {
                Sqlite.executeVoid(string.Format
                (
                "INSERT INTO [phishing-urls] ('host','originalurl','project','isreported') " +
                "VALUES ('{0}', '{1}', '{2}', '{3}');",
                fullCleanUrl, originallyPostedUrl, project, 1
                ));
            }
            else
            {
                Sqlite.executeVoid(string.Format
                    ("UPDATE [phishing-urls] SET isreported=1 WHERE host like '{0}'",
                    fullCleanUrl
                    ));
            }
            #endregion
            SkypeStatic.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! {1} отмечен как отправленный хостеру.", msg.Sender.FullName, fullCleanUrl));
            return;
        }

        public void markAllAsReported()
        {
            SkypeStatic.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! Все мошеннические страницы отмечены как отправленные хостерам {1}", msg.Sender.FullName, SkypeStatic.getRandomSmile()));
            double nowTime = Utilities.getCurrentUnixTime();
            Sqlite.executeVoid(string.Format("UPDATE [links] SET last_report_to_hoster = '{0}' WHERE last_report_to_hoster = 0 AND category like 'phishing_page';", nowTime));
        }

    }

    public class Links
    {
        string category { get; set; }
        string host { get; set; }
        string original_url { get; set; }
        string game_project { get; set; }
        int count_of_posts_to_chat { get; set; }
        int last_post_to_chat { get; set; }
        int last_report_to_hoster { get; set; }
    }
}
