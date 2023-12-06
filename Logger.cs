using System;
using System.IO;
using MeasureConsole.Bootstrap;
using System.Text.Json;
using System.Collections.Generic;
using System.Windows.Documents;
using Autofac;

namespace MeasureConsole
{

    class JSONNode
    {
        public class JSONMeasurement
        {
            public class DatasetNode
            {
                public class Values
                {
                    public class Value
                    {
                        public double v { get; set; }
                        public int c { get; set; }
                        public int s { get; set; }
                    }
                    public class UNIT
                    {
                        
                        public string type { get; set; }
                        public string s { get; set; }
                        public string q { get; set; }
                        public string a { get; set; }
                        
                    }
                    public string type { get; set; }
                    public int arraytype { get; set; }
                    public string description { get; set; }
                    public UNIT unit { get; set; }
                    public IList<Value> datavalues { get; set; }
                }
                public string type { get; set; }
                public IList<Values> values { get; set; }
            }
            public string title { get; set; }
            public long timestamp { get; set; }
            public long utctimestamp { get; set; }
            public int deviceused { get; set; }
            public string deviceserial { get; set; }
            public string devicefw { get; set; }
            public string type { get; set; }
            public DatasetNode dataset { get; set; }
            public string method { get; set; }
            //public IList<string> curves;
            //public IList<string> eisdatalist;
        }
        public string type { get; set; }
        public string coreversion { get; set; }
        public string methodformeasurement { get; set; }
        public IList<JSONMeasurement> measurements { get; set; }

    };
    public class Logger
    {
        
        private static MainWindow _mainWindow;
        public static string processParametersToSaveInCSV { get; private set; }
        private static int linesInLogFile = 0;
        private static int linesInCSVFile = 0;
        private static int currentLogChapterNumber = 0;
        private static int currentCSVChapterNumber = 0;
        private static IContainer Container = Factory.Container;
        

        public class Settings
        {
            public static string logFileName = /*"log.txt"; */DateTime.Now.ToString("dd-MM-yyyy");
            public static string CSVFileName = DateTime.Now.ToString("dd-MM-yyyy_HH_mm_ss");
        }

        public static void CleanLog(MainWindow mainWnd)
        {
            _mainWindow = mainWnd;
            var settings = Container.Resolve<Properties.Settings>();
            var path = Path.Combine(settings.LogFolder, $"{Settings.logFileName}_{currentLogChapterNumber}.txt");
            using (var f = File.Open(path, FileMode.Create))
            {

            }
            path = Path.Combine(settings.CSVFolder, $"{Settings.CSVFileName}_{currentCSVChapterNumber}.csv");
            using (var f = File.Open(path, FileMode.Create))
            using (var writer = new  StreamWriter(f))
            {
                writer.WriteLine(settings.CSVColumnNames);
            }
        }
        public static void PSSessionToCSV(string pssessionFileName, string output="tmp.csv")
        {
            var json = File.ReadAllText(pssessionFileName);
            var lastIndex = json.LastIndexOf("}");
            json = json.Substring(0,lastIndex+1);
            //Console.WriteLine("JSON OBJECT");
            //Console.WriteLine(json);
            JSONNode topNode = JsonSerializer.Deserialize<JSONNode>(json);
            /*foreach(var mnt in topNode.measurements)
            {
                //Console.WriteLine(mnt.title);
                foreach (var value in mnt.dataset.values)
                {
                    Console.WriteLine(value.description);
                    foreach (var datavalue in value.datavalues)
                    {
                        //Console.WriteLine($"{datavalue.v} - {datavalue.c} - {datavalue.s}");
                    }
                }
            }*/
            var settings = Container.Resolve<Properties.Settings>();
            var path = Path.Combine(settings.DataFolder, output);
            using (var f = File.Open(path, FileMode.Create))
            using (var writer = new StreamWriter(f))
            {
                writer.WriteLine($"Frequency; Phase; Idc; Z; ZRe; ZIm");
                    for (int i = 0; i < topNode.measurements[0].dataset.values[0].datavalues.Count;i++)
                    {
                        var Idc         = topNode.measurements[0].dataset.values[0].datavalues[i].v;
                        var potential   = topNode.measurements[0].dataset.values[1].datavalues[i].v;
                        var time        = topNode.measurements[0].dataset.values[2].datavalues[i].v;
                        var Frequency   = topNode.measurements[0].dataset.values[3].datavalues[i].v;
                        var ZRe         = topNode.measurements[0].dataset.values[4].datavalues[i].v;
                        var ZIm         = topNode.measurements[0].dataset.values[5].datavalues[i].v;
                        var Z           = topNode.measurements[0].dataset.values[6].datavalues[i].v;
                        var Phase       = topNode.measurements[0].dataset.values[7].datavalues[i].v;
                        var Iac         = topNode.measurements[0].dataset.values[8].datavalues[i].v;
                        var nPointsAC   = topNode.measurements[0].dataset.values[9].datavalues[i].v;
                        var realtintac  = topNode.measurements[0].dataset.values[10].datavalues[i].v;
                        var ymean       = topNode.measurements[0].dataset.values[11].datavalues[i].v;
                        var debugtext   = topNode.measurements[0].dataset.values[12].datavalues[i].v;
                        var Y           = topNode.measurements[0].dataset.values[13].datavalues[i].v;
                        var YRe         = topNode.measurements[0].dataset.values[14].datavalues[i].v;
                        var YIm         = topNode.measurements[0].dataset.values[15].datavalues[i].v;

                        writer.WriteLine($"{Frequency}; {Phase}; {Idc}; {Z}; {ZRe}; {ZIm}");
                    }
            }

        }

