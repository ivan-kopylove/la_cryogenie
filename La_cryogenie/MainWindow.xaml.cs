using SKYPE4COMLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NLog;
using System.Text.RegularExpressions;

namespace La_cryogenie
{
    public partial class MainWindow : Window
    {
        #region Список чатов для бота
        private const string noticesSettingsChat = "#ivan.kopilov/$82e879a1b2e963f0"; // [Настройка уведомлений бота]
        private const string testChat1 = "#ivan.kopilov/$ae7ccff35f82918a"; //[Skype Bot testing]
        private const string testChat2 = "#ivan.kopilov/$ce2faa6be0751de2"; //[Polygon] Тесточат-2
        private const string botBustersChat = "#ivan.kopilov/$ee20bc1f679ee266"; //[Bot busters] [Antiphishing] New and improved v2.0
        private const string firstLineInfoChat = "#sergei.masalov/$alexey.nelubov;19d49b9a2a817ccd"; // [1st-line][ИНФО][ВАЖНО] - оповещение первой линии.
        private const string failChat = "#katakan-at/$alexey.nelubov;ec5878d355b6fdea"; //[fail] Не забываем отписывать причины косяков сюда!
        List<string> activeChatsDbIds = new List<string>(); //список ID чатов, для которых включен бот, наполняется по событию выделения в listview
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            skypeMainInstance.MessageStatus += new _ISkypeEvents_MessageStatusEventHandler(skype_MessageReceived);//подписываемся на событие
            startSelfTestTimer();
        }

        #region antiflood vars and funcs
        int messagesPerMinuteCurrent = 0;
        int messagesPerMinuteLimit = 20;
        int previousUnixTime = (int)Utilities.getCurrentUnixTime();

        int difference = 0;
        const int resetTime = 120;
        DispatcherTimer antiFloodTimer;

        private void antiFloodElapsed(object source, ElapsedEventArgs e)
        {
            Globals.isBotPaused = false;
            antiFloodTimer.Stop();
        }
        #endregion

        #region notices vars and funcs
        DispatcherTimer noticesChecker = new DispatcherTimer();

        private void button_StartCheckingNotices_Click(object sender, RoutedEventArgs e)
        {
            initNoticesChecker();
        }

        void noticesChecker_Tick(object sender, EventArgs e)
        {
            noticesChecksCount++;
            label_NoticesCheckerCount.Content = noticesChecksCount.ToString();
            if (Globals.listOfActiveNotices.Rows.Count == 0)
            {
                return;
            }
            else
            {
                foreach (DataRow row in Globals.listOfActiveNotices.Rows)
                {
                    DateTime currentTime = DateTime.Now;

                    string daysOfWeek = row.Field<string>("daysofweek");
                    string dayOfWeek = Convert.ToInt32(currentTime.DayOfWeek).ToString();
                    if (daysOfWeek.Contains(dayOfWeek))
                    {
                        int runtimes = (int)row.Field<long>("runtimes");

                        if (runtimes > 0)
                        {
                            string startTime = row.Field<string>("start");
                            string[] startArgs = startTime.Split(':');
                            int hour = Convert.ToInt32(startArgs[0]);
                            int minute = Convert.ToInt32(startArgs[1]);
                            int second = Convert.ToInt32(startArgs[2]);

                            if (currentTime.Hour == hour & currentTime.Minute == minute & currentTime.Second == second)
                            {
                                int rowid = (int)row.Field<long>("id");
                                string project = row.Field<string>("project");
                                string text = row.Field<string>("text");
                                string result = string.Format("[{0}] {1}", project, text);
                                //debug var - firstLineInfoChat
                                SkypeStatic.sendMessage(firstLineInfoChat, result);
                                Notices notices = new Notices();
                                notices.decrementRunTimes(rowid);
                            }
                        }
                    }
                }
            }
        }

        private int noticesChecksCount = 0;
        private void initNoticesChecker()
        {
            noticesChecker.Interval = new TimeSpan(0, 0, 1);
            noticesChecker.Tick += new EventHandler(noticesChecker_Tick);
            noticesChecker.IsEnabled = true;
            noticesChecker.Start();

            Notices notices = new Notices();
            notices.refreshListOfActiveNotices();
        }

        #endregion

        #region interface operations

        private void button_acceptSelectedChats_Click(object sender, RoutedEventArgs e)
        {
            acceptSelectedChats();
        }

        private void button_StartBot_Click(object sender, RoutedEventArgs e)
        {
            //если не прицепился, то давай досвиданья
            if (!isAttachToSkypeSuccess())
            {
                label_IsAttachedToSkype.Foreground = Brushes.Red;
                label_IsAttachedToSkype.Content = "Skype client isn't running.";
                return;
            }
            //иначе - гого инициализироваться
            else
            {
                label_IsAttachedToSkype.Foreground = Brushes.Green;
                label_IsAttachedToSkype.Content = "Connected to skype client.";
                fillListViewChats();
            }
        }

