using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKYPE4COMLib;
using System.Data;

namespace La_cryogenie
{
    public static class SkypeStatic
    {
        private static DataTable allSmilies = Sqlite.executeSearch("SELECT * FROM [skype-smilies]");
        
        public static void sendMessage(string chat, string message)
        {
            try
            {
                Skype skypeSendMessage = new Skype(); //основной экземляр скайпа
                skypeSendMessage.Chat[chat].SendMessage(message);
            }
            catch (Exception ex)
            {
                App.log.Trace("Ошибка с отправкой скайп-сообщения SkypeStatic SendMessage: {0}, {1}", ex.Message, ex.TargetSite.ToString());
            }

        }

        public static string getRandomSmile()
        {
            Random rand = new Random();
            int smileIndex = rand.Next(0, allSmilies.Rows.Count);
            return allSmilies.Rows[smileIndex].Field<string>("smile");
        }

    }
}