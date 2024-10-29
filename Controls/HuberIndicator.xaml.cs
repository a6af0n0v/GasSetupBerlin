using MeasureConsole.Scene;
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
        public HuberIndicator()
        {
            InitializeComponent();
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
    }
}
