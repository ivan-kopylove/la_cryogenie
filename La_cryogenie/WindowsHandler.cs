using SKYPE4COMLib;
using System;

namespace La_cryogenie
{
    public static class WindowsHandler
    {
        public static MainWindow MainWindow { private get; set; }


        #region textBox_log
        public static void textBox_log(string log)
        {
            MainWindow.textBox_log.Text += string.Format("{0}: {1}\n", DateTime.Now.ToString(), log);
        }

        public static void textBox_log(string log, ChatMessage msg)
        {
            MainWindow.textBox_log.Text += string.Format("{0}: [{1}] [{2}] {3}\n", DateTime.Now.ToString(), msg.Chat.Topic, msg.Sender.FullName, log);
        }

        public static void textBox_log_clear()
        {
            MainWindow.textBox_log.Clear();
            MainWindow.textBox_skypeCommandsLog.Clear();
        } 
        #endregion

        #region textBox_skypeCommandsLog


        public static void textBox_skypeCommandsLog(string log, ChatMessage msg)
        {
            MainWindow.textBox_skypeCommandsLog.Text =
                string.Format("{0}: [{1}] [{2}] {3}\n", DateTime.Now.ToString(), msg.Chat.Topic, msg.Sender.FullName, log) + Environment.NewLine + MainWindow.textBox_skypeCommandsLog.Text;
        }
		 
	#endregion
        
        public static void statusbar1_log(string text)
        {
            MainWindow.textblock_StatusBar1.Text = text;
        }

        public static void statusbar2_log(string text)
        {
            MainWindow.textblock_StatusBar2.Text = text;
        }

        public static void textBox_badChatUpdateLog(string log)
        {
            MainWindow.textBox_badChatUpdateLog.Text =
                log + Environment.NewLine + MainWindow.textBox_badChatUpdateLog.Text;
        }
    }
}
