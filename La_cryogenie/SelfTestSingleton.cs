using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace La_cryogenie
{
    class SelfTestSingleton
    {
        #region singleton
        static SelfTestSingleton()
        {
            Instance = new SelfTestSingleton();
        }
        private SelfTestSingleton() { }

        public static SelfTestSingleton Instance { get; private set; }

        static DateTime botStartTime = DateTime.Now;
        static DispatcherTimer selfTest_Timer = new DispatcherTimer();

        public void Start()
        {
            if (!selfTest_Timer.IsEnabled)
            {
                selfTest_Timer.Interval = new TimeSpan(0, 5, 0);
                selfTest_Timer.Tick += SelfTestTimer_Tick;
                selfTest_Timer.Start();

                SkypeSingleton.Instance.sendChatMessage(Chats.botCommandChat, "[+] Включен таймер селфтеста");
            }
        }

        public void Stop()
        {
            if (selfTest_Timer.IsEnabled)
            {
                selfTest_Timer.Stop();
                selfTest_Timer.Tick -= SelfTestTimer_Tick;
                SkypeSingleton.Instance.sendChatMessage(Chats.botCommandChat, "[-] Выключен таймер селфтеста");                
            }

        }

        private static void SelfTestTimer_Tick(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan uptime = currentTime - botStartTime;
            string result = string.Format("[Selftest] Аптайм: {0} дней {1} часов {2} минут {3} секунд.", uptime.Days.ToString(), uptime.Hours.ToString(), uptime.Minutes.ToString(), uptime.Seconds.ToString());
            SkypeSingleton.Instance.sendChatMessage(Chats.SelfTest, result);
        }

        #endregion singleton
    }
}
