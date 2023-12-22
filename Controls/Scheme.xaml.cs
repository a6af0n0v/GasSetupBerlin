using Autofac;
using Autofac.Core;
using MeasureConsole.Bootstrap;
using MeasureConsole.Dialogs;
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
        private double shtT;
        private double rH;
        private double shtRH;
        private bool _PIDEnabled;
        private double p;
        private static IContainer Container = Factory.Container;
        IEventAggregator _eventAggregator = null;
        public bool PIDEnabled
        {
            get { return _PIDEnabled; }
            set
            {
                _PIDEnabled = value;
                if (value)
                {
                    cProp1.lbValue.Content = "PID";
                    cProp2.lbValue.Content = "PID";
                }
                else
                {
                    cProp1.lbValue.Content = "-";
                    cProp2.lbValue.Content = "-";
                }
                if (value)
                    btnShowPIDDialog.Background = new SolidColorBrush(Color.FromRgb(200, 255, 200));
                else
                    btnShowPIDDialog.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            }
        }
        private double _PropValve1SetValue = 0;
        private double _PropValve2SetValue = 0;
        public double PIDTargetHumidity { get; set; }
        public double PropValve1SetValue {
            get {
                return _PropValve1SetValue;
            }
            set {
                _PropValve1SetValue = value;
                cProp1.lbValue.Content = _PropValve1SetValue.ToString("N2");
            } 
        }
        public double PropValve2SetValue {
            get
            {
                return _PropValve2SetValue;
            }
            set
            {
                _PropValve2SetValue = value;
                cProp2.lbValue.Content = _PropValve2SetValue.ToString("N2");
            }
        }
        public double Temperature
        {
            get { return t; }
            set
            {
                t = value;
                tbTemp.Text = $"{t:N1}";
            }
        }
        public double SHTTemperature
        {
            get { return shtT; }
            set
            {
                shtT = value;
                this.tbSHTTemp.Text = $"{shtT:N1}";
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
        public double SHTHumidity
        {
            get { return shtRH; }
            set
            {
                shtRH = value;
                tbSHTHumidity.Text = $"{shtRH:N1}";
            }
        }

        public Scheme()
        {
            InitializeComponent();
            PropValve1SetValue = 0;
            PropValve2SetValue = 0;
            PIDTargetHumidity = 0;
            PIDEnabled = false;
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

        private void ShowPIDDialogClicked(object sender, RoutedEventArgs e)
        {
            PIDDialog dlg = new PIDDialog();
            var pos = Mouse.GetPosition(null);
            dlg.Left = pos.X;
            dlg.Top = pos.Y;
            dlg.Value = Convert.ToInt16(PIDTargetHumidity);
            dlg.ShowDialog();
            var container = Factory.Container;
            var arduino = container.Resolve<IArduino>();
            if (dlg.result == System.Windows.Forms.DialogResult.OK)
            {
                if(arduino.isOpen == false)
                {
                    Logger.WriteLine("Arduino is not connected");
                    return;
                }
                PIDTargetHumidity = dlg.Value;
                arduino.setHumidity(dlg.Value*1000);
                if (dlg.Value >= 100 || dlg.Value <= 0)
                {
                    PIDEnabled = false;
                }
                else
                {
                    PIDEnabled = true;
                }
            }

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