        private void fillListViewChats()
        {
            ChatCollection listOfChats = skypeMainInstance.Chats;
            List<SkypeChats> skypeChats = new List<SkypeChats>();
            foreach (Chat chat in listOfChats)
            {
                if (chat.Members.Count > 2)//показываем только те чаты где больше 2 чел
                {
                    skypeChats.Add(new SkypeChats() { friendlyChatName = chat.Topic, dBChatname = chat.Name, CountOfMembers = chat.Members.Count });
                }
            }
            listView_Chats.ItemsSource = skypeChats;
        }

        #endregion

        #region inner funcs

        private const string chatCommandTrigger = "!";
        Skype skypeMainInstance = new Skype();
        private static bool isAttachToSkypeSuccess()
        {
            Skype skypeMainInstance = new Skype();
            if (!skypeMainInstance.Client.IsRunning)
            {
                return false;
            }
            else
            {
                skypeMainInstance.Attach(8, true);
                return true;
            }
        }

        /*
        * есть глюк, когда события обрабатываются дважды. 
        * Массив полученных сообщений. Дублирующиеся сообщения лесом
        */
        List<int> receivedMessagesIds = new List<int>();
        private void skype_MessageReceived(ChatMessage msg, TChatMessageStatus status)
        {
            
            //отбрасываем ненужные нам события
            if (status == TChatMessageStatus.cmsSending | status == TChatMessageStatus.cmsRead | status == TChatMessageStatus.cmsUnknown)
            {
                return;
            }
            /*
             * Если список активных чатов не содержит имени чата, переданного
             * событием, то прерываем функцию
             * личка при таком раскладе не будет пока работать
             * возможно запилить на личку отдельное событие
             */
            if (!activeChatsDbIds.Contains(msg.ChatName))
            {
                return;
            }
            //проверка на получение сообщения только из целевых чатов
            if (receivedMessagesIds.Contains(msg.Id))
            {
                return;
            }
            receivedMessagesIds.Add(msg.Id);
            

            if (status == TChatMessageStatus.cmsSent)
            {
                messagesPerMinuteCurrent++; //увеличить счётчик отправленных сообщений
            }

            int currentUnixTime = (int)Utilities.getCurrentUnixTime();
            difference = currentUnixTime - previousUnixTime;
            if (difference > resetTime)
            {
                previousUnixTime = currentUnixTime;
                messagesPerMinuteCurrent = 0;
            }
            else
            {
                if (messagesPerMinuteCurrent > messagesPerMinuteLimit)
                {
                    antiFloodTimer = new DispatcherTimer();
                    App.log.Trace("Лимит полученных команд, на всякий случай прекращаю работу.");
                    Environment.Exit(0);
                    //Globals.isBotPaused = true;
                    //messagesPerMinuteCurrent = 0;
                    //antiFloodTimer.Interval = new TimeSpan(0, 5, 0);
                    //antiFloodTimer.exp += antiFloodElapsed;
                    //antiFloodTimer.Start();
                    return;
                }
            }



            /*
            * TChatMessageStatus.cmsReceived - нужна эта проверка. Skype api принимает сообщение с этим статусом. 
            * Если не фильтровать статус, то сообщения будут дублироваться при прочтении на скайп-клиенте руками 
            * (будет генерироваться событие read при котором все полученные сообщения вновь будут приходить сюда)
            */
            if (status == TChatMessageStatus.cmsReceived)
            {
                // Продолжаем только если входящее сообщение - триггер (воскл знак)
                if (msg.Body.IndexOf(chatCommandTrigger) == 0) //что позиция в строке нужного сивола - 0, то есть в начале
                {
                    //удалить триггер и перевести в нижний регистр

                    File.AppendAllText("log/commands.txt", string.Format("[{0}] {1}: {2}\n", msg.Timestamp, msg.Sender.Handle, msg.Body));
                    //обработать команду
                    addTextToRichChatLog(msg);
                    processCommand(msg);
                }
            }
        }

