using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PalmSens;
using PalmSens.Data;
using PalmSens.Comm;
using PalmSens.Windows.Devices;
using PalmSens.Devices;
using PalmSens.Core.Simplified.Data;
using PalmSens.Core.Simplified.WPF;      
using PalmSens.Techniques;
using PalmSens.Analysis;


using Autofac;
using MeasureConsole.Bootstrap;
using System.IO;
using EmStatConsoleExample;
using System.IO.Ports;
using System.Windows.Documents;

namespace MeasureConsole
{
    public interface IPalmsense
    {
        void LoadMethod(string path);        
        void Measure(int channel=-1);  //Sync  
        void RunMethodScript(string path, string csv);
        (List<double> list_x, List<double> list_y) SaveMeasurements(string path, bool csv=false); 
        void Connect(Device device);
        void SetupMUX(bool[] channels);
        PSCommSimpleWPF PSCommSimpleWPF { set; }        
    }

    public class Palmsense : IPalmsense
    {
        private Device[] devices;
        private SimpleCurve _activeSimpleCurve = null;
        public PSCommSimpleWPF psCommSimpleWPF;
        private Method method = null;
        private Device _device;
        private SimpleMeasurement activeSimpleMeasurement = null;
        private Queue<KeyValuePair<double, double>> measurements;
        private SerialPort scriptPort;

        public PSCommSimpleWPF PSCommSimpleWPF
        {
            set
            {
                psCommSimpleWPF = value;
                psCommSimpleWPF.MeasurementStarted += measurementStarted;
                psCommSimpleWPF.SimpleCurveStartReceivingData += startRecievingData;
                psCommSimpleWPF.MeasurementEnded += measurementEnded;
            }
        }

        public Palmsense()
        {
            // Charts docs here: https://habr.com/ru/post/145343/
            //chart.ChartAreas.Add(new ChartArea("Default"));

            // Добавим линию, и назначим ее в ранее созданную область "Default"
            /*chart.Series.Add(new Series("Series1"));
            chart.Series["Series1"].ChartArea = "Default";
            chart.Series["Series1"].ChartType = SeriesChartType.Line;

            // добавим данные линии
            string[] axisXData = new string[] { "a", "b", "c" };
            double[] axisYData = new double[] { 0.1, 1.5, 1.9 };
            chart.Series["Series1"].Points.DataBindXY(axisXData, axisYData);*/
            PalmSens.Windows.CoreDependencies.Init();
            measurements = new Queue<KeyValuePair<double, double>>();
        }
        ~Palmsense()
        {
            try
            {
                psCommSimpleWPF.MeasurementStarted -= measurementStarted;
                psCommSimpleWPF.SimpleCurveStartReceivingData -= startRecievingData;
                psCommSimpleWPF.MeasurementEnded -= measurementEnded;
                psCommSimpleWPF.Disconnect();
                foreach (Device dev in devices)
                {
                    dev.Close();
                    Logger.WriteLine("Closing device");
                }
            }
            catch (NullReferenceException ex)
            {
                Logger.WriteLine("Palmsense. Null Reference Exception");
            }
        }

        private void activeSimpleCurve_NewDataAdded(object sender, PalmSens.Data.ArrayDataAddedEventArgs e)
        {
            int startIndex = e.StartIndex;
            int count = e.Count;
            SimpleCurve curve = (SimpleCurve)sender;
            //Logger.WriteLine("New Data Added");
            //for (int i = startIndex; i < curve.YAxisValues.Length; i++)
            for (int i = startIndex; i < startIndex+count; i++)
            {
                // Console.WriteLine($"x={curve.XAxisValues[i]:F2}|y={curve.YAxisValues[i]:F2}");
                double xValue = curve.XAxisValue(i);
                double yValue = curve.YAxisValue(i);
                measurements.Enqueue(new KeyValuePair<double, double>(xValue, yValue));
                //measurements.Enqueue(new KeyValuePair<double, double>(curve.XAxisValues[i], curve.YAxisValues[i]));
            }

            
        }
        private void _activeSimpleCurve_CurveFinished(object sender, EventArgs e)
        {
            Logger.WriteLine("Curve Finished");
            _activeSimpleCurve.CurveFinished -= _activeSimpleCurve_CurveFinished;
            _activeSimpleCurve.NewDataAdded -= activeSimpleCurve_NewDataAdded;
            Console.WriteLine($"active curve finised: {_activeSimpleCurve.IsFinished}");
        }


        public void startRecievingData(object sender, SimpleCurve activeSimpleCurve)
        {
            Logger.WriteLine("Start Receiving Data");
            _activeSimpleCurve = activeSimpleCurve;
            _activeSimpleCurve.NewDataAdded += activeSimpleCurve_NewDataAdded;
            _activeSimpleCurve.CurveFinished += _activeSimpleCurve_CurveFinished;
        }

        public void measurementStarted(object sender, EventArgs e)
        {
            Logger.WriteLine("Measurement started");

        }
        public void measurementEnded(object sender, EventArgs e)
        {
            Logger.WriteLine("Measurement ended ");
            Controls.JSList.IsPreviousStatemenComplete = true;
            Controls.JSList.mre.Reset();
        }
 

        public void LoadMethod(string path)
        {
            method = SimpleLoadSaveFunctions.LoadMethod(path);
            Logger.WriteLine($"Palmsense method loaded {path}");
            Controls.JSList.mre.Reset();
        }

