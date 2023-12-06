using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using MeasureConsole.Bootstrap;

namespace MeasureConsole
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            OpenSplashScreen();
            StartApplication();
            
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void StartApplication()
        {
            
        }

        private void OpenSplashScreen()
        {
            var container = Factory.Initialize();
            var mw = Factory.Container.Resolve<MainWindow>();
            var splash = Factory.Container.Resolve<Splash>();
            splash.Show();
            mw.WindowState = WindowState.Minimized;
            mw.Show();
        }

        public void CloseSplashScreen()
        {
            //destroy splash
            //Console.WriteLine("CloseSplash triggered");
            var splash = Factory.Container.Resolve<Splash>();
            splash.Close();
            var mw = Factory.Container.Resolve<MainWindow>();
            mw.WindowState = WindowState.Maximized;
            
        }
    }
}
