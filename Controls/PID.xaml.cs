using Autofac;
using MeasureConsole.Bootstrap;
using MeasureConsole.Dialogs;
using MeasureConsole.Scene;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
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
using System.Xml;
using System.Xml.Schema;

namespace MeasureConsole.Controls
{

    public partial class PID : SceneControl
    {
        public string Kc { get; set; } = "0";
        public string Kp { get; set; } = "0.00005";
        public string Kd { get; set; } = "-0.00001";
        public string Ki { get; set; } = "0.0000001";
        private double _targetHumidity = 0;

        public double TargetHumidity
        {
            get
            {
                return _targetHumidity;
            }
            set
            {
                _targetHumidity = value;
                if (_targetHumidity > 100)
                {
                    lbTargetValue.Content = "-%";
                }else
                    lbTargetValue.Content = _targetHumidity.ToString("N1") + "%";
            }
        }
        public PID()
        {
            InitializeComponent();
        }

        public string Label { get; set; }
        
        public override void Update(string package)
        {

        }
        public override string ToString()
        {
            return $"PID Control";
        }

        public override string CSVValue
        {
            get
            {
                return ";";
            }
        }

        public override void OnAttributesReadHandler()
        {            
            float result = 0;
         
            if (Attributes.ContainsKey("kc"))                
                Kc = Attributes["kc"];
            if(Attributes.ContainsKey("kd"))
                Kd = Attributes["kd"];
            if (Attributes.ContainsKey("ki"))
                Ki = Attributes["ki"];
            if (Attributes.ContainsKey("kp"))
                Kp = Attributes["kp"];
            base.OnAttributesReadHandler();
        }

        private void btnShowPIDDialog_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PIDDialog();
            var pos = Mouse.GetPosition(null);
            dlg.Left = pos.X;
            dlg.Top = pos.Y;
            dlg.ShowDialog();
            if ((dlg.result == System.Windows.Forms.DialogResult.OK))
            {
                var arduino = Factory.Container.Resolve<IArduino>();
                if (arduino.isOpen)
                {
                    arduino.setHumidity(dlg.Value*100);
                }
                else
                {
                    Logger.WriteLine("Can't set value. Arduino is not connected.");
                }
            }
        }
    }
}
