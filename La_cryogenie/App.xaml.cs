using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NLog;

namespace La_cryogenie
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Logger log;
        //www.codeproject.com/Articles/90866/Unhandled-Exception-Handler-For-WPF-Applications
        private void Application_DispatcherUnhandledException(object sender,
                       System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //if (this.DoHandle)
            //{
            //Handling the exception within the UnhandledException handler.
            log.Trace("DispatcherUnhandledExceptionEventArgs: {0} ||| TargetSite: {1}", e.Exception.Message, e.Exception.TargetSite.ToString());
            //MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = false;
            //}
            //else
            //{
            //    //If you do not set e.Handled to true, the application will close due to crash.
            //    MessageBox.Show("Application is going to close! ", "Uncaught Exception");
            //    e.Handled = false;
            //}
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            AppDomain.CurrentDomain.UnhandledException +=
                         new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            #region Nlog
            try
            {
                log = LogManager.GetCurrentClassLogger();

                log.Trace("Version: {0}", Environment.Version.ToString());
                log.Trace("OS: {0}", Environment.OSVersion.ToString());

                NLog.Targets.FileTarget tar = (NLog.Targets.FileTarget)LogManager.Configuration.FindTargetByName("run_log");
                tar.DeleteOldFileOnStartup = false;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Ошибка работы с логом!\n" + ex.Message);
                Environment.Exit(0);
            }

            #endregion
        }



        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            log.Trace("UnhandledExceptionEventArgs: {0} ||| TargetSite: {1}", ex.Message, ex.TargetSite.ToString());
            
            //MessageBox.Show(ex.Message, "Uncaught Thread Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
