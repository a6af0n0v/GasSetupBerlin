using MeasureConsole.Scene;
using PalmSens.Units;
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

    public partial class SceneControl : UserControl, ISceneControl
    {
        public SceneControl()
        {

        }
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public XmlSchema GetSchema() => null;

        public virtual void OnAttributesReadHandler()
        {
            int result = 0;
            if (Attributes.ContainsKey("x"))
                if (int.TryParse(Attributes["x"], out result))
                    Canvas.SetLeft(this, result);
            if (Attributes.ContainsKey("y"))
                if (int.TryParse(Attributes["y"], out result))
                    Canvas.SetTop(this, result);
            if (Attributes.ContainsKey("width"))
                if (int.TryParse(Attributes["width"], out result))
                    Width = result;
        }
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                string key = reader.Name;
                switch (key)
                {
                    case "label":
                    case "x":
                    case "y":
                    case "width":
                    case "height":
                    case "channel":
                    case "address":
                    case "start":
                    case "isMFC":
                    case "scale":
                    case "mul":
                    case "div":
                    case "length":
                    case "bit":
                    case "heartbeat":
                    case "offset":
                    case "kc":
                    case "ki":
                    case "kp":
                    case "kd":
                    case "gas":
                    case "debug":
                    case "showgastype":
                    case "showaddress":
                    case "type":
                    case "humidityregulator":
                        var value = reader.ReadElementContentAsString();
                        Attributes.Add(key, value);
                        break;                    
                    default:
                        reader.Skip();
                        break;
                }
            }
            OnAttributesReadHandler();
            reader.ReadEndElement();
        }

        public virtual void Update(string package)
        {
            
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var attrib in Attributes)
            {
                writer.WriteElementString(attrib.Key, attrib.Value);
            }

        }
    }
}