        public static void LogToCSV(Package package)
        {
            var settings = Container.Resolve<Properties.Settings>();
            var path = Path.Combine(settings.CSVFolder, $"{Settings.CSVFileName}_{currentCSVChapterNumber}.csv");
            using (var f = File.Open(path, FileMode.Append))
            using (var writer = new StreamWriter(f))
            {
                var MFC1_Flow = package.mfc1;//((double)package.adc0 * 132 / 1024).ToString("N1");
                var MFC2_Flow = package.mfc2;//((double)package.adc1 * 132 / 1024).ToString("N1");
                
                double temperature = (double)package.temperature/100;
                var v0 = ((package.porta & (1 << settings.Valve1IO)) == 0) ? 0 : 1;
                var v1 = ((package.porta & (1 << settings.Valve2IO)) == 0) ? 0 : 1;
                var v2 = ((package.porta & (1 << settings.Valve3IO)) == 0) ? 0 : 1;
                var v3 = ((package.porta & (1 << settings.Valve4IO)) == 0) ? 0 : 1;
                var v4 = ((package.porta & (1 << settings.Valve5IO)) == 0) ? 0 : 1;
                var v5 = ((package.porta & (1 << settings.Valve6IO)) == 0) ? 0 : 1;
                var v6 = ((package.porta & (1 << settings.Valve7IO)) == 0) ? 0 : 1;

                double humidity = (double)package.humidity/1000;
                double pressure = (double)package.pressure/100;
                double huberT = (double)package.huber/100;


                writer.WriteLine($"{package.package_number}; {MFC1_Flow}; {MFC2_Flow};" +
                    $" {temperature:N1}; {humidity:N2}; {pressure:N3}; {v0}; {v1}; {v2}; {v3}; {v4}; {package.huber:N1}");
                processParametersToSaveInCSV = $"MFC1: {MFC1_Flow}; " +
                    $"MFC2: {MFC2_Flow}; t: {temperature:N1}; t_huber: {huberT:N4}; h: {humidity:N2}; p: {pressure:N3};";
                linesInCSVFile++;
                if (linesInCSVFile > settings.MaxNumberOfLinesInCSV)
                {
                    linesInCSVFile = 0;
                    currentCSVChapterNumber++;
                    var new_path = Path.Combine(settings.CSVFolder, $"{Settings.CSVFileName}_{currentCSVChapterNumber}.csv");
                    using (var f_new = File.Open(new_path, FileMode.Create))
                    using (var writer_new = new StreamWriter(f_new))
                    {
                        writer_new.WriteLine(settings.CSVColumnNames);
                    }
                }
            }            
        }

