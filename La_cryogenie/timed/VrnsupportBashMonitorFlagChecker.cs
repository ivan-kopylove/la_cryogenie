using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace La_cryogenie
{
    static class VrnsupportBashMonitorFlagChecker
    {
        #region timer
        static DispatcherTimer timer = new DispatcherTimer();

        public static void timer_Start()
        {
            if (!timer.IsEnabled)
            {
                timer.Tick += timer_Tick;
                timer.Interval = new TimeSpan(0, 0, 5);
                timer.Start();
                SkypeSingleton.Instance.sendChatMessage(Chats.botCommandChat, "[+] Включен таймер VrnsupportBashMonitorFlagChecker");
            }
        }

        public static void timer_Stop()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                timer.Tick -= timer_Tick;
                SkypeSingleton.Instance.sendChatMessage(Chats.botCommandChat, "[-] Выключен таймер VrnsupportBashMonitorFlagChecker");
            }
        }

        private static void timer_Tick(object sender, EventArgs e)
        {
            string src = File.ReadAllText(@"X:\db\VrnsupportBashMonitor\update_flag.txt");
            if (src.Contains("1"))
            {
                File.WriteAllText(@"X:\db\VrnsupportBashMonitor\update_flag.txt", "0");
                string[] logins = File.ReadAllLines(@"X:\db\VrnsupportBashMonitor\subscribers_list.txt");
                foreach (string login in logins)
                {
                    SkypeSingleton.Instance.sendPrivateMessageBySkypeLogin(login, string.Format("{0}, новая цитата на http://bit.ly/vrnsupportbash", SkypeSingleton.Instance.skype.get_User(login).FullName));

                }
            }
            
        }
        #endregion timer
    }
}
