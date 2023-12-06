using Autofac;
using MeasureConsole.Bootstrap;
using System;
using System.Collections.Generic;
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

namespace MeasureConsole.Dialogs
{
    /// <summary>
    /// Interaction logic for HuberSetTDlg.xaml
    /// </summary>
    public partial class HuberSetTDlg : Window
    {
        public HuberSetTDlg()
        {
            InitializeComponent();
        }
        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            int valueToSet = Convert.ToInt32(sValue.Value * 100);
            IContainer Container = Factory.Container;
            var huber = Container.Resolve<IHuber>();
            huber.setTemperature(valueToSet);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