        private void processCommand(ChatMessage msg)
        {
            
            string command = msg.Body.Remove(0, chatCommandTrigger.Length);

            //~~~~~~args by space~~~~~~~
            string[] commandArguments = command.Split(' ');
            commandArguments[0] = commandArguments[0].ToLower();
            //~~~~~~args by space~~~~~~~

            //~~~~~~args by new line~~~~~~~
            string[] commandArgumentsLineByLine = command.Split('\n');
            commandArgumentsLineByLine[0] = commandArgumentsLineByLine[0].ToLower();
            //~~~~~~args by new line~~~~~~~
            

            #region notices settings chat
            if (msg.ChatName == noticesSettingsChat)
            {
                switch (commandArguments[0])
                {
                    case "добавить":
                    case "add":
                        Notices newNotice = new Notices(msg, commandArguments);
                        newNotice.addNewNotice();
                        break;
                    case "список":
                    case "list":
                        Notices listOfNotices = new Notices(msg);
                        listOfNotices.postListOfNotices();
                        break;
                    case "удалить":
                    case "delete":
                        Notices deleteNotice = new Notices(msg, commandArguments);
                        deleteNotice.deleteNotice();
                        break;
                    case "применить":
                    case "apply":
                        Notices applyNotices = new Notices();
                        applyNotices.refreshListOfActiveNotices();
                        break;
                    default:
                        break;
                }
            }
            #endregion

            #region BotBusters
            //debug var - botBustersChat
            if (msg.ChatName == botBustersChat)
            {
                if (command.Contains("vk.com/"))
                {
                    SkypeStatic.sendMessage(msg.ChatName, "Неугодные сообщества vk.com следует передавать КМам в комменты на странице:\nhttps://confluence.mail.ru/pages/viewpage.action?pageId=30517456");
                    return;
                }

                switch (commandArguments[0])
                {
                    case "ф":
                    case "ph":
                    case "фишинг":
                    case "phishing":
                        if (commandArguments.Length < 3)
                        {
                            SkypeStatic.sendMessage(msg.ChatName, string.Format("{0}, ты ошибся в написании команды. Правильно так: \"!{1} ССЫЛКА ПРОЕКТ\" {2}", msg.Sender.FullName, commandArguments[0], SkypeStatic.getRandomSmile()));
                            break;
                        }
                        BadLink phishing = new BadLink(msg, commandArguments);
                        phishing.processPhishinLink();
                        break;
                    case "зарепорчу":
                    case "репорчу":
                    case "отрепорчу":
                    case "отпишу":
                    case "напишу":
                    case "iwillreport":
                        if (commandArguments[1].ToLower() == "все" | commandArguments[1].ToLower() == "all")
                        {
                            BadLink markAllAsReported = new BadLink(msg);
                            markAllAsReported.markAllAsReported();
                            break;
                        }
                        if (commandArguments.Length < 2)
                        {
                            SkypeStatic.sendMessage(msg.ChatName, string.Format("{0}, ты ошибся в написании команды. Правильно так: \"!{1} ССЫЛКА ПРОЕКТ\" или так: \"!{1} все (all)\" {2}", msg.Sender.FullName, commandArguments[0], SkypeStatic.getRandomSmile()));
                            break;
                        }
                        break;

                        ///////старая - перепилить
                        if (commandArguments.Length < 3)
                        {
                            SkypeStatic.sendMessage(msg.ChatName, string.Format("Команда не принята.\n" +
                            "Правильный формат команды:\n" +
                            "!{0} URL ПРОЕКТ\n" +
                            "Например: !{0} http://cheats.hut2.ru WF", commandArguments[0]));
                            break;
                        }
//тут создание экземпляра класса Badlink для запуска функции mark as reported (одиночной)
                        break;
                    case "unreported":
                    case "unrtd":
                    case "неотправленные":
                    case "неотпр":
                        BadLink unreported = new BadLink(msg);
                        unreported.postUnreported();
                        break;


                    ////////ВСЕ КОМАНДЫ СНИЗУ - СТАРЫЕ, ПЕРЕДЕЛАТЬ:
                    case "читы":
                    case "cheats":
                    case "ch":
                    case "читоры":
                        if (commandArguments.Length < 3)
                        {
                            SkypeStatic.sendMessage(msg.ChatName, string.Format("Команда не принята.\n" +
                            "Правильный формат команды:\n" +
                            "!{0} URL ПРОЕКТЫ\n" +
                            "Например: !{0} http://zhyk.ru/ PW,JD,WF", commandArguments[0]));
                            break;
                        }
                        Badresource cheat = new Badresource(msg, commandArguments, "cheat-portals");
                        cheat.addNewValue();
                        break;

                    case "gold":
                    case "голд":
                    case "goldseller":
                    case "продавец":
                    case "барыга":
                    case "барыги":
                    case "продавцы":
                        if (commandArguments.Length < 3)
                        {
                            SkypeStatic.sendMessage(msg.ChatName, string.Format("Команда не принята.\n" +
                            "Правильный формат команды:\n" +
                            "!{0} URL ПРОЕКТЫ\n" +
                            "Например: !{0} http://pwlvl.ru/ PW,JD,WF", commandArguments[0]));
                            break;
                        }
                        Badresource goldselleer = new Badresource(msg, commandArguments, "goldsell-portals");
                        goldselleer.addNewValue();

                        break;
                    case "info":
                    case "инфо":
                    case "и":
                    case "i":
                        if (commandArguments.Length < 2)
                        {
                            SkypeStatic.sendMessage(msg.ChatName, string.Format("Неправильный формат команды. Наберите !{0} URL", commandArguments[0]));
                            break;
                        }
                        PhishingInfo info = new PhishingInfo(msg.ChatName, commandArguments[1]);
                        info.postInfo();
                        break;
                    default:
                        //SkypeStatic.sendMessage(msg.ChatName, string.Format("Неизвестная команда >>{0}<<. Помощь по командам: https://confluence.mail.ru/x/PB6fAg", commandArguments[0]));
                        break;
                }
            }
            #endregion

            #region failchat
            if (msg.ChatName == testChat1)
            {
                if (commandArgumentsLineByLine[0].Contains("operator.mail.ru/support/staff/index.php?/Tickets/Ticket/View/".ToLower()))
                {
                    SkypeStatic.sendMessage(msg.ChatName, "DAS");
                    Fail fail = new Fail(msg, commandArgumentsLineByLine);
                    fail.proccessNewFail();
                }
            }
            #endregion
        }

