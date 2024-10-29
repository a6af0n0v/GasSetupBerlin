
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MeasureConsole.Controls;
using System.Xml.Linq;
using System.Xml.Serialization;
using Jint.Runtime.Debugger;
using System.Xml.Schema;
using System.Xml;
using PalmSens.Units;
using System.Windows.Markup;
using Autofac;
using MeasureConsole.Bootstrap;
using MeasureConsole.Properties;


namespace MeasureConsole.Scene
{
    public class ChartArea
    {
        private int QueueSize = 10;
        public string Label { set; get; } = "Default";
        public string xTitle { set; get; } = "Ticks";
        public string yTitle { set; get; } = "";
        public string y2Title { set; get; } = "";
        public ChartArea(int queueSize) {
            QueueSize = queueSize;
        }
        public IList<ChartLine> Lines { get; set; } = new List<ChartLine>();
    }

    public class ChartLine
    {
        
        public ChartLine(int queueSize) {
            QueueSize = queueSize;
        }
        public string Label { get; set; } = "undefined";
        public int Start { get; set; } = 0;
        public int Length { get; set; } = 0;
        public string yAxis { get; set; } = "primary";
        public int Mul { get; set; } = 1;
        public int Div { get; set; } = 1;
        public int Offset { get; set; } = 0;
        private int QueueSize = 10;
        public Queue<double> Data { set; get; } = new Queue<double>();
        
        public void Update(string package)
        {            
            try
            {
                string sValue = package.Substring(Start, Length);
                double value = ((double)(Convert.ToInt32(sValue, 16) * Mul / Div + Offset));
                if (Data.Count >= QueueSize)
                {
                    Data.Dequeue();
                }
                Data.Enqueue(value);
            }
            catch (Exception ex){
                Console.WriteLine($"Update {Label} error: {ex.Message}");
            }                                     
        }
                
        public override string ToString()
        {
            return $"Chart line {Label}, start {Start}, length {Length}";
        }        
    };

    public class Scheme : IXmlSerializable
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Image { get; set; }
        public List<ISceneControl> Controls { get; set; } = new List<ISceneControl>();
        public List<ChartArea> ChartAreas { get; set; } = new List<ChartArea>();
        public List<mfc> Mfcs { get; set; } = new List<mfc>();
        public XmlSchema GetSchema() => null;
        private PID pid = null;
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            Name = reader.ReadElementContentAsString("Name", "");
            Version = reader.ReadElementContentAsString("Version", "");
            Image = reader.ReadElementContentAsString("Image", "");
            reader.ReadStartElement("Controls");
            while (reader.IsStartElement())
            {
                ISceneControl obj = null;
                switch (reader.Name)
                {
                    case "Valve":
                        obj = new Valve();
                        break;
                    case "LED":
                        obj = new LED();
                        break;
                    case "MFC":
                        obj = new mfc();
                        Mfcs.Add((mfc)obj);
                        break;
                    case "Huber":
                        obj = new HuberIndicator();
                        break;
                    case "PID":
                        obj = new PID();
                        pid = (PID)obj;
                        break;
                    case "Indicator":
                        obj = new Indicator();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
                if (obj != null)
                {
                    obj.ReadXml(reader);
                    Controls.Add(obj);
                }
            }
            reader.ReadEndElement(); // Controls
            reader.ReadStartElement("Charts");
            var settings = Factory.Container.Resolve<Properties.Settings>();
            while (reader.IsStartElement())
            {
                switch(reader.Name)
                {
                    case "Chart":
                        ChartArea area = new ChartArea(settings.MaxPointsOnChart);
                        reader.ReadStartElement();
                        while (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "label":
                                    area.Label = reader.ReadElementContentAsString();
                                    break;
                                case "xTitle":
                                    area.xTitle = reader.ReadElementContentAsString();
                                    break;
                                case "yTitle":
                                    area.yTitle = reader.ReadElementContentAsString();
                                    break;
                                case "y2Title":
                                    area.y2Title = reader.ReadElementContentAsString();
                                    break;
                                case "Line":
                                    var line = new ChartLine(settings.MaxPointsOnChart);
                                    reader.ReadStartElement();
                                    while (reader.NodeType != XmlNodeType.EndElement)
                                    {
                                        string key = reader.Name;
                                        switch (key)
                                        {
                                            case "label":
                                                line.Label = reader.ReadElementContentAsString();
                                                break;
                                            case "start":
                                                try
                                                {
                                                    line.Start = reader.ReadElementContentAsInt();
                                                }
                                                catch (Exception ex) { }
                                                break;
                                            case "length":
                                                try
                                                {
                                                    line.Length = reader.ReadElementContentAsInt();
                                                }
                                                catch (Exception ex) { }
                                                break;
                                            case "mul":
                                                try
                                                {
                                                    line.Mul = reader.ReadElementContentAsInt();
                                                }
                                                catch (Exception ex) { }
                                                break;
                                            case "div":
                                                try
                                                {
                                                    line.Div = reader.ReadElementContentAsInt();
                                                }
                                                catch (Exception ex) { }
                                                break;
                                            case "offset":
                                                try
                                                {
                                                    line.Offset = reader.ReadElementContentAsInt();
                                                }
                                                catch (Exception ex) { }
                                                break;
                                            case "yaxis":
                                                line.yAxis = reader.ReadElementContentAsString();
                                                break;
                                            default:
                                                reader.Skip();
                                                break;
                                        }
                                    }
                                    reader.ReadEndElement();
                                    area.Lines.Add(line);
                                    break;
                                default:
                                    reader.Skip();
                                    break;
                            }
                        }
                        ChartAreas.Add(area);                        
                        reader.ReadEndElement();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }           
            reader.ReadEndElement();
            reader.ReadEndElement(); // Scheme
            string handshake = $"${Mfcs.Count} ";
            foreach (var mfc in Mfcs)            
                handshake += $"{mfc.Gas} ";            
            if(pid!=null)
                handshake += $"{pid.Kc} {pid.Kp} {pid.Ki} {pid.Kd}";

            Factory.Container.Resolve<IArduino>().HandshakeString = handshake ;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Version", Version);
            writer.WriteElementString("Image", Image);
            writer.WriteStartElement("Controls");
            writer.WriteEndElement(); // Controls
        }
    }

    public partial class Scene : UserControl
    {
        public Scheme Scheme { get; set; } = new Scheme();
        public Scene()
        {
            InitializeComponent();
        }
        public void LoadSceneFromXml(string filePath)
        {
            var serializer = new XmlSerializer(typeof(Scheme));
            using (Stream s = File.Open(filePath, FileMode.Open))
            {
                var scheme = (Scheme)serializer.Deserialize(s);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                string absolutePath = System.IO.Path.GetFullPath(scheme.Image); // Resolve relative path
                Uri imageUri = new Uri(absolutePath, UriKind.Absolute);
                bitmap.UriSource = imageUri;
                bitmap.EndInit();
                iBackground.Source = bitmap;
                lSchemeName.Content = scheme.Name;
                foreach (var control in scheme.Controls)
                {
                    mainCanvas.Children.Add((UIElement)control);
                    //Console.WriteLine(control);
                }              
                Scheme = scheme;
            }
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            var settings = Factory.Container.Resolve<Settings>();            
            try
            {
                LoadSceneFromXml(settings.SceneFileName);
            }catch (Exception ex)
            {
                MessageBox.Show("Scene XML file was not found");                
            }
        }
    }
}
