using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace La_cryogenie
{
    class CyberMailer
    {
        string myAddress = @"investigations@corp.mail.ru";
        string myPassword = @"p3boRKfOvK7J";
        string smtpServer = @"smtp.mail.ru";

        string abuseEmail;
        string body;
        string subject;

        public CyberMailer(string abuseEmail_, string body_, string subject_)
        {
            this.abuseEmail = abuseEmail_;
            this.body = body_;
            this.subject = subject_;


        }

        public void sendEmail()
        {
            //abuseEmail = "stealthwar@gmail.com"; //закомментить когда нужно будет отправлять хостеру

            MailMessage mailMessage = new MailMessage();
            mailMessage.SubjectEncoding = Encoding.UTF8;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.From = new MailAddress(myAddress);

            mailMessage.To.Add(new MailAddress(abuseEmail)); //раскомменить когда нужно будет отправлять хостеру
            mailMessage.To.Add(new MailAddress(myAddress));
            mailMessage.Subject = subject;

            mailMessage.Body = body + Environment.NewLine + 
                @"-- 
Cybercrime Investigations Division
Mail.Ru Games
investigations@corp.mail.ru

The information contained in this e-mail is intended only for the use of
the individuals to whom it is addressed and may contain information
which is privileged and confidential. Thank you.";
            mailMessage.IsBodyHtml = false;

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = smtpServer;
            //дальше смотрим настройки как в почтовом клиенте
            smtpClient.Port = 25; //25 - для обычного, 465 - для SSL smtp исходящей
            smtpClient.EnableSsl = false; // true - 465, false для 25

            //(можно отправлять на свой же адрес копию)
            //smtpClient.PickupDirectoryLocation = @"ByBot";
            //smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;

            smtpClient.Credentials = new NetworkCredential(myAddress, myPassword);
            try
            {
                smtpClient.Send(mailMessage);
                smtpClient.Dispose();
            }
            catch (Exception ex)
            {
                SkypeSingleton.Instance.sendMessage(Chats.botCommandChat, "Проблема с выполнением «smtpClient.Send(mailMessage)» === " + ex.Message);
            }
        }

    }
}
