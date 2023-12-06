using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for LED.xaml
    /// </summary>
    public partial class LED : UserControl
    {
        bool _state = false;
        private Timer tmr;

        public bool State 
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                if (_state)
                    src.UriSource = new Uri("pack://application:,,/icons/LED_ON.png");
                else
                {
                    src.UriSource = new Uri("pack://application:,,/icons/LED_OFF.png");
                }
                src.DecodePixelHeight = 200;
                src.EndInit();
                iMain.Source = src;
                if(_state!=false)
                    tmr = new System.Threading.Timer(OnDelay, null, 250, 250);
            }
        }

        private void OnDelay(object state)
        {
            tmr.Dispose();
            try
            {
                Dispatcher.Invoke(() =>
                {
                    State = false;
                });
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                
            }    
        }

        public LED()
        {
            InitializeComponent();
        }
    }
}
