using HigLabo.Net.Mail;
using HigLabo.Net.Pop3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Web;
using System.Text.RegularExpressions;

namespace La_cryogenie
{
    public static class NoOwner
    {

        public static string checkmail()
        {
            Pop3Client cl = new Pop3Client("pop.mail.ru");
            cl.UserName = "lacryogenie@mail.ru";
            cl.Password = @"FLn&maAKyq+\?_swV2=Q@.*eEbz,D%P9";
            cl.Ssl = false;
            if (cl.Authenticate() == true)
            {
                int totalMailCount = (int)cl.GetTotalMessageCount();
                if (totalMailCount > 0)
                {
                    long mailIndex = 1;
                    MailMessage mg = cl.GetMessage(mailIndex);
                    string mailTo = mg.To;
                    string mailCc = mg.Cc;
                    string title = mg.Subject;
                    string bodyText = mg.BodyText;
                    string returnPath = mg["Return-Path"];

                    long[] listToDel = new long[] { 1 };
                    cl.DeleteMail(listToDel);
                    
                    if (title.Contains("[fail][no_owner]"))
                    {
                        string retunLine = "";
                        using (StringReader reader = new StringReader(bodyText))
                        {
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.Contains("URL:") | line.Contains("[ Operator"))
                                {
                                    retunLine += Utilities.StripHTML(line) + Environment.NewLine;
                                    
                                }
                            }
                        }
                        retunLine = "[fail] [no_owner] " + retunLine;
                        return retunLine;
                    }
                    return null;
                }
                else
                {
                    return null;
                }

            }
            return null;
        }

    }
}
