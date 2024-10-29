
using Autofac;
using MeasureConsole.Bootstrap;
using MeasureConsole.Dialogs;
using MeasureConsole.Scene;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MeasureConsole.Controls
{
    public partial class mfc : SceneControl
    {
        private Dictionary<int, string> gases = new Dictionary<int, string> {
            { 13, "N2" },
            { 15, "O2" },
            { 7, "H2" },
            { 1, "He" },
            { 28, "CH4" },
            { 27, "N2O" },
            { 4, "Ar" }
        };
        public mfc()
        {
            InitializeComponent();
            Debug = false;
            ShowAddress = false;
            ShowGasType = false;
            Gas = 13;
            _sollWert = float.NaN;
         }
        private int channel = 0;
        private double scaledValue = 0;
        private bool _automaticMode = false;
        private int gas = 13;
        private float _value;
        private float _sollWert;
        private bool debug = false;
        private bool showAddress = false;
        private bool showGasType = false;
        public int Channel {
            get { return channel; }
            set {
                channel = value;
                lbAddr.Content = $"Ch {value}";          
            } 
        }
        public bool isMFC { get; set; } = true;
        public double Mul { get; set; } = 1;
        public double Div { get; set; } = 1;
        public double Offset { get; set; } = 0;
        public bool HumidityRegulator { get; set; } = false;
        public double MaxError { get; set; } = 1;  //1%
        public bool AutomaticMode { 
            set
            {
                _automaticMode = value;
                if (_automaticMode)                
                    lbSollWert.Visibility = Visibility.Hidden;                
                else
                    lbSollWert.Visibility = Visibility.Visible;
            }
            get
            {
                return _automaticMode;
            }
        }
        public int Gas
        {
            get
            {
                return gas;
            }
            set
            {
                gas = value;
                if (gases.ContainsKey(gas))
                {
                    lbGasType.Content = gases[gas] ;
                }
                
            }
        }
        public bool ShowAddress
        {
            get
            {
                return showAddress;
            }
            set
            {
                showAddress = value;
                if (value)
                    lbAddr.Visibility = Visibility.Visible;
                else
                    lbAddr.Visibility = Visibility.Collapsed;
            }
        }
        public bool ShowGasType
        {
            get
            {
                return showGasType;
            }
            set
            {
                showGasType = value;
                if (value) 
                    lbGasType.Visibility = Visibility.Visible;
                else 
                    lbGasType.Visibility = Visibility.Collapsed;
            }
        }
        public bool Debug
        {
            get { return debug; }
            set
            {
                debug = value;
                if(!debug)
                    lbRawValue.Visibility = Visibility.Collapsed;
                else
                    lbRawValue.Visibility = Visibility.Visible;
            }
        }
        public float  SollWert
        {
            get
            {
                return _sollWert;
            }
            set
            {
                _sollWert = value;                
                lbSollWert.Content = _sollWert.ToString("N1");
                updatePicture();
            }
        }
        private void updatePicture()
        {
            if (!float.IsNaN(_sollWert))
            {
                if (Math.Abs(_sollWert - scaledValue) > MaxError)
                {
                    iMFCerr.Visibility = Visibility.Visible;
                    iMFCok.Visibility = Visibility.Collapsed;
                    iMFC.Visibility = Visibility.Collapsed;
                }
                else
                {
                    iMFCerr.Visibility = Visibility.Collapsed;
                    iMFCok.Visibility = Visibility.Visible;
                    iMFC.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                iMFCerr.Visibility = Visibility.Collapsed;
                iMFCok.Visibility = Visibility.Collapsed;
                iMFC.Visibility = Visibility.Visible;
            }
        }
        public float   Value // in l/min
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                scaledValue = (((double)(_value * Mul) / Div + Offset));
                lbValue.Content = scaledValue.ToString("N1");
                updatePicture();                
            }
        }
        public  string  Label
        {
            get {
                return controLabel.Content.ToString();
            }
            set
            {
                controLabel.Content = value;
            }
        }      
        public int Start { get; set; } = 0;
        public int Length { get; set; } = 0;
        public override string ToString()
        {
            return $"MFC label: {Label}, channel: {Channel}, start: {Start}, length: {Length}";
        }
        public override void Update(string package)
        {
            //Console.WriteLine(package);           
            if (Length == 0) return;
            try
            {
                if (isMFC)
                {
                    Value = (float)Convert.ToUInt32(package.Substring(Start, Length),16);     
                    lbRawValue.Content = package.Substring(Start, Length);
                }
                else
                {
                    
                    string sValue = package.Substring(Start, Length);
                    //Console.WriteLine(sValue);
                    Value = Convert.ToInt32(sValue, 16);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MFC {Label} parsing error: " + ex.Message);
                Console.WriteLine($"start {Start} length {Length} " );
                /*Console.WriteLine(package.Substring(Start, Length));
                Console.WriteLine(package);*/                
            }
           
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var container = Factory.Container;
            var mainWnd = container.Resolve<MainWindow>();
            if (AutomaticMode)
            {
                Logger.WriteLine("PID activated. To control valves manually disable PID.");
                return;
            }
            MFCDialog dlg = new MFCDialog(isMFC);
            var pos = Mouse.GetPosition(null);
            dlg.Left    = pos.X;
            dlg.Top     = pos.Y;
            dlg.ShowDialog();
            var arduino = container.Resolve<IArduino>();
            {
                if ((dlg.result == System.Windows.Forms.DialogResult.OK) ||
                    (dlg.result == System.Windows.Forms.DialogResult.Abort))
                {
                    if (arduino.isOpen)
                    {
                        float setValue = 0;
                        if (dlg.result == System.Windows.Forms.DialogResult.Abort)
                            setValue = 0;
                        else
                        {
                            if (isMFC)
                            {
                                setValue = ((float)dlg.Value / 100);
                                arduino.setFlow(Channel, setValue);                                
                            }
                            else //proportional valve
                            {
                                setValue = ((float)dlg.Value);
                                arduino.setProportionalValve(Channel, setValue);                               
                            }
                        }
                        //Console.WriteLine($"value raw {setValue}");
                    }
                    else
                    {
                        Logger.WriteLine("Can't set value. Arduino is not connected.");
                    }
                }
            }
        }
      
        public override void OnAttributesReadHandler()
        {
            int result = 0;
            bool bResult = false;
            double fResult = 1;
            if (Attributes.ContainsKey("start"))
                if (int.TryParse(Attributes["start"], out result))
                    Start = result;
            if (Attributes.ContainsKey("length"))
                if (int.TryParse(Attributes["length"], out result))
                    Length = result;
            if (Attributes.ContainsKey("channel"))
                if (int.TryParse(Attributes["channel"], out result))
                    Channel = result;
            if (Attributes.ContainsKey("label"))
                Label = Attributes["label"];
            if (Attributes.ContainsKey("isMFC"))
                if (bool.TryParse(Attributes["isMFC"], out bResult))
                    isMFC = bResult;
            if (Attributes.ContainsKey("mul"))
                if (double.TryParse(Attributes["mul"], out fResult))
                    Mul = fResult;
            if (Attributes.ContainsKey("div"))
                if (double.TryParse(Attributes["div"], out fResult))
                    Div = fResult;
            if (Attributes.ContainsKey("offset"))
                if (double.TryParse(Attributes["offset"], out fResult))
                    Offset = fResult;
            if (Attributes.ContainsKey("gas"))
                if (int.TryParse(Attributes["gas"], out result))
                    Gas = result;
            if (Attributes.ContainsKey("debug"))
                if (bool.TryParse(Attributes["debug"], out bResult))
                    Debug = bResult;
            if (Attributes.ContainsKey("showaddress"))
                if (bool.TryParse(Attributes["showaddress"], out bResult))
                    ShowAddress = bResult;
            if (Attributes.ContainsKey("showgastype"))
                if (bool.TryParse(Attributes["showgastype"], out bResult))
                    ShowGasType = bResult;
            if (Attributes.ContainsKey("humidityregulator"))
                if (bool.TryParse(Attributes["humidityregulator"], out bResult))
                    HumidityRegulator = bResult;
            base.OnAttributesReadHandler();
        }
    }
}