        public static void WriteLine(string format, params object[] args)
        {
            
            var settings = Container.Resolve<Properties.Settings>();
            string now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            format = $"[{now}] " +  format;
            Console.WriteLine(format, args);
            FileStream f = null;
            try
            {
                var path = Path.Combine(settings.LogFolder, $"{Settings.logFileName}_{currentLogChapterNumber}.txt");
                f = File.Open(path, FileMode.Append);
                using (var writer = new StreamWriter(f))
                {
                    writer.WriteLine(format, args);
                    linesInLogFile++;
                    if (linesInLogFile>settings.MaxNumberOfLinesInLog)
                    {
                        linesInLogFile = 0;
                        currentLogChapterNumber++; 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Logger.WriteLine {ex.Message}");
                return;
            }
            finally
            {
                f?.Dispose();
            }
            
            try
            {
                _mainWindow.Dispatcher.Invoke(() =>
                {
                    _mainWindow.FlowDoc.Blocks.Add(new Paragraph(new Run(string.Format(format, args))));                    
                    if (_mainWindow.FlowDoc.Blocks.Count>settings.MaxNumberOfMsgInStatusBar)
                    {
                        _mainWindow.FlowDoc.Blocks.Remove(_mainWindow.FlowDoc.Blocks.FirstBlock);
                    }                    
                    _mainWindow.rtbStatusBar.ScrollToEnd();
                });
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        private static System.Threading.Timer tmr;

        public static void Cleanup()
        {
            tmr.Dispose();
        }

        private static void OnDelay(object notused)
        {   
            tmr.Dispose();
            Controls.JSList.IsPreviousStatemenComplete = true;
        }

        public static void Wait(int ms)
        {
            Controls.JSList.IsPreviousStatemenComplete = false;
            tmr = new System.Threading.Timer(OnDelay, null, ms, ms);
        }



    }

    public class DataDump
    {
        // Writes an additional line with the given value as E = value V at the bottom of the given file
        public static void DumpPotentialinFile(double potential, string path)
        {
            using (var f = File.Open(path, FileMode.Append))
            using (var writer = new StreamWriter(f))
            {
                writer.WriteLine("");
                writer.WriteLine($"E={potential} V");
            }
            Controls.JSList.IsPreviousStatemenComplete = true;
        }

    }
    public class MethodCreation
    {
        // For chronoamperometry measurements. Follow the usual naming conventions to keep track. If the path_new_method doesn't exist yet,
        // it is created by changing only the value for the potential in the file basefile. This copy is stored in path_new_method.
        public static void ChangePotentialinCA(string path_new_method, string basefile, double potential)
        {
            if (File.Exists(path_new_method))
            {
                Logger.WriteLine("Method already exists.");
                return;
            }
            else
            {
                using (StreamReader read = new StreamReader(basefile))
                {
                    string line;
                    Boolean potential_found = false;
                    while ((line = read.ReadLine()) != null)
                    {
                        //Logger.WriteLine(line);
                        using (var f = File.Open(path_new_method, FileMode.Append))
                        using (var writer = new StreamWriter(f))
                        {
                            if (potential_found == true)
                            {
                                writer.WriteLine($"E={potential}");
                                potential_found = false;
                            }
                            else
                            {
                                if (line == "#Time method parameters")
                                {
                                    writer.WriteLine(line);
                                    potential_found = true;
                                }
                                else
                                {
                                    writer.WriteLine(line);
                                }
                            }
                        }
                    }
                }
            }
            Controls.JSList.IsPreviousStatemenComplete = true;
        }

        // For multistep chronoamperometry measurements. Changes the 2nd specified potential. Follow the usual naming conventions to keep track. If the path_new_method doesn't exist yet,
        // it is created by changing only the value for the potential in the file basefile. This copy is stored in path_new_method.
        public static void ChangePotentialinCAmultistep(string path_new_method, string basefile, double potential)
        {
            //Logger.WriteLine(path_new_method);
            if (File.Exists(path_new_method))
            {
                Logger.WriteLine("Method already exists.");
                return;
            }
            else
            {
                using (StreamReader read = new StreamReader(basefile))
                {
                    string line;
                    Boolean potential_found = false;
                    while ((line = read.ReadLine()) != null)
                    {
                        //Logger.WriteLine(line);
                        using (var f = File.Open(path_new_method, FileMode.Append))
                        using (var writer = new StreamWriter(f))
                        {
                            if (potential_found == true)
                            {
                                line = line.Replace("|", ",");
                                //Logger.WriteLine(line);
                                String[] line_split = line.Split(',');
                                //Logger.WriteLine(line_split[0]);
                                string new_line = line_split[0] + "|" + $"{potential}";
                                //Logger.WriteLine(new_line);
                                writer.WriteLine(new_line);
                                potential_found = false;
                            }
                            else
                            {
                                if (line.IndexOf("N_CYCLES") != -1)
                                {
                                    writer.WriteLine(line);
                                    potential_found = true;
                                }
                                else
                                {
                                    writer.WriteLine(line);
                                }
                            }
                        }
                    }
                }
            }
            Controls.JSList.IsPreviousStatemenComplete = true;
        }
    }
}
