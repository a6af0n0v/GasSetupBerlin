using MeasureConsole.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization;
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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MeasureConsole.Controls
{
    public partial class LED : SceneControl
    {
        bool _state = false;
        private Timer tmr;
        public string Label
        {
            get { return controLabel.Content.ToString(); }
            set
            {
                controLabel.Content = value;
            }
        }
        public override string ToString()
        {
            return $"LED label: {Label}, start: {Start}, length: {Length}";
        }
        public bool bHeartBeat { get; set; } = false;
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
                if (_state != false)
                    if(bHeartBeat)
                        tmr = new System.Threading.Timer(OnDelay, null, 250, 250);
            }
        }
        public override void Update(string package)
        {
            try
            {
                string sValue = package.Substring(Start, Length);                
                int iValue = Convert.ToInt32(sValue, 16);             
                State = (iValue & (1 << Bit)) == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in parsing LED value: " + ex.Message);                
            }
        }
        public string label { set; get; }
        public int Start { get; set; } = 0;
        public int Length { get; set; } = 0;
        public int Bit { get; set; } = 0;       
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
       

        public override void OnAttributesReadHandler()
        {
            int result = 0;
            bool bResult = false;
            if (Attributes.ContainsKey("label"))
                Label = Attributes["label"];
            if (Attributes.ContainsKey("start"))
                if (int.TryParse(Attributes["start"], out result))
                    Start = result;
            if (Attributes.ContainsKey("length"))
                if (int.TryParse(Attributes["length"], out result))
                    Length = result;
            if (Attributes.ContainsKey("bit"))
                if (int.TryParse(Attributes["bit"], out result))
                    Bit = result;
            if (Attributes.ContainsKey("heartbeat"))
                if (bool.TryParse(Attributes["heartbeat"], out bResult))
                    bHeartBeat = bResult;
            base.OnAttributesReadHandler();
        }

        public LED()
        {
            InitializeComponent();
        }
      
    }
}
