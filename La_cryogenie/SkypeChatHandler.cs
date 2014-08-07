using FishLib;
using SKYPE4COMLib;
using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace La_cryogenie
{
    class SkypeChatHandler
    {

        private const string chatCommandTrigger = "!";

        ChatMessage msg;

        public SkypeChatHandler(ChatMessage skypeMessage)
        {
            this.msg = skypeMessage;
        }

        private void handle_meAddedMembers()
        {
            switch (msg.ChatName)
            {
                case Chats.botBustersChat:

                    UserCollection botBusers_users = msg.Users;
                    foreach (User user in botBusers_users)
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, @"Привет! Это чат противодействия спам-ботам.
При помощи команд мне сдаются мошеннические ссылки.
Если ты увидел в тикете, чатлогах или на сторонних ресурсах ссылку на вредоносный ресурс (фишинг, продажа игровых ценностей за реал, и т.п.), то зарепортить можно в этот чат.
В заголовке чата есть ссылка на инструкцию.");
                    }
                    break;
                case Chats.InvestigationSectorAndFirstLineChat:
                    UserCollection sector_users = msg.Users;
                    foreach (User user in sector_users)
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, @"Привет! Это чат с Сектором расследований.
Здесь можно задать вопрос касательно обработки запросов по взломам. 
Не забывай ставить тег проекта, когда задаешь вопрос.");
                    }
                    break;
                case Chats.botCommandChat:
                    UserCollection commandChat_users = msg.Users;
                    foreach (User user in commandChat_users)
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, @"Привет! Это командный чат бота. Здесь мной управляют.");
                    }
                    break;
                default:
                    break;
            }
        }


        public void handleMsg()
        {
            switch (msg.Type)
            {
                case TChatMessageType.cmeAddedMembers:
                    handle_meAddedMembers();
                    break;
                case TChatMessageType.cmeCreatedChatWith:
                    break;
                case TChatMessageType.cmeEmoted:
                    break;
                case TChatMessageType.cmeGapInChat:
                    break;
                case TChatMessageType.cmeJoinedAsApplicant:
                    break;
                case TChatMessageType.cmeKickBanned:
                    break;
                case TChatMessageType.cmeKicked:
                    break;
                case TChatMessageType.cmeLeft:
                    break;
                case TChatMessageType.cmePostedContacts:
                    break;
                case TChatMessageType.cmeSaid:
                    if (msg.Chat.Members.Count < 3)
                    {
                        handlePrivateMessage();
                    }
                    else
                    {
                        handlePublicMessage();
                    }
                    break;
                case TChatMessageType.cmeSawMembers:
                    break;
                case TChatMessageType.cmeSetGuidelines:
                    break;
                case TChatMessageType.cmeSetOptions:
                    break;
                case TChatMessageType.cmeSetPicture:
                    break;
                case TChatMessageType.cmeSetRole:
                    break;
                case TChatMessageType.cmeSetTopic:
                    break;
                case TChatMessageType.cmeUnknown:
                    break;
                default:
                    break;
            }

        }

        private void handlePublicMessage()
        {
            //все-таки нужна эта проверка. парсинг строки умирает от хитрых ссылок и неожиданных вхождений
            if (msg.Body.IndexOf(chatCommandTrigger) == -1)
            {
                //что позиция в строке нужного сивола - 0, то есть в начале
                //если не 0, то команда начинается не с нужного символа
                // Продолжаем только если входящее сообщение - триггер (воскл знак)
                return;
            }


            File.AppendAllText("log/public_commands.txt", string.Format("[{0}] [{1}] {2}: {3}\n", msg.Timestamp, msg.Sender.Handle, msg.Chat.TopicXML, msg.Body));

            switch (msg.ChatName)
            {
                case Chats.botBustersChat:
                case Chats.AutoReports:
                    handle_BotBustersMsg();
                    break;
                case Chats.noticesSettingsChat:
                    handle_noticesSettingsChat();
                    break;
                case Chats.botCommandChat:
                    handleCommander();
                    break;
                default:
                    break;
            }
        }

        private string[] prepareOneLineCommand(string msgBody)
        {
            string command = msgBody.Remove(0, chatCommandTrigger.Length);
            WindowsHandler.textBox_skypeCommandsLog(command, msg);

            //замена двойных пробелов одинарыми:

            if (Regex.Match(command, " {2,}").Success)
            {
                //SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("{0}, у тебя в команде затесались лишние пробелы. Я бережно убрала их и выполняю команду дальше", msg.Sender.FullName));
                command = Regex.Replace(command, " {2,}", " ");
            }

            //~~~~~~args by space~~~~~~~
            string[] commandArguments = command.Split(' ');
            commandArguments[0] = commandArguments[0].ToLower();
            //~~~~~~args by space~~~~~~~
            WindowsHandler.textBox_skypeCommandsLog(string.Format("Команда к выполнению: {0} \n", command), msg);
            return commandArguments;
        }

        private string getGameProjects(string[] commandArguments)
        {
            //тут можно будет дёргать по регекспу игровые проекты
            string game_projects = null;
            if (commandArguments.Length > 2)
            {
                for (int i = 2; i < commandArguments.Length; i++)
                {
                    game_projects += commandArguments[i];
                }
                game_projects = game_projects.ToUpper();
            }

            return game_projects;
        }

        private bool isGameProjectsValid(string gameProjects)
        {
            string[] splittedProjects = gameProjects.Split(',');
            foreach (string splittedProject in splittedProjects)
            {
                if (splittedProject.Length > 3)
                {
                    SkypeSingleton.Instance.sendMessage(msg.ChatName,
                    string.Format("{0}, длина аббревиатуры проекта ({1}) не должна превышать трёх символов",
                    msg.Sender.FullName, splittedProject));
                    return false;
                }

                /*
                [A-z,a-z] - латинская буква
                [^A-za-z] - не латинская буква.
                Если второй регкэсп вернет Sucсess - значит, в строке есть по крайней мере одна не латинская буква.
                 */
                if (Regex.Match(splittedProject, @"[^A-za-z]").Success)
                {
                    SkypeSingleton.Instance.sendMessage(msg.ChatName,
                    string.Format("{0}, аббревиатура проекта ({1}) должна быть написана латинскими буквами",
                    msg.Sender.FullName, splittedProject));
                    return false;
                }
            }
            return true;
        }

        #region chats handling



        private bool isCommandLengthError(string[] args, int maxlength)
        {
            if (args.Length < maxlength)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void handle_BotBustersMsg()
        {
            string[] commandArguments = prepareOneLineCommand(msg.Body);

            if (msg.Body.Contains("vk.com/"))
            {
                SkypeSingleton.Instance.sendMessage(msg.ChatName, "Неугодные сообщества vk.com следует передавать КМам в комменты на странице:\nhttps://confluence.mail.ru/x/0KjRAQ");
                return;
            }
            string game_projects = null;
            URLTools urltool;


            switch (commandArguments[0])
            {
                case "test":
                case "тест":
                    SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("{0}", commandArguments[0]));
                    break;
                case "стат":
                case "stat":
                    Stat stat = new Stat(msg, commandArguments);
                    stat.continueStatCommand();
                    break;
                case "unreported":
                case "unrtd":
                case "неотправленные":
                case "неотпр":
                    Unreported unreported = new Unreported(msg, commandArguments);
                    unreported.postUnreported();
                    break;
                case "зарепорчу":
                case "отрепорчу":
                case "ireport":
                    Ireport report = new Ireport(msg, commandArguments);
                    report.report();
                    break;
                case "ф":
                case "ph":
                case "фишинг":
                case "phishing":
                case "заебали":
                case "хуишинг":
                    if (isCommandLengthError(commandArguments, 3))
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("{0}, ошибка в синтаксисе. Нужно так: \"!{1} [ссылка] проекты_через_запятую\"", msg.Sender.FullName, commandArguments[0]));
                        return;
                    }

                    game_projects = getGameProjects(commandArguments);
                    if (!isGameProjectsValid(game_projects))
                    {
                        return;
                    }

                    urltool = new URLTools(commandArguments[1]);

                    if (urltool.host == null)
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName,
                        string.Format("{0}, что-то не так в ссылке ({1}). Исправь и попробуй ещё раз", msg.Sender.FullName, urltool.original_url));
                        return;
                    }

                    PhishingLink ph = new PhishingLink(urltool.host);
                    if (ph.HasDbRecord)
                    {
                        if (ph.IsReported)
                        {
                            SkypeSingleton.Instance.sendMessage(msg.ChatName,
                            string.Format("{0}, «{1}» уже сдан мне. Последний раз отправляли хостеру {2}",
                            msg.Sender.FullName, urltool.host, ph.LastReportToHoster.ToString()));

                            TimeSpan ts = new TimeSpan(2, 0, 0, 0);
                            if (DateTime.Now - ph.LastReportToHoster > ts)
                            {
                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                string.Format("По всей видимости, за {0} дня ресурс не забыл закрыт. «{1}» помечен как неотправленный",
                                ts.Days, urltool.host));
                                ph.markUnSent(urltool.host);
                            }
                        }
                        else
                        {
                            SkypeSingleton.Instance.sendMessage(msg.ChatName,
                            string.Format("{0}, «{1}» уже сдан мне, однако, хостеру его ещё не отправляли.",
                            msg.Sender.FullName, urltool.host));
                        }
                    }
                    else
                    {
                        Hoster freeHoster = new Hoster(urltool);
                        if (freeHoster.hasRecord())
                        {


                            if (freeHoster.hasEmail())
                            {
                                PredefGenerator predef = new PredefGenerator(freeHoster.Name, freeHoster.Country, freeHoster.Type, urltool.host);
                                CyberMailer cMailer = new CyberMailer(freeHoster.AbuseEmail, predef.getBody(), predef.getSubject());
                                cMailer.sendEmail();

                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                string.Format("Спасибо, {0}! Я отправила письмо хостеру «{1}» ( {2} ) на {3} с жалобой на «{4}»", msg.Sender.FullName, freeHoster.Name, freeHoster.HomePage, freeHoster.AbuseEmail, urltool.host));
                                ph.insertNewLink(msg.Sender.Handle, SkypeSingleton.Instance.skype.CurrentUserHandle, urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), Utilities.getCurrentUnixTime());
                                return;
                            }
                            else
                            {
                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                string.Format("Спасибо, {0}! «{1}» принадлежит хостеру «{2}» ( {3} ). Для него не прописана почта, но есть страница обратной связи: {4} ",
                                msg.Sender.FullName, urltool.host, freeHoster.Name, freeHoster.HomePage, freeHoster.AbusePage));
                                ph.insertNewLink(msg.Sender.Handle, "NULL", urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), 0);
                                return;
                            }
                        }
                        else
                        {
                            if (msg.Sender.Handle != "ivan.kopilov")
                            {
                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                string.Format("«{0}» помечен как неотправленный *", urltool.host));
                                ph.insertNewLink(msg.Sender.Handle, "NULL", urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), 0);
                                return;
                            }



                            DomainTools dt = new DomainTools(urltool.host);
                            if (!dt.hasXml)
                            {
                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                string.Format("«{0}» помечен как неотправленный **", urltool.host));
                                ph.insertNewLink(msg.Sender.Handle, "NULL", urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), 0);
                                return;
                            }
                            else
                            {
                                if (dt.nServers.Count > 0)
                                {
                                    foreach (string ns in dt.nServers)
                                    {
                                        Hoster paidHoster = new Hoster(ns);
                                        if (paidHoster.hasRecord())
                                        {
                                            if (paidHoster.hasEmail())
                                            {
                                                PredefGenerator predef = new PredefGenerator(paidHoster.Name, paidHoster.Country, paidHoster.Type, urltool.host);
                                                CyberMailer cMailer = new CyberMailer(paidHoster.AbuseEmail, predef.getBody(), predef.getSubject());
                                                cMailer.sendEmail();

                                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                                string.Format("Спасибо, {0}! Я отправила письмо хостеру «{1}» ( {2} ) на {3} с жалобой на «{4}»",
                                                msg.Sender.FullName, paidHoster.Name, paidHoster.HomePage, paidHoster.AbuseEmail, urltool.host));

                                                ph.insertNewLink(msg.Sender.Handle, SkypeSingleton.Instance.skype.CurrentUserHandle, urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), Utilities.getCurrentUnixTime());
                                                return;
                                            }
                                            else
                                            {
                                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                                string.Format("Спасибо, {0}! «{1}» принадлежит хостеру «{2}» ( {3} ). Для него не прописана почта, но есть страница обратной связи: {4} ",
                                                msg.Sender.FullName, urltool.host, paidHoster.Name, paidHoster.HomePage, paidHoster.AbusePage));
                                                ph.insertNewLink(msg.Sender.Handle, "NULL", urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), 0);
                                                return;
                                            }
                                        }
                                    }
                                }

                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                string.Format("«{0}» помечен как неотправленный ***", urltool.host));
                                ph.insertNewLink(msg.Sender.Handle, "NULL", urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), 0);
                            }


                            return;
                        }
                    }
                    break;



                case "файл":
                case "file":
                case "malware":
                    if (isCommandLengthError(commandArguments, 3))
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("{0}, ошибка в синтаксисе. Нужно так: \"!{1} [ссылка] проекты_через_запятую\"", msg.Sender.FullName, commandArguments[0]));
                        return;
                    }

                    game_projects = getGameProjects(commandArguments);
                    if (!isGameProjectsValid(game_projects))
                    {
                        return;
                    }

                    urltool = new URLTools(commandArguments[1]);

                    if (urltool.host == null)
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName,
                        string.Format("{0}, что-то не так в ссылке ({1}). Исправь и попробуй ещё раз", msg.Sender.FullName, urltool.original_url));
                        return;
                    }

                    MalwareLink mw = new MalwareLink(urltool.original_url);
                    if (mw.alreadyInDb())
                    {
                        //уже в базе
                        if (mw.reported())
                        {
                            SkypeSingleton.Instance.sendMessage(msg.ChatName,
                            string.Format("{0}, «{1}» уже сдан мне. Последний раз отправляли хостеру {2}",
                            msg.Sender.FullName, urltool.original_url, mw.LastReportToHosterString));
                        }
                        else
                        {
                            SkypeSingleton.Instance.sendMessage(msg.ChatName,
                            string.Format("{0}, «{1}» уже сдан мне, однако, хостеру его ещё не отправляли.",
                            msg.Sender.FullName, urltool.original_url));
                        }
                    }
                    else
                    {
                        Hoster freeHoster = new Hoster(urltool);
                        if (freeHoster.hasRecord())
                        {
                            if (freeHoster.hasEmail())
                            {
                                PredefGenerator predef = new PredefGenerator(freeHoster.Name, freeHoster.Country, freeHoster.Type, urltool.original_url);
                                CyberMailer cMailer = new CyberMailer(freeHoster.AbuseEmail, predef.getBody(), predef.getSubject());
                                cMailer.sendEmail();

                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                string.Format("Спасибо, {0}! Я отправила письмо хостеру «{1}» ( {2} ) на {3} с жалобой на «{4}»", msg.Sender.FullName, freeHoster.Name, freeHoster.HomePage, freeHoster.AbuseEmail, urltool.original_url));
                                mw.insertNewLink(msg.Sender.Handle, SkypeSingleton.Instance.skype.CurrentUserHandle, urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), Utilities.getCurrentUnixTime());
                                return;
                            }
                            else
                            {
                                SkypeSingleton.Instance.sendMessage(msg.ChatName,
                                string.Format("Спасибо, {0}! «{1}» принадлежит хостеру «{2}» ( {3} ). Для него не прописана почта, но есть страница обратной связи: {4} ",
                                msg.Sender.FullName, urltool.host, freeHoster.Name, freeHoster.HomePage, freeHoster.AbusePage));
                                mw.insertNewLink(msg.Sender.Handle, "NULL", urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), 0);
                                return;
                            }
                        }
                        else
                        {
                            SkypeSingleton.Instance.sendMessage(msg.ChatName,
                            string.Format("Записей о домене второго уровня «{0}» в базе бесплатных хостингов не найдено. «{1}» помечен как неотправленный.",
                            urltool.secondLevelDomain, urltool.host));
                            mw.insertNewLink(msg.Sender.Handle, "NULL", urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), 0);
                            return;
                        }
                    }

                    break;
                case "читы":
                case "cheats":
                case "ch":
                case "читоры":
                    if (isCommandLengthError(commandArguments, 3))
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("{0}, ошибка в синтаксисе. Нужно так: \"!{1} [ссылка] проекты_через_запятую\"", msg.Sender.FullName, commandArguments[0]));
                        return;
                    }

                    game_projects = getGameProjects(commandArguments);
                    if (!isGameProjectsValid(game_projects))
                    {
                        return;
                    }

                    urltool = new URLTools(commandArguments[1]);

                    if (urltool.host == null)
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName,
                        string.Format("{0}, что-то не так в ссылке ({1}). Исправь и попробуй ещё раз", msg.Sender.FullName, urltool.original_url));
                        return;
                    }

                    CheatPortal cPortal = new CheatPortal(urltool.host);
                    if (cPortal.alreadyInDb())
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! Я обновила статус активности «{1}»", msg.Sender.FullName, urltool.host));
                        cPortal.updateLink(game_projects);
                    }
                    else
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! Ты первый добавил «{1}»", msg.Sender.FullName, urltool.host));
                        cPortal.insertNewLink(msg.Sender.Handle, "NULL", urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), -1);
                    }
                    break;
                case "gold":
                case "голд":
                case "goldseller":
                case "барыга":
                case "барыги":
                case "продавец":

                    if (isCommandLengthError(commandArguments, 3))
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("{0}, ошибка в синтаксисе. Нужно так: \"!{1} [ссылка] [проекты_через_запятую]\"", msg.Sender.FullName, commandArguments[0]));
                        return;
                    }

                    game_projects = getGameProjects(commandArguments);
                    if (!isGameProjectsValid(game_projects))
                    {
                        return;
                    }

                    urltool = new URLTools(commandArguments[1]);

                    if (urltool.host == null)
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName,
                        string.Format("{0}, что-то не так в ссылке ({1}). Исправь и попробуй ещё раз", msg.Sender.FullName, urltool.original_url));
                        return;
                    }

                    GoldSellerLink gseller = new GoldSellerLink(urltool);
                    if (gseller.alreadyInDb())
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! Я обновила статус активности «{1}»", msg.Sender.FullName, urltool.host));
                        gseller.updateLink(game_projects);
                    }
                    else
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Спасибо, {0}! Ты первый добавил «{1}»", msg.Sender.FullName, urltool.host));
                        gseller.insertNewLink(msg.Sender.Handle, "NULL", urltool.host, urltool.original_url, game_projects, Utilities.getCurrentUnixTime(), -1);
                    }
                    break;
                case "площадка":
                case "platform":
                    SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Команда «{0}» выпилена за редкостью использования", commandArguments[0]));
                    break;
                case "info":
                case "инфо":
                case "и":
                case "i":
                    if (commandArguments.Length < 2)
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Неправильный формат команды. Наберите !{0} URL", commandArguments[0]));
                        break;
                    }

                    urltool = new URLTools(commandArguments[1]);
                    DomainTools dt2 = new DomainTools(urltool.host);
                    if (!dt2.hasXml)
                    {
                        SkypeSingleton.Instance.sendMessage(msg.ChatName, "[DomainTools.com] Не удалось получить никакой информации.");
                        return;
                    }
                    break;
                case "тест2":
                case "test2":

                    break;
                default:
                    break;
            }

        }

        private void handle_noticesSettingsChat()
        {
            string[] commandArguments = prepareOneLineCommand(msg.Body);

            switch (commandArguments[0])
            {
                case "добавить":
                case "add":
                    //Notices newNotice = new Notices(msg, commandArguments);
                    //newNotice.addNewNotice();
                    SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Команда «{0}» временно выключена", commandArguments[0]));
                    break;
                case "список":
                case "list":
                    //Notices listOfNotices = new Notices(msg);
                    //listOfNotices.postListOfNotices();
                    SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Команда «{0}» временно выключена", commandArguments[0]));
                    break;
                case "удалить":
                case "delete":
                    //Notices deleteNotice = new Notices(msg, commandArguments);
                    //deleteNotice.deleteNotice();
                    SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Команда «{0}» временно выключена", commandArguments[0]));
                    break;
                case "применить":
                case "apply":
                    //Notices applyNotices = new Notices();
                    //applyNotices.refreshListOfActiveNotices();
                    SkypeSingleton.Instance.sendMessage(msg.ChatName, string.Format("Команда «{0}» временно выключена", commandArguments[0]));
                    break;
                default:
                    break;
            }
        }

        private void handleCommander()
        {
            if (!(msg.Sender.Handle == "lazyf1sh" || msg.Sender.Handle == "ivan.kopilov"))
            {
                return;
            }

            string[] commandArguments = prepareOneLineCommand(msg.Body.ToLower());
            switch (commandArguments[0])
            {

                case "sql":
                    break;
                case "запустить":
                case "run":
                case "старт":
                case "start":
                    switch (commandArguments[1])
                    {
                        case "сообщения":
                        case "прием":
                        case "receiving":
                            SkypeSingleton.Instance.startReceiving();
                            break;
                        case "бэдчат":
                        case "бедчат":
                        case "badchat":
                            AutoReportSingleTon.Instance.Start();
                            break;
                        case "selftest":
                        case "селфтест":
                            SelfTestSingleton.Instance.Start();
                            break;
                        case "нотайсы":
                        case "notices":
                            NoticesSingleton.Instance.startChecking();
                            break;
                        case "noowner":
                        case "ноовнер":
                        case "ноувнер":
                            NoOwnerSingleton.Instance.timerNoOwner_Start();
                            break;
                        case "all":
                        case "все":
                            SkypeSingleton.Instance.startReceiving();
                            AutoReportSingleTon.Instance.Start();
                            SelfTestSingleton.Instance.Start();
                            NoticesSingleton.Instance.startChecking();
                            NoOwnerSingleton.Instance.timerNoOwner_Start();
                            break;
                        default:
                            break;
                    }
                    break;
                case "остановить":
                case "stop":
                case "стоп":
                    switch (commandArguments[1])
                    {
                        case "сообщения":
                        case "прием":
                        case "receiving":
                            SkypeSingleton.Instance.stopReceiving();
                            break;
                        case "бэдчат":
                        case "бедчат":
                        case "badchat":
                            AutoReportSingleTon.Instance.Stop();
                            break;
                        case "selftest":
                        case "селфтест":
                            SelfTestSingleton.Instance.Stop();
                            break;
                        case "нотайсы":
                        case "notices":
                            NoticesSingleton.Instance.StopChecking();
                            break;
                        case "noowner":
                        case "ноовнер":
                        case "ноувнер":
                            NoOwnerSingleton.Instance.timerNoOwner_Stop();
                            break;
                        case "all":
                        case "все":
                            SkypeSingleton.Instance.stopReceiving();
                            AutoReportSingleTon.Instance.Stop();
                            SelfTestSingleton.Instance.Stop();
                            NoticesSingleton.Instance.StopChecking();
                            NoOwnerSingleton.Instance.timerNoOwner_Stop();
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    SkypeSingleton.Instance.sendMessage(Chats.botCommandChat, string.Format("Нет команды «{0}»", commandArguments[0]));
                    break;
            }
        }
        #endregion

        #region private messages handling // under construction
        private void handlePrivateMessage()
        {
            SkypeSingleton.Instance.sendMessage(msg.ChatName, "Я ещё не обучена отвечать на приватные сообщения.");
        }

        #endregion
    }
}
