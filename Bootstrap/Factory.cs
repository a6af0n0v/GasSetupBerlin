using Autofac;
using MeasureConsole.Controls;
using MeasureConsole.Demonstrators;
using MeasureConsole.Scene;
using MeasureConsole.Dialogs;
using PalmSens.Core.Simplified.WPF;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureConsole.Bootstrap
{
    public class Factory
    {
        private static IContainer _container = null;

        public static IContainer Container 
        {
            get
            {
                return _container;
            }
                
          private set
            {
                _container = value;
            }
        }

        public static IContainer Initialize()
        {
            if (_container!=null)
            {
                return _container;
            }
            var builder = new ContainerBuilder();
            builder.RegisterType<Arduino>().As<IArduino>().SingleInstance();
            builder.RegisterType<MainWindow>().As<IMainWindow>().SingleInstance();
            builder.RegisterType<MainWindow>().AsSelf().SingleInstance();
            //builder.RegisterType<Scene.Scene>().AsSelf().SingleInstance();            
            builder.RegisterType<HuberSetTDlg>().AsSelf();
            builder.RegisterType<Palmsense>().As<IPalmsense>().SingleInstance().WithParameter(new TypedParameter(typeof(PSCommSimpleWPF), "simpleWPF"));
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<Splash>().AsSelf().SingleInstance();
            builder.RegisterType<WhatsNew>().AsSelf();
            builder.RegisterType<HelpWindow>().AsSelf();
            builder.RegisterType<Demonstrator>().As<IDemonstrator>().SingleInstance();
            builder.RegisterType<Huber>().As<IHuber>().SingleInstance();
            builder.RegisterType<Properties.Settings>().AsSelf().SingleInstance();
            builder.RegisterType<Dialogs.SettingsWindow>().AsSelf();
            Container = builder.Build();
            return Container;
        }

    }
}
