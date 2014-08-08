using SKYPE4COMLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;

namespace La_cryogenie
{
    class SkypeSingleton
    {
        static SkypeSingleton()
        {
            Instance = new SkypeSingleton();

            msgQueControlTimer.Interval = new TimeSpan(0, 0, 0, 2);
            msgQueControlTimer.Tick += msgQueControlTimer_Tick;
            msgQueControlTimer.Start();
        }
        private SkypeSingleton() { }


        public static SkypeSingleton Instance { get; private set; }

        private static DispatcherTimer msgQueControlTimer = new DispatcherTimer();
        private static List<ChatMessage> msgQue = new List<ChatMessage>();
        private static void msgQueControlTimer_Tick(object sender, EventArgs e)
        {
            WindowsHandler.statusbar2_log(string.Format("Очередь команд: {0}", msgQue.Count.ToString()));
            if (msgQue.Count > 0)
            {
                if (msgQue.Count > 10)
                {
                    msgQueControlTimer.Interval = new TimeSpan(0, 0, 0, 15);
                }
                else
                {
                    msgQueControlTimer.Interval = new TimeSpan(0, 0, 0, 5);
                }

                SkypeChatHandler skype_handler = new SkypeChatHandler(msgQue[0]);
                skype_handler.handleMsg();
                msgQue.RemoveAt(0);
            }
        }
        public Skype skype = new Skype();


        private bool isMessageReceivingEnabled { get; set; }
        public void startReceiving()
        {
            if (!isMessageReceivingEnabled)
            {
                isMessageReceivingEnabled = true;
                sendChatMessage(Chats.botCommandChat, "[+] Включен глобальный прием сообщений");
            }
        }

        public void stopReceiving()
        {
            if (isMessageReceivingEnabled)
            {
                isMessageReceivingEnabled = false;
                sendChatMessage(Chats.botCommandChat, "[-] Выключен глобальный прием сообщений");
            }
        }
        /*
        * есть глюк, когда события обрабатываются дважды. 
        * Массив полученных сообщений. Дублирующиеся сообщения лесом
        */
        private static List<int> receivedMessagesIds = new List<int>();
        public void skype_MessageReceived(ChatMessage msg, TChatMessageStatus status)
        {
            //сюда поступают все события
            if (receivedMessagesIds.Contains(msg.Id)) // - отфильтровываются дублирующие событие с одинаковыми id
            {
                return;
            }
            receivedMessagesIds.Add(msg.Id);

            switch (status)
            {
                case TChatMessageStatus.cmsRead:
                case TChatMessageStatus.cmsSending:
                case TChatMessageStatus.cmsUnknown:
                    break;
                case TChatMessageStatus.cmsSent:
                    break;
                /*
                * TChatMessageStatus.cmsReceived - нужна эта проверка. Skype api принимает сообщение с этим статусом. 
                * Если не фильтровать статус, то сообщения будут дублироваться при прочтении на скайп-клиенте руками 
                * (будет генерироваться событие read при котором все полученные сообщения вновь будут приходить сюда)
                */
                case TChatMessageStatus.cmsReceived:
                    File.AppendAllText("log/all_messages.txt", string.Format("[{0}] [{1}] {2}: {3}\n", msg.Timestamp, msg.Sender.Handle, msg.Chat.TopicXML, msg.Body));

                    switch (msg.ChatName)
                    {
                        case Chats.botCommandChat:
                            SkypeChatHandler skype_handler = new SkypeChatHandler(msg);
                            skype_handler.handleMsg();
                            break;
                        default:
                            if (isMessageReceivingEnabled)
                            {
                                msgQue.Add(msg);
                            }
                            break;
                    }

                    break;
                default:
                    break;
            }
        }

        public void sendChatMessage(string chat, string message)
        {
            this.skype.Chat[chat].SendMessage(message);
        }

        public void sendPrivateMessageBySkypeLogin(string username, string message)
        {
            this.skype.SendMessage(username, message);
        }
    }
}
