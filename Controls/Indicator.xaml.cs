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

    public partial class Indicator : SceneControl
    {
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
            return $"Indicator label: {Label}, start: {Start}, length: {Length}";
        }
        public string ValueType { set; get; } = "int";
        public int Start { get; set; }
        public int Length { get; set; }
        public int Mul { set; get; } = 1;
        public int Div { set; get; } = 1;

        public override void Update(string package)
        {
            try
            {
                string sValue = package.Substring(Start, Length);
                //Console.WriteLine(package);
                //Console.WriteLine(sValue);
                if (ValueType == "int")
                {
                    tbValue.Text = ((double)(Convert.ToInt32(sValue, 16) * Mul) / Div).ToString("N1");
                }else if (ValueType == "string") {
                    tbValue.Text = sValue;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Indicator parsing error: " + ex.Message);
                Console.WriteLine(package);                
            }
        }
        public Indicator()
        {
            InitializeComponent();
        }                
        
        public override void OnAttributesReadHandler()
        {
            int result = 0;
            if (Attributes.ContainsKey("start"))
                if (int.TryParse(Attributes["start"], out result))
                    Start = result;
            if (Attributes.ContainsKey("length"))
                if (int.TryParse(Attributes["length"], out result))
                    Length = result;
            if (Attributes.ContainsKey("mul"))
                if (int.TryParse(Attributes["mul"], out result))
                    Mul = result;
            if (Attributes.ContainsKey("div"))
                if (int.TryParse(Attributes["div"], out result))
                    Div = result;
            if (Attributes.ContainsKey("type"))
                ValueType = Attributes["type"];
            if (Attributes.ContainsKey("label"))
                Label = Attributes["label"];
            base.OnAttributesReadHandler();
        }
    }
}
