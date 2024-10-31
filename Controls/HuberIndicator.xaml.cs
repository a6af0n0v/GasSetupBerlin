using Autofac;
using MeasureConsole.Bootstrap;
using MeasureConsole.Scene;
using Prism.Events;
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
using System.Xml.Serialization;

namespace MeasureConsole.Controls
{
    public partial class HuberIndicator : SceneControl
    {
        private static IContainer Container = Factory.Container;
        private int currentTemperature = -273;
        public int CurrentTemperature
        {
            set
            {
                currentTemperature = value;
                double t = (double)value / 100;
                tbHuberT.Text = t.ToString("N1");
            }
            get
            {
                return currentTemperature;
            }
        }

        public override string CSVValue
        {            
            get
            {
                return tbHuberT.Text;
            }
        }

        public HuberIndicator()
        {
            InitializeComponent();
            var _ea = Container.Resolve<IEventAggregator>();
            _ea.GetEvent<HuberTChangeEvent>().Subscribe(OnHuberTempertureChange);

        }

        public override string ToString()
        {
            return $"HuberIndicator";
        }

        public string Label { get; set; }        

        public void Update(string package)
        {

        }
       
        public override void OnAttributesReadHandler()
        {            
            if (Attributes.ContainsKey("label"))
                Label = Attributes["label"];
            base.OnAttributesReadHandler();
        }
        private void OnHuberTempertureChange(int temperature)
        {
            this.Dispatcher.Invoke(() =>
            {
                CurrentTemperature = temperature;
            });
        }
    }
}
