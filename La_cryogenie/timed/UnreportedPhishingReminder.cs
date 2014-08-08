using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace La_cryogenie
{
    //whole class - spike
    static class UnreportedPhishingReminder
    {
        #region timer
        static DispatcherTimer timer = new DispatcherTimer();

        public static void timer_Start()
        {
            if (!timer.IsEnabled)
            {
                timer.Tick += timer_Tick;
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Start();
                SkypeSingleton.Instance.sendChatMessage(Chats.botCommandChat, "[+] Включен таймер UnreportedPhishingReminder");
            }
        }

        public static void timer_Stop()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                timer.Tick -= timer_Tick;
                SkypeSingleton.Instance.sendChatMessage(Chats.botCommandChat, "[-] Выключен таймер UnreportedPhishingReminder");
            }
        }

        private static void timer_Tick(object sender, EventArgs e)
        {
            if (

                (DateTime.Now.Hour == 10 && DateTime.Now.Minute == 40 && DateTime.Now.Second == 00) 
                ||
                (DateTime.Now.Hour == 18 && DateTime.Now.Minute == 00 && DateTime.Now.Second == 00)

               )
            {
                DataTable phishingUrls = Sqlite.executeSearch(string.Format("SELECT * FROM [links] WHERE last_report_to_hoster = 0 AND (category = 'phishing_page' OR category = 'malware');"));
                if (phishingUrls.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    string msg = string.Format("Неотправленных сайтов: {0}.", phishingUrls.Rows.Count.ToString());
                    SkypeSingleton.Instance.sendPrivateMessageBySkypeLogin("ivan.kopilov", msg);
                    SkypeSingleton.Instance.sendPrivateMessageBySkypeLogin("alexfaqigro", msg);
                    SkypeSingleton.Instance.sendPrivateMessageBySkypeLogin("danijel.martines", msg);
                    SkypeSingleton.Instance.sendPrivateMessageBySkypeLogin("s.lapygin", msg);
                    return;
                }
            }
        }
        #endregion timer
    }
}
