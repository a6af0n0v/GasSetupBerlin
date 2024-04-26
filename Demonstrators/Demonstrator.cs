using EmStatConsoleExample;
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
                    MethodExample.SerialPortEsP = _demonstrator.portHandle;
                    MethodExample.RunMethodScript(methodScriptPath);
                    if (csv != "")
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
    }
}
