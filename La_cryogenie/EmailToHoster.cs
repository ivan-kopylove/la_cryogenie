using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace La_cryogenie
{
    class EmailToHoster
    {
        DataTable free_hoster;
        string hosterType;
        string hosterName;
        string abuseEmail;
        string hosterCountry;
        string predefinedBody;
        string host;

        string myAddress = "lacryogenie@bk.ru";
        string myPassword = "wm7XMA+$f?zsnxYZQ*_yPjVJLN2^pt3d";
        string smtpServer = "smtp.bk.ru";

        public EmailToHoster(DataTable fh, string h)
        {
            this.free_hoster = fh;
            this.hosterType = free_hoster.Rows[0].Field<string>("type");
            this.hosterName = free_hoster.Rows[0].Field<string>("hoster");
            this.abuseEmail = free_hoster.Rows[0].Field<string>("abuseemail");
            this.hosterCountry = free_hoster.Rows[0].Field<string>("country");
            this.host = h;
        }

        public void processSending()
        {
            getPredef();
        }

        private void getPredef()
        {
                    DataTable predef = Sqlite.executeSearch(String.Format(
                    "SELECT predef FROM [predefs] WHERE language like '{0}' AND type like '{1}'", hosterCountry, hosterType));
                    predefinedBody = predef.Rows[0].Field<string>("predef");
                    if (predef.Rows.Count != 0)
                    {
                        sendEmail();
                    }
        }

        private void sendEmail()
        {
            abuseEmail = "stealthwar@gmail.com";

            MailMessage mailMessage = new MailMessage();
            mailMessage.SubjectEncoding = Encoding.UTF8;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.From = new MailAddress(myAddress);
            
            mailMessage.To.Add(new MailAddress(abuseEmail));
            switch (hosterType)
            {
                case "page":
                    mailMessage.Subject = string.Format("Мошеннический сайт на Вашем хостинге ( http://{0} ) ", host);
                    break;
                case "file":
                    mailMessage.Subject = string.Format("Мошеннический файл на Вашем хостинге ( http://{0} ) ", host);
                    break;
                default:
                    mailMessage.Subject = string.Format("Мошеннический URL http://{0}");
                    break;
            }
            mailMessage.Body = string.Format(predefinedBody, "http://" + host);
            mailMessage.IsBodyHtml = false;

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = smtpServer;
            smtpClient.Port = 25;
            smtpClient.EnableSsl = false;
            smtpClient.Credentials = new NetworkCredential(myAddress, myPassword);
            try
            {
                smtpClient.Send(mailMessage);
                smtpClient.Dispose();
            }
            catch (Exception ex)
            {
                App.log.Trace("Mail.Send: " + ex.Message);
            }
        }


    }
}
