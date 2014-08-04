using ActiveUp.Net.Mail;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace La_cryogenie
{
    class NoOwnerSingleton
    {
        private const string testChat1 = "#ivan.kopilov/$ae7ccff35f82918a"; //[Skype Bot testing]
        private const string failChat = "#katakan-at/$alexey.nelubov;ec5878d355b6fdea"; //[fail] Не забываем отписывать причины косяков сюда!
        #region singleton
        static NoOwnerSingleton()
        {
            Instance = new NoOwnerSingleton();
        }
        private NoOwnerSingleton() { }

        public static NoOwnerSingleton Instance { get; private set; }
        #endregion singleton
        #region timer
        DispatcherTimer timerNoOwner = new DispatcherTimer();

        public void timerNoOwner_Start()
        {
            if (!timerNoOwner.IsEnabled)
            {
                timerNoOwner.Tick += timerNoOwner_Tick;
                timerNoOwner.Interval = new TimeSpan(0, 0, 30);
                timerNoOwner.Start();

                SkypeSingleton.Instance.sendMessage(Chats.botCommandChat, "[+] Включен таймер проверки no_owner");
            }
        }

        public void timerNoOwner_Stop()
        {
            if (timerNoOwner.IsEnabled)
            {
                timerNoOwner.Stop();
                timerNoOwner.Tick -= timerNoOwner_Tick;
                SkypeSingleton.Instance.sendMessage(Chats.botCommandChat, "[-] Выключен таймер проверки no_owner");
            }
        }

        private void timerNoOwner_Tick(object sender, EventArgs e)
        {
            try
            {
                checkEmail();
            }
            catch (Exception ex)
            {
                File.AppendAllText("log/no_ownerExceptions.log", ex.Message);
            }
        }
        #endregion timer
        #region mail
        private void checkEmail()
        {
            Pop3Client pop = new Pop3Client();

            try
            {
                pop.Connect("pop.mail.ru", @"lacryogenie@mail.ru", @"FLn&maAKyq+\?_swV2=Q@.*eEbz,D%P9");
                if (pop.MessageCount > 0)
                {
                    Message message = new Message();
                    try
                    {
                        message = pop.RetrieveMessageObject(1);
                    }
                    catch (Exception)
                    {

                    }

                    if (message.Subject.Contains("[fail][no_owner]"))
                    {
                        string messageToPost = "";
                        using (StringReader reader = new StringReader(message.BodyHtml.Text))
                        {
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
                                line = Utilities.StripHTML(line);

                                if (line.Contains("[ Operator"))
                                {
                                    Regex regexOperatorN = new Regex(@"(0|[1-9][0-9]*)"); // [ Operator1007 ] updated #XXX-66-666 exract 1007
                                    string OperatorN = regexOperatorN.Match(line).Value;

                                    string das = string.Format("{0} {1} {2}", OperatorN, OperatorN.Length.ToString(), line);

                                    switch (OperatorN.Length)
                                    {
                                        case 2:
                                        case 3:
                                            messageToPost += line + Environment.NewLine;
                                            break;
                                        default:
                                            pop.DeleteMessage(1);
                                            return;
                                    }
                                }

                                if (line.Contains("URL:"))
                                {
                                    messageToPost += line + Environment.NewLine;
                                }
                            }
                        }
                        messageToPost = "[no_owner] " + messageToPost;
                        SkypeSingleton.Instance.sendMessage(failChat, messageToPost);
                    }
                    pop.DeleteMessage(1);
                }
            }
            finally
            {
                if (pop.IsConnected)
                {
                    pop.Disconnect();
                }
            }
        }
        #endregion
    }
}
