using EmStatConsoleExample;
using Newtonsoft.Json;
using PalmSens.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeasureConsole.Demonstrators
{
    internal class Demonstrator : IDemonstrator
    {

        private Dictionary<string, DemonstratorDescriptor> demonstrators = new Dictionary<string, DemonstratorDescriptor>();

        public void Close(string comName)
        {
            Logger.WriteLine($"Closing port {comName}");
            Controls.JSList.IsPreviousStatemenComplete = false;
            if (demonstrators.ContainsKey(comName))
            {
                DemonstratorDescriptor _demonstrator;
                demonstrators.TryGetValue(comName, out _demonstrator);
                _demonstrator.portHandle.Close();
                demonstrators.Remove(comName);

            }
            else
            {
                Logger.WriteLine("Nothing to close, the port was not open");
            }

            Controls.JSList.IsPreviousStatemenComplete = true;
            Controls.JSList.mre.Reset();

        }

        private bool aValidDemonstrator(SerialPort portHandle, out string serialNumber)
        {
            serialNumber = "";
            try
            {
                portHandle.WriteLine("t\n");
                string picoline = portHandle.ReadLine();
                if (picoline.Contains("pico"))
                {
                    portHandle.WriteLine("G05\n");
                    var prefix = portHandle.ReadExisting();
                    serialNumber    = portHandle.ReadLine();
                    Logger.WriteLine($"This is a valid demonstrator sn. {serialNumber}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Cannot grab serial number of the demonstrator. Wrong port?");
                Logger.WriteLine(ex.Message);
                return false;
            }

            return false;
        }

        private void CloseAll()
        {
            foreach(string portName in demonstrators.Keys)
            {
                try
                {
                    DemonstratorDescriptor _demonstrator;
                    if(demonstrators.TryGetValue(portName, out _demonstrator))
                    {
                        _demonstrator.portHandle.Close();
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public DemonstratorDescriptor[] Discover()
        {
            Logger.WriteLine($"Discovering demonstrators, be patient");
            CloseAll();
            demonstrators.Clear();
            foreach (string portName in SerialPort.GetPortNames())
            {
                Open(portName);                
            }
            return demonstrators.Values.ToArray();
        }

        public void Open(string comName)
        {
            Logger.WriteLine($"Opening port {comName}");
            Controls.JSList.IsPreviousStatemenComplete = false;
            try
            {
                var _serialPort = new SerialPort();
                _serialPort.BaudRate = 230400;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;
                _serialPort.PortName = comName;
                _serialPort.Open();
                var _descriptor = new DemonstratorDescriptor();
                _descriptor.portHandle = _serialPort;
                _descriptor.comName = comName;
                string serialNumber = "";
                if (aValidDemonstrator(_serialPort, out serialNumber))
                {
                    _descriptor.demoId = serialNumber;
                }
                demonstrators.Add(comName, _descriptor);
                Logger.WriteLine($"Port Open");
            }
            catch (Exception e)
            {
                Logger.WriteLine($"Error during opening port {comName}. {e.Message}");
            }
            finally
            {
                Controls.JSList.IsPreviousStatemenComplete = true;
                Controls.JSList.mre.Reset();
            }
        }

        public void RunMethodScript(string comName, string methodScriptPath, string csv)
        {
            Logger.WriteLine($"Running Method Script");
            Controls.JSList.IsPreviousStatemenComplete = false;
            try
            {
                DemonstratorDescriptor _demonstrator;
                if (demonstrators.TryGetValue(comName, out _demonstrator))
                {
                    Logger.WriteLine($"Running method script: {methodScriptPath}");
                    MethodExample MethodExampleObj = new MethodExample();
                    MethodExample.SerialPortEsP = _demonstrator.portHandle;
                    MethodExampleObj.RunMethodScript(methodScriptPath);

                    // check presence of data for arranging it to write to file, trying to match psmethod data
                    string header = "";
                    bool TimeReadings_present = false;
                    bool VoltageReadings_present = false;
                    bool CurrentReadings_present = false;
                    bool EIS_present = false;
                    bool Cell_potential_present = false;

                    if (MethodExampleObj.TimeReadings.Any())
                    {
                        header = String.Concat(header, "Time; ");
                        TimeReadings_present = true;
                    }
                    if (MethodExampleObj.VoltageReadings.Any())
                    {
                        header = String.Concat(header, "Voltage; ");
                        VoltageReadings_present = true;
                    }
                    if (MethodExampleObj.CurrentReadings.Any())
                    {
                        header = String.Concat(header, "Current; ");
                        CurrentReadings_present = true;
                    }
                    if (MethodExampleObj.CellpotentialReadings.Any())
                    {
                        Cell_potential_present = true;
                    }
                    if (MethodExampleObj.FrequencyReadings.Any())
                    {
                        header = String.Concat(header, "Frequency; ");
                        EIS_present = true;
                    }
                    if (MethodExampleObj.PhaseReadings.Any())
                    {
                        header = String.Concat(header, "Phase; ");
                    }
                    if (MethodExampleObj.IdcReadings.Any())
                    {
                        header = String.Concat(header, "Idc; ");
                    }
                    if (MethodExampleObj.ZReadings.Any())
                    {
                        header = String.Concat(header, "Z; ");
                    }
                    if (MethodExampleObj.ZReReadings.Any())
                    {
                        header = String.Concat(header, "ZRe; ");
                    }
                    if (MethodExampleObj.ZImReadings.Any())
                    {
                        header = String.Concat(header, "ZIm");
                    }

                    //Logger.WriteLine(header);

                    if (csv != "")
                    {
                        using (var f = File.Open(csv, FileMode.Append))
                        using (var writer = new StreamWriter(f))
                        {
                            string now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                            writer.WriteLine($"Date and time: ; {now}");
                            writer.WriteLine(String.Concat(Logger.processParametersToSaveInCSV_demonstrator_part, $" t_sht: {MethodExampleObj.temperature:N2}; h_sht: {MethodExampleObj.humidity:N2};"));
                            writer.WriteLine(header);
                            if (VoltageReadings_present && CurrentReadings_present && TimeReadings_present)
                            {
                                Logger.WriteLine($"Triple Output detected");
                                for (var i = 0; i < MethodExampleObj.TimeReadings.Count; i++)
                                {
                                    writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.VoltageReadings[i]}; {MethodExampleObj.CurrentReadings[i] * 1000000}; Current");
                                    if (Cell_potential_present)
                                    {
                                        writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.VoltageReadings[i]}; {MethodExampleObj.CellpotentialReadings[i]}; CEPotential");
                                    }
                                }
                            }
                            else if (TimeReadings_present && CurrentReadings_present)
                            {
                                for (var i = 0; i < MethodExampleObj.TimeReadings.Count; i++)
                                {
                                    writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.CurrentReadings[i] * 1000000}; Current");
                                    if (Cell_potential_present)
                                    {
                                        writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.CellpotentialReadings[i]}; CEPotential");
                                    }
                                }
                            }
                            else if (TimeReadings_present && VoltageReadings_present)
                            {
                                for (var i = 0; i < MethodExampleObj.TimeReadings.Count; i++)
                                {
                                    writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.VoltageReadings[i]};");
                                }
                            }                     
                            else if (VoltageReadings_present && CurrentReadings_present)
                            {
                                for (var i = 0; i < MethodExampleObj.VoltageReadings.Count; i++)
                                {
                                    writer.WriteLine($"{MethodExampleObj.VoltageReadings[i]}; {MethodExampleObj.CurrentReadings[i] * 1000000}; Current");
                                    if (Cell_potential_present)
                                    {
                                        writer.WriteLine($"{MethodExampleObj.VoltageReadings[i]}; {MethodExampleObj.CellpotentialReadings[i]}; CEPotential");
                                    }
                                }
                            }
                            else if (EIS_present)
                            {
                                for (var i = 0; i < MethodExampleObj.FrequencyReadings.Count; i++)
                                {
                                    Logger.WriteLine("i {i}");
                                    writer.WriteLine($"{MethodExampleObj.FrequencyReadings[i]}; {MethodExampleObj.PhaseReadings[i]}; {MethodExampleObj.IdcReadings[i]}; {MethodExampleObj.ZReadings[i]}; {MethodExampleObj.ZReReadings[i]}; {MethodExampleObj.ZImReadings[i]}");
                                }
                            }

                            //foreach (Reading reading in MethodExample.Readings)
                            //{
                                //if (reading.Time != none)
                                //writer.WriteLine($"{reading.Time};  {reading.Voltage}; {reading.Current}; {reading.Frequency};  {reading.Phase}; {reading.Idc}; {reading.Z};  {reading.ZRe}; {reading.ZIm}");
                   
                            //}
                        }
                    }
                }
                else
                {
                    Logger.WriteLine("Open demonstrator's com port first");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Running MethodScript failed.");
                Logger.WriteLine(ex.Message);
            }
            finally
            {
                Controls.JSList.IsPreviousStatemenComplete = true;
                Controls.JSList.mre.Reset();
            }
        }

        public void RunMethodScriptOriginal(string comName, string methodScriptPath, string csv)
        {
            Logger.WriteLine($"Running Method Script");
            Controls.JSList.IsPreviousStatemenComplete = false;
            try
            {
                DemonstratorDescriptor _demonstrator;
                if (demonstrators.TryGetValue(comName, out _demonstrator))
                {
                    Logger.WriteLine($"Running method script: {methodScriptPath}");
  
                    MethodExample.SerialPortEsP = _demonstrator.portHandle;
                    MethodExample MethodExampleObj = new MethodExample();
                    MethodExampleObj.RunMethodScript(methodScriptPath);


                    if (csv != "")
                    {
                        using (var f = File.Open(csv, FileMode.Append))
                        using (var writer = new StreamWriter(f))
                        {
                            string now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                            writer.WriteLine($"Date and time: ; {now}");

                            writer.WriteLine("Time; Voltage; Current; Frequency; Phase; Idc; Z; ZRe; ZIm;");
                            foreach (Reading reading in MethodExample.Readings)
                            {
                                //if (reading.Time != none)
                                writer.WriteLine($"{reading.Time};  {reading.Voltage}; {reading.Current}; {reading.Frequency};  {reading.Phase}; {reading.Idc}; {reading.Z};  {reading.ZRe}; {reading.ZIm}");

                            }
                        }
                    }
                }
                else
                {
                    Logger.WriteLine("Open demonstrator's com port first");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Running MethodScript failed.");
                Logger.WriteLine(ex.Message);
            }
            finally
            {
                Controls.JSList.IsPreviousStatemenComplete = true;
                Controls.JSList.mre.Reset();
            }
        }

        // when using info dump
        public void RunMethodScript(string dump_json, string comName, string methodScriptPath, string csv)
        {

            string jsonString = dump_json;
            var infoDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
            string dump = DumpInfoToString(infoDict);

            Logger.WriteLine($"Running Method Script");
            Controls.JSList.IsPreviousStatemenComplete = false;

            try
            {
                DemonstratorDescriptor _demonstrator;
                if (demonstrators.TryGetValue(comName, out _demonstrator))
                {
                    Logger.WriteLine($"Running method script: {methodScriptPath}");
                    MethodExample.SerialPortEsP = _demonstrator.portHandle;
                    MethodExample MethodExampleObj = new MethodExample();
                    MethodExampleObj.RunMethodScript(methodScriptPath);
                    

                    // check presence of data for arranging it to write to file, trying to match psmethod data
                    string header = "";
                    bool TimeReadings_present = false;
                    bool VoltageReadings_present = false;
                    bool CurrentReadings_present = false;
                    bool EIS_present = false;
                    bool Cell_potential_present = false;

                    if (MethodExampleObj.TimeReadings.Any())
                    {
                        header = String.Concat(header, "Time; ");
                        TimeReadings_present = true;
                    }
                    if (MethodExampleObj.VoltageReadings.Any())
                    {
                        header = String.Concat(header, "Voltage; ");
                        VoltageReadings_present = true;
                    }
                    if (MethodExampleObj.CurrentReadings.Any())
                    {
                        header = String.Concat(header, "Current; ");
                        CurrentReadings_present = true;
                    }
                    if (MethodExampleObj.CellpotentialReadings.Any())
                    {
                        Cell_potential_present = true;
                    }
                    if (MethodExampleObj.FrequencyReadings.Any())
                    {
                        header = String.Concat(header, "Frequency; ");
                        EIS_present = true;
                    }
                    if (MethodExampleObj.PhaseReadings.Any())
                    {
                        header = String.Concat(header, "Phase; ");
                    }
                    if (MethodExampleObj.IdcReadings.Any())
                    {
                        header = String.Concat(header, "Idc; ");
                    }
                    if (MethodExampleObj.ZReadings.Any())
                    {
                        header = String.Concat(header, "Z; ");
                    }
                    if (MethodExampleObj.ZReReadings.Any())
                    {
                        header = String.Concat(header, "ZRe; ");
                    }
                    if (MethodExampleObj.ZImReadings.Any())
                    {
                        header = String.Concat(header, "ZIm");
                    }

                    if (csv != "")
                    {
                        using (var f = File.Open(csv, FileMode.Append))
                        using (var writer = new StreamWriter(f))
                        {
                            string now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                            writer.WriteLine($"Date and time: ; {now}");
                            writer.WriteLine(String.Concat(Logger.processParametersToSaveInCSV_demonstrator_part, $" t_sht: {MethodExampleObj.temperature:N2}; h_sht: {MethodExampleObj.humidity:N2};"));
                            writer.WriteLine(dump);
                            writer.WriteLine(header);

                            if (VoltageReadings_present && CurrentReadings_present && TimeReadings_present)
                            {
                                for (var i = 0; i < MethodExampleObj.VoltageReadings.Count; i++)
                                {
                                    writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.VoltageReadings[i]}; {MethodExampleObj.CurrentReadings[i] * 1000000}; Current");
                                    if (Cell_potential_present)
                                    {
                                        writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.VoltageReadings[i]}; {MethodExampleObj.CellpotentialReadings[i]}; CEPotential");
                                    }
                                }
                            }
                            else if (TimeReadings_present && CurrentReadings_present)
                            {
                                for (var i = 0; i < MethodExampleObj.TimeReadings.Count; i++)
                                {
                                    writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.CurrentReadings[i] * 1000000}; Current");
                                    if (Cell_potential_present)
                                    {
                                        writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.CellpotentialReadings[i]}; CEPotential");
                                    }
                                }
                            }
                            else if (TimeReadings_present && VoltageReadings_present)
                            {
                                for (var i = 0; i < MethodExampleObj.TimeReadings.Count; i++)
                                {
                                    writer.WriteLine($"{MethodExampleObj.TimeReadings[i]}; {MethodExampleObj.VoltageReadings[i]};");
                                }
                            }
                            else if (VoltageReadings_present && CurrentReadings_present)
                            {
                                for (var i = 0; i < MethodExampleObj.VoltageReadings.Count; i++)
                                {
                                    writer.WriteLine($"{MethodExampleObj.VoltageReadings[i]}; {MethodExampleObj.CurrentReadings[i] * 1000000}; Current");
                                    if (Cell_potential_present)
                                    {
                                        writer.WriteLine($"{MethodExampleObj.VoltageReadings[i]}; {MethodExampleObj.CellpotentialReadings[i]}; CEPotential");
                                    }
                                }
                            }
                            else if (EIS_present)
                            {
                                for (var i = 0; i < MethodExampleObj.FrequencyReadings.Count; i++)
                                {

                                    writer.WriteLine($"{MethodExampleObj.FrequencyReadings[i]}; {MethodExampleObj.PhaseReadings[i]}; {MethodExampleObj.IdcReadings[i]}; {MethodExampleObj.ZReadings[i]}; {MethodExampleObj.ZReReadings[i]}; {MethodExampleObj.ZImReadings[i]}");
                                }
                            }

                        }
                    }
                }
                else
                {
                    Logger.WriteLine("Open demonstrator's com port first");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Running MethodScript failed.");
                Logger.WriteLine(ex.Message);
            }
            finally
            {
                Controls.JSList.IsPreviousStatemenComplete = true;
                Controls.JSList.mre.Reset();
            }
        }

        private String DumpInfoToString(Dictionary<string, string> dump)
        {
            String data = "";
            foreach (var pair in dump)
            {
                data += $"{pair.Key}: {pair.Value}; ";
            }
            return data;
        }
    }
}