        private void addTextToRichChatLog(ChatMessage msg)
        {
            string message = string.Format("{0} {1}: {2}", msg.Timestamp, msg.Sender.Handle, msg.Body);
            richTextBox_MessageLog.Document.Blocks.Add(new Paragraph(new Run(message)));
        }

        private void acceptSelectedChats()
        {
            activeChatsDbIds.Clear();
            if (listView_Chats.SelectedItems.Count != 0)
            {
                for (int i = 0; i < listView_Chats.SelectedItems.Count; i++)
                {
                    SkypeChats selected = (SkypeChats)listView_Chats.SelectedItems[i];
                    activeChatsDbIds.Add(selected.dBChatname);
                }
            }
        }

        #endregion

        #region no owner
        DispatcherTimer timerNoOwner = new DispatcherTimer();
        private int noOwnerChecksCount = 0;
        private void startCheckingNoOwner()
        {
            label_IsNoOwnerStarted.Foreground = Brushes.Green;
            label_IsNoOwnerStarted.Content = "Started";


            timerNoOwner.Tick += timerNoOwner_Tick;
            timerNoOwner.Interval = new TimeSpan(0, 0, 60);
            timerNoOwner.Start();
        }

        private void stopCheckingNoOwner()
        {
            label_IsNoOwnerStarted.Foreground = Brushes.Red;
            label_IsNoOwnerStarted.Content = "Stopped";
            timerNoOwner.Stop();
        }

        private void timerNoOwner_Tick(object sender, EventArgs e)
        {
            label_NoOwnerChecksCount.Content = noOwnerChecksCount++.ToString();
            try
            {
                string mail = NoOwner.checkmail();
                if (mail != null)
                {
                    SkypeStatic.sendMessage(failChat, mail);
                }
            }
            catch (Exception ex)
            {
                App.log.Trace("NoOwner.checkmail(): {0}", ex.Message);
            }

        }

        private void button_NoOwnerStop_Click(object sender, RoutedEventArgs e)
        {
            stopCheckingNoOwner();
        }

        private void button_NoOwnerStart_Click(object sender, RoutedEventArgs e)
        {
            if (timerNoOwner.IsEnabled)
            {
                //MessageBox.Show("Таймер уже запущен");
            }
            else
            {
                startCheckingNoOwner();
            }

        }

        #endregion

        #region uptime + selfcheck
        DateTime botStartTime = DateTime.Now;
        DispatcherTimer selfTest_Timer = new DispatcherTimer();

        private void startSelfTestTimer()
        {
            selfTest_Timer.Interval = new TimeSpan(0, 60, 0);
            selfTest_Timer.Tick += SelfTestTimer_Tick;
            selfTest_Timer.Start();
        }

        private void SelfTestTimer_Tick(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan uptime = currentTime - botStartTime;
            string result = string.Format("[Selftest] Аптайм: {0} дней {1} часов {2} минут {3} секунд.", uptime.Days.ToString(), uptime.Hours.ToString(), uptime.Minutes.ToString(), uptime.Seconds.ToString());
            SkypeStatic.sendMessage(testChat1, result);
        }
        #endregion








    }

    public class SkypeChats
    {
        public string friendlyChatName { get; set; }
        public string dBChatname { get; set; }
        public int CountOfMembers { get; set; }
    }
}

