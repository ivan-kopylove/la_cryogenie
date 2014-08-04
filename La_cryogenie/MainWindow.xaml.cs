using SKYPE4COMLib;
using System;
using System.Windows;

namespace La_cryogenie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeApp.createDirs();
            WindowsHandler.MainWindow = this;
            WindowsHandler.textBox_log_clear();
            subscribe_MessageStatusEventHandler();
        }

        private void menuitem_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void menuitem_Skype_Attach_Click(object sender, RoutedEventArgs e)
        {
                if (!SkypeSingleton.Instance.skype.Client.IsRunning)
                {
                    WindowsHandler.textBox_log("Не запущено ни одной копии Skype. Дальнейший процесс прерван");
                    return;
                }
                else
                {
                    WindowsHandler.textBox_log("Пытаюсь прикрепиться к копии Skype");
                    WindowsHandler.statusbar1_log("Пытаюсь прикрепиться к копии Skype");
                    try
                    {
                        SkypeSingleton.Instance.skype.Attach(8, true);
                        WindowsHandler.statusbar1_log(String.Format("OK: прикреплено к скайпу {0}.", SkypeSingleton.Instance.skype.CurrentUser.Handle));
                        WindowsHandler.textBox_log(String.Format("Прикреплено к скайпу {0}", SkypeSingleton.Instance.skype.CurrentUser.Handle));
                        WindowsHandler.textBox_log("Не забудте включить обработчик событий.");
                        return;
                    }
                    catch (Exception ex)
                    {
                        WindowsHandler.statusbar1_log(String.Format("Приложение сгенерировало исключение {0}.", ex.Message));
                        return;
                    }
                }
        }


        private void subscribe_MessageStatusEventHandler()
        {
            if (true)
            {
                SkypeSingleton.Instance.skype.MessageStatus += new _ISkypeEvents_MessageStatusEventHandler(SkypeSingleton.Instance.skype_MessageReceived);//подписываемся на событие
                WindowsHandler.statusbar1_log("OK: Подписано на обработчика событий.");
                WindowsHandler.textBox_log("Подписано на обработчика событий.");
            }
            else
            {
                SkypeSingleton.Instance.skype.MessageStatus -= new _ISkypeEvents_MessageStatusEventHandler(SkypeSingleton.Instance.skype_MessageReceived);//отписываемся от события
                WindowsHandler.statusbar1_log("OK: Отписано от обработчика событий.");
                WindowsHandler.textBox_log("Отписано от обработчика событий.");
            }
        }
    }
}
