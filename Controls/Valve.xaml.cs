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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MeasureConsole.Controls
{
    /// <summary>
    /// Interaction logic for Valve.xaml
    /// </summary>
    public partial class Valve : UserControl
    {
        private bool _state = false;
        private IContainer _container;

        public  int Channel { get; set; }

        public  bool State { get
            {
                return _state;
            }
            set {
                _state = value;
                updateIcon();
            } }

        public Valve()
        {
            InitializeComponent();
            _container = Factory.Container;
        }


        private void updateIcon()
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            if (_state)
                src.UriSource = new Uri("pack://application:,,/icons/valve_on.png");
            else
            {
                src.UriSource = new Uri("pack://application:,,/icons/valve_off.png");
            }
            src.DecodePixelHeight = 200;
            src.EndInit();
            img.Source = src;
            
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            IArduino arduino = _container.Resolve<IArduino>();
            if (arduino.isOpen == false)
                return;
            State = !State;
            
            if (State)
                arduino.openValve(Channel);
            else
                arduino.closeValve(Channel);

        }
    }
}
