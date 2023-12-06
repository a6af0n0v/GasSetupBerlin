using Autofac;
using Autofac.Core;
using MeasureConsole.Bootstrap;
using Prism.Events;
using System;

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MeasureConsole.Controls
{
    /// <summary>
    /// Interaction logic for Scheme.xaml
    /// </summary>
    public partial class Scheme : UserControl
    {

        private double t;
        private double rH;
        private double p;
        private static IContainer Container = Factory.Container;
        IEventAggregator _eventAggregator = null;
        public double Temperature
        {
            get { return t; }
            set 
            { 
                t = value;
                tbTemp.Text = $"{t:N1}";
            }
        }
        public double Humidity
        {
            get { return rH; }
            set
            {
                rH = value;
                tbHumidity.Text = $"{rH:N1}";
            }
        }
        public double Pressure
        {
            get { return p; }
            set
            {
                p = value;
                tbPressure.Text = $"{p:N1}";
            }
        }

        
        
        public Scheme()
        {
            InitializeComponent();
            
            _eventAggregator = Container.Resolve<IEventAggregator>();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void btnMethod_Click(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<MethodClickedEvent>().Publish();   
        }

        private void btnMeasure_Click(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<MeasureClickedEvent>().Publish();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<SaveClickedEvent>().Publish();
        }
    }
    public class MethodClickedEvent : PubSubEvent
    {

    }
    public class MeasureClickedEvent : PubSubEvent
    {

    }
    public class SaveClickedEvent : PubSubEvent
    {

    }

}