         public void Measure(int channel = -1)
        {
            Controls.JSList.IsPreviousStatemenComplete = false;
            if (_device == null)
            {
                Logger.WriteLine("First connect to Palmsense");
                return;
            }
            if (method == null)
            {
                Logger.WriteLine("First load Palmsense method");
                return;
            }     

            try
            {
                /*var taskCompletionSource = new TaskCompletionSource<Exception>();
                EventHandler<Exception> measurementEndedHandler = new EventHandler<Exception>(
                    (object sender, Exception ex) =>
                    {
                        taskCompletionSource.SetResult(ex);
                    }
                    );
                psCommSimpleWPF.MeasurementEnded += measurementEndedHandler;*/
                activeSimpleMeasurement =  psCommSimpleWPF.Measure(method, channel);
                //taskCompletionSource.Task.Wait();
                //psCommSimpleWPF.MeasurementEnded -= measurementEndedHandler;
                //var exception = taskCompletionSource.Task.Result;
                Console.WriteLine($"Measure method result: {activeSimpleMeasurement == null}");
            }
            catch (NullReferenceException ex)
            {
                Controls.JSList.IsPreviousStatemenComplete = true;
                Controls.JSList.mre.Reset();
                Logger.WriteLine($"Error in palmsense.Measure: {ex.Message}");
            }
        }

        public (List<double> list_x, List<double> list_y) SaveMeasurements(string path, bool csv = false)
        {
            Controls.JSList.IsPreviousStatemenComplete = false;
            List<double> list_x = new List<double>();
            List<double> list_y = new List<double>();
            if (activeSimpleMeasurement == null)
            {
                Logger.WriteLine("Measure first");
                return (list_x, list_y);
            }
            if (csv == false)
            {
                SaveToPSSession(path);
                string pathinter = path;
                string pathout = pathinter.Replace(".pssession", ".csv");
                Logger.WriteLine($"Pathout set to {pathout}");
                Logger.PSSessionToCSV(path, pathout);
            }
            else
            {
                (list_x, list_y) = SaveToCSV(path);
            }
            Controls.JSList.IsPreviousStatemenComplete = true;
            Logger.WriteLine($"Data successfully saved to {path}");
            Controls.JSList.mre.Reset();
            return (list_x, list_y);
        }

        private (List<double> list_x, List<double> list_y) SaveToCSV(string path)
        {
            Controls.JSList.IsPreviousStatemenComplete = false;
            List<double> list_x = new List<double>();
            List<double> list_y = new List<double>();
            using (var f = File.Open(path, FileMode.Append))
            using (var writer = new StreamWriter(f))
            {
                string now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                writer.WriteLine($"Date and time: ; {now}");
                writer.WriteLine(Logger.processParametersToSaveInCSV);
                writer.WriteLine("V; uA");
                foreach (var pair in measurements)
                {
                    writer.WriteLine($"{pair.Key}; {pair.Value}");
                    list_x.Add(pair.Key);
                    list_y.Add(pair.Value);
                }
                measurements.Clear();
            }
            Controls.JSList.IsPreviousStatemenComplete = true;
            Controls.JSList.mre.Reset();
            return (list_x, list_y);
        }

        public void SetupMUX(bool[] channels)
        {
            method.MuxSett = new Method.MuxSettings(false)
            {
                CommonCERE = false,
                ConnSEWE = false,
                ConnectCERE = true,
                OCPMode = false,
                SwitchBoxOn = false,
                UnselWE = Method.MuxSettings.UnselWESetting.FLOAT
            };
            method.MuxMethod = MuxMethod.Sequentially;
            method.UseMuxChannel = new System.Collections.BitArray(channels);
        }

        private void SaveToPSSession(string path)
        {
            Controls.JSList.IsPreviousStatemenComplete = false;
            try
            {
                SimpleLoadSaveFunctions.SaveMeasurement(activeSimpleMeasurement, path);
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex.Message);
               
            }
            Controls.JSList.IsPreviousStatemenComplete = true;
            Controls.JSList.mre.Reset();
        }

        public void Connect(Device device)
        {
            Controls.JSList.IsPreviousStatemenComplete = false;
            try
            {
                psCommSimpleWPF.Connect(device);
                Logger.WriteLine("Device open");
            
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Cannot open device {ex.Message}");
            }
            _device = device;
            Controls.JSList.IsPreviousStatemenComplete = true;
            Controls.JSList.mre.Reset();
        }

       

        public void RunMethodScript(string path, string csv="")
        {
            Controls.JSList.IsPreviousStatemenComplete = false;
            Logger.WriteLine($"Running method script: {path}");
            try
            {
                MethodExample.RunMethodScript(path);
            }
            catch (Exception ex)
            {

                Logger.WriteLine("Error in RunMethodScript. Is script path correct?", ex.Message);
            }
            
            if (csv!="")
            {
                using (var f = File.Open(csv, FileMode.Append))
                using (var writer = new StreamWriter(f))
                {
                    string now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                    writer.WriteLine($"Date and time: ; {now}");
                    writer.WriteLine("index; V; I");
                    foreach (Reading reading in MethodExample.Readings)
                        writer.WriteLine($"{reading.index};  {reading.Voltage}; {reading.Current}");
                }
            }

            Controls.JSList.IsPreviousStatemenComplete = true;
            Controls.JSList.mre.Reset();
        }
    }
}
