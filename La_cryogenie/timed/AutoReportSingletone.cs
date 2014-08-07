using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SKYPE4COMLib;
using FishLib;

namespace La_cryogenie
{
    class AutoReportSingleTon
    {
        #region singleton
        static AutoReportSingleTon()
        {
            Instance = new AutoReportSingleTon();
        }
        private AutoReportSingleTon() { }

        public static AutoReportSingleTon Instance { get; private set; }
        #endregion

        DispatcherTimer timer = new DispatcherTimer();
        private static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public void Start()
        {
            if (!timer.IsEnabled)
            {
                firstReport();

                timer.Tick += timerTick;
                timer.Interval = new TimeSpan(0, 2, 30);
                timer.Start();

                SkypeSingleton.Instance.sendMessage(Chats.botCommandChat, "[+] Включен таймер мониторинга файлов из бедчата");
            }
        }

        public void Stop()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                timer.Tick -= timerTick;

                SkypeSingleton.Instance.sendMessage(Chats.botCommandChat, "[+] Выключен таймер мониторинга файлов из бедчата");
            }
        }

        private void timerTick(object sender, EventArgs e)
        {
            firstReport();

        }

        private void firstReport()
        {
            string finalUrlsPath_ToReport = desktopPath + string.Format(@"\db\BadChatLinkExtractor\URLs_final_ToReport_wf.log");

            string[] wf_links_to_report;
            try
            {
                wf_links_to_report = File.ReadAllLines(finalUrlsPath_ToReport);
            }
            catch (Exception)
            {
                return;
            }
            

            if (wf_links_to_report.Length > 0)
            {
                foreach (var wf_link in wf_links_to_report)
                {
                    continueReport(wf_link, "WF");
                }
            }

            File.WriteAllText(finalUrlsPath_ToReport, "");



            string finalUrlsPath_ToCheck = desktopPath + string.Format(@"\db\BadChatLinkExtractor\URLs_final_ToCheck_wf.log");

            string[] wf_links_to_check;

            try
            {
                wf_links_to_check = File.ReadAllLines(finalUrlsPath_ToCheck);
            }
            catch (Exception)
            {
                return;
            }

            if (wf_links_to_check.Length > 0)
            {
                string post = null;
                foreach (var wf_link in wf_links_to_check)
                {
                    post += string.Format("[{0}] {1}\n", "WF", wf_link);
                }

                SkypeSingleton.Instance.sendMessage(Chats.botBustersChat, post);
            }

            
            File.WriteAllText(finalUrlsPath_ToCheck, "");
        }

        private void continueReport(string uri, string project)
        {
            URLTools urtool = new URLTools(uri);
            PhishingLink ph = new PhishingLink(urtool.host);

            if (ph.HasDbRecord)
            {
                //уже в базе
                if (ph.IsReported)
                {
                    SkypeSingleton.Instance.sendMessage(Chats.AutoReports,
                    string.Format("[Авторепорт] «{0}» уже сдан мне. Последний раз отправляли хостеру {1}",
                    urtool.host, ph.LastReportToHoster.ToString()));
                }
                else
                {
                    SkypeSingleton.Instance.sendMessage(Chats.AutoReports,
                    string.Format("[Авторепорт] «{0}» уже сдан мне, однако, хостеру его ещё не отправляли.",
                    urtool.host));
                }
            }
            else
            {
                Hoster hst = new Hoster(urtool);
                if (hst.hasRecord())
                {


                    if (hst.hasEmail())
                    {
                        PredefGenerator predef = new PredefGenerator(hst.Name, hst.Country, hst.Type, urtool.host);
                        CyberMailer cMailer = new CyberMailer(hst.AbuseEmail, predef.getBody(), predef.getSubject());
                        cMailer.sendEmail();

                        SkypeSingleton.Instance.sendMessage(Chats.AutoReports,
                        string.Format("[Авторепорт] Я отправила письмо хостеру «{0}» ( {1} ) на {2} с жалобой на «{3}»",
                        hst.Name, hst.HomePage, hst.AbuseEmail, urtool.host));
                        ph.insertNewLink(SkypeSingleton.Instance.skype.CurrentUserHandle, SkypeSingleton.Instance.skype.CurrentUserHandle, urtool.host, urtool.original_url, project, Utilities.getCurrentUnixTime(), Utilities.getCurrentUnixTime());
                        return;
                    }
                    else
                    {
                        SkypeSingleton.Instance.sendMessage(Chats.AutoReports,
                        string.Format("[Авторепорт] «{0}» принадлежит хостеру «{1}» ( {2} ). Для него не прописана почта, но есть страница обратной связи: {3} ",
                        urtool.host, hst.Name, hst.HomePage, hst.AbusePage));
                        ph.insertNewLink(SkypeSingleton.Instance.skype.CurrentUserHandle, "NULL", urtool.host, urtool.original_url, project, Utilities.getCurrentUnixTime(), 0);
                        return;
                    }
                }
                else
                {
                    SkypeSingleton.Instance.sendMessage(Chats.AutoReports,
                    string.Format("[Авторепорт] Записей о домене второго уровня «{0}» в базе бесплатных хостингов не найдено. «{1}» помечен как неотправленный.",
                    urtool.secondLevelDomain, urtool.host));
                    ph.insertNewLink(SkypeSingleton.Instance.skype.CurrentUserHandle, "NULL", urtool.host, urtool.original_url, project, Utilities.getCurrentUnixTime(), 0);
                    return;
                }
            }
        }
    }
}
