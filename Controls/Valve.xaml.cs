using Autofac;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MeasureConsole.Bootstrap;
using MeasureConsole.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MeasureConsole.Controls
{
    public partial class Valve : SceneControl
    {
        private bool _state = false;
        private IContainer _container;
        public  override string  ToString()
        {
            return $"Valve label: {Label}, channel: {Channel}, start: {Start}, length: {Length}";
        }
        public int Start { get; set; } = 0;
        public int Length { get; set; } = 0;    
        public int Channel { get; set; } = 0;
        public string Label
        {
            get
            {
                return controLabel.Content.ToString();
            }
            set
            {
                controLabel.Content = value;
            }
        }
        public  bool State { get
            {
                return _state;
            }
            set {
                _state = value;
                updateIcon();
            } }
       
        public override void OnAttributesReadHandler()
        {
            int result = 0;
            if (Attributes.ContainsKey("label"))
                Label = Attributes["label"];
            if (Attributes.ContainsKey("start"))
                if (int.TryParse(Attributes["start"], out result))
                    Start = result;            
            if (Attributes.ContainsKey("length"))
                if (int.TryParse(Attributes["length"], out result))
                    Length = result;
            if (Attributes.ContainsKey("channel"))
                if (int.TryParse(Attributes["channel"], out result))
                    Channel = result;            
            base.OnAttributesReadHandler();
        }

        public Valve()
        {
            InitializeComponent();
            _container = Factory.Container;
            Width = 100;
            Height = 70;
        }

        public override void Update(string package)
        {
            try
            {
                string porta = package.Substring(Start, Length);
                int iPorta = Convert.ToInt32(porta, 16);
                State = ((iPorta & (1 << Channel)) == 0) ? false : true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Valve parsing error: " + ex.Message);
                Console.WriteLine(package);                
            }            
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
