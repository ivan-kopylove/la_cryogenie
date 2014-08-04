using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace La_cryogenie
{
    class NoticesSingleton
    {
        static NoticesSingleton()
        {
            Instance = new NoticesSingleton();
        }
        private NoticesSingleton() { }

        public static NoticesSingleton Instance { get; private set; }

        private static DispatcherTimer noticesChecker = new DispatcherTimer();

        public void startChecking()
        {
            if (!noticesChecker.IsEnabled)
            {
                refreshListOfActiveNotices();
                noticesChecker.Interval = new TimeSpan(0, 0, 1);
                noticesChecker.Tick += noticesChecker_Tick;
                noticesChecker.Start();

                SkypeSingleton.Instance.sendMessage(Chats.botCommandChat, "[+] Включен таймер проверки нотайсов");
            }
        }
        public void StopChecking()
        {
            if (noticesChecker.IsEnabled)
            {
                noticesChecker.Stop();
                noticesChecker.Tick -= noticesChecker_Tick;
                SkypeSingleton.Instance.sendMessage(Chats.botCommandChat, "[-] Выключен таймер проверки нотайсов");
            }
            
        }

        #region SQL queries
        private static void refreshListOfActiveNotices()
        {
            activeNotices = Sqlite.executeSearch("SELECT * FROM [notices-maintenance];");
        }

        private static void decrementRunTimes(long id)
        {
            Sqlite.executeVoid(string.Format("UPDATE [notices-maintenance] SET runtimes=runtimes-1 WHERE id = {0};", id));
            refreshListOfActiveNotices();
        }

        private static void deleteNotice(int id)
        {
            Sqlite.executeVoid(string.Format("DELETE FROM [notices-maintenance] WHERE ID = {0}", id));
            refreshListOfActiveNotices();
        }

        private static void addNewNotice(string chatToPost, string project, string startTime, int runTimes, string daysOfWeek, string text)
        {
            Sqlite.executeVoid(string.Format("INSERT INTO [notices-maintenance] ('chat_to_post', 'start_time', 'runtimes', 'daysofweek', 'text') VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", chatToPost, startTime, runTimes, daysOfWeek, text));
            refreshListOfActiveNotices();
        }
        #endregion

        private static DataTable activeNotices { get; set; }

        private static void noticesChecker_Tick(object sender, EventArgs e)
        {
            if (activeNotices.Rows.Count == 0)
            {
                return;
            }
            else
            {
                foreach (DataRow notice in activeNotices.Rows)
                {
                    DateTime currentTime = DateTime.Now;
                    string startTime = notice.Field<string>("start_time");
                    string[] startArgs = startTime.Split(':');

                    int hour = Convert.ToInt32(startArgs[0]);
                    int minute = Convert.ToInt32(startArgs[1]);
                    int second = Convert.ToInt32(startArgs[2]);

                    if (currentTime.Minute == minute & currentTime.Hour == hour & currentTime.Second == second)
                    {
                        string daysOfWeek = notice.Field<string>("daysofweek");
                        string dayOfWeek;
                        switch (currentTime.DayOfWeek)
                        {
                            case DayOfWeek.Friday:
                                dayOfWeek = "5";
                                break;
                            case DayOfWeek.Monday:
                                dayOfWeek = "1";
                                break;
                            case DayOfWeek.Saturday:
                                dayOfWeek = "6";
                                break;
                            case DayOfWeek.Sunday:
                                dayOfWeek = "7";
                                break;
                            case DayOfWeek.Thursday:
                                dayOfWeek = "4";
                                break;
                            case DayOfWeek.Tuesday:
                                dayOfWeek = "2";
                                break;
                            case DayOfWeek.Wednesday:
                                dayOfWeek = "3";
                                break;
                            default:
                                dayOfWeek = "1";
                                break;
                        }

                        if (daysOfWeek.Contains(dayOfWeek))
                        {
                            long runtimes = notice.Field<long>("runtimes");

                            if (runtimes > 0)
                            {
                                long rowid = notice.Field<long>("id");
                                string chatToPost = notice.Field<string>("chat_to_post");
                                string text = notice.Field<string>("text");

                                SkypeSingleton.Instance.sendMessage(chatToPost, text);
                                decrementRunTimes(rowid);
                            }
                        }
                    }
                }
            }
        }

        /*
        public void postListOfNotices()
        {
            if (activeNotices.Rows.Count == 0)
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
                SkypeSingleton.Instance.sendMessage(msg.ChatName, result);
            }
        }
         */
    }
}
