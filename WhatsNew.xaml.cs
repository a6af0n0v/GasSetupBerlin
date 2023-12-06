using Autofac;
using MeasureConsole.Bootstrap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MeasureConsole
{
    /// <summary>
    /// Interaction logic for WhatsNew.xaml
    /// </summary>
    public partial class WhatsNew : Window
    {
        public int Version = 52;
        public WhatsNew()
        {
            InitializeComponent();
            var fs = new FileStream("whatsnew.html", FileMode.Open, FileAccess.Read);
            
            wbBrowser.NavigateToStream(fs);
            
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(cbDontShowAgain.IsChecked==true)
            {
                var settings = Factory.Container.Resolve<Properties.Settings>();
                settings.Version = Version;
                settings.Save();
            }
        }
    }
}
