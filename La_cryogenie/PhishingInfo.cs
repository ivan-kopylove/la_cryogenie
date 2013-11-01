using SKYPE4COMLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    class PhishingInfo
    {
        Skype skype = new Skype();
        string chatname = null;
        string url = null;
        public PhishingInfo(string chat = null, string site = null)
        {
            this.chatname = chat;
            this.url = site;
        }

        private void skypeSendMessage(string chat, string message)
        {
            skype.Chat[chat].SendMessage(message);
        }

        public void postInfo()
        {
            //cleanUri - URL без http://, без get-параметров после слэша и прочего мусора
            //одновременно проверка на корректность
            string fullCleanUrl = NetTools.getHostFromInputUrl(url);
            if (fullCleanUrl == null)
            {
                skypeSendMessage(chatname, "[Antiphishing] Некорректный URL [0].");
                return;
            }

            //получаем домен второго уровня 
            string secondLevelDomain = NetTools.getsecondleveldomain(fullCleanUrl);
            if (secondLevelDomain == null)
            {
                skypeSendMessage(chatname, "[Antiphishing] Некорректный URL [1].");
                return;
            }

            //ищем в локальной базе
            DataTable result = Sqlite.executeSearch(string.Format("SELECT * FROM [free-hosters-info] WHERE hoster like (SELECT hoster FROM [free-hosters-domains] WHERE domain like '{0}');", secondLevelDomain));
            if (result.Rows.Count != 0)
            {
                string post = string.Format
                    (
                    "[Antiphishing] Хостинг: {0}; Страна: {1}\n" +
                    "[Antiphishing] Почтовый адрес для жалоб: {2}\n" +
                    "[Antiphishing] Форма для подачи жалоб: {3}\n" +
                    "[Antiphishing] Домашняя страница: {4}\n" +
                    "[Antiphishing] Описание: {5}\n",
                    result.Rows[0].Field<string>("hoster"), result.Rows[0].Field<string>("country"),
                    result.Rows[0].Field<string>("abuseemail"),
                    result.Rows[0].Field<string>("abusepage"),
                    result.Rows[0].Field<string>("homepage"),
                    result.Rows[0].Field<string>("description")
                    );
                skypeSendMessage(chatname, post);
                return;
            }

            //если нету в базе, берем с api:
            string xml = DomainTools.getXML(secondLevelDomain);
            if (xml == null)
            {
                skypeSendMessage(chatname, "[Antiphishing] Не удалось получить никакой информации.");
                return;
            }
            File.WriteAllText("xml/xml-" + fullCleanUrl + ".txt", xml);

            string nserverLines = DomainTools.getHoster(xml);
            if (nserverLines != null)
            {
                skypeSendMessage(chatname, "[Antiphishing] записи DNS-серверов, по ним иногда можно определить контактные данные адресата для жалобы:");
                skypeSendMessage(chatname, nserverLines);
            }

            string emailLines = DomainTools.getEmails(xml);
            if (emailLines != null)
            {
                skypeSendMessage(chatname, "[Antiphishing] Найденные почтовые адреса. Если в списке есть abuse@, support@, info@, то можно попытаться связаться по этим адресам:");
                skypeSendMessage(chatname, emailLines);
            }
        }
    }

}
