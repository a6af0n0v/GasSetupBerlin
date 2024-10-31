
using System;
using System.IO.Ports;
using System.Windows;
using System.Threading;
using Prism.Events;
using System.Runtime.Remoting.Channels;
using static MeasureConsole.JSONNode.JSONMeasurement.DatasetNode.Values;
using System.Linq;
using MeasureConsole.Bootstrap;
using MeasureConsole.Scene;
using Autofac;
using MeasureConsole.Controls;

namespace MeasureConsole
{
    public class Arduino : IArduino
    {
        static SerialPort _serialPort;
        private string _portName = "";
        private double _temperature = 0;
        private Thread reader;
        public string HandshakeString { get; set; } = "$";
        public double Temperature
        {
            get
            {
                return _temperature;
            }
            private set
            {
                _temperature = value;
            }
        }
        public double SHTTemperature
        {
            get; private set;
        }
        public double Humidity
        {
            get; set;
        }
        public double SHTHumidity
        {
            get; private set;
        }
        public double Pressure
        {
            get; set;
        }

        public string PortName
        {
            set
            {
                try
                {
                    _serialPort.PortName = value;
                    _portName = value;
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(ex.Message);
                }
                
            }
            get
            {
                return _portName;
            }
        }
        public bool isOpen
        {
            get {
                return _serialPort.IsOpen;
            }
        }
        private IEventAggregator _ea;
        private Timer tmr;
        public void close()
        {
            if(_serialPort.IsOpen)
                _serialPort.Close();
        }
        public void init()
        {
            _serialPort.WriteLine(HandshakeString);
            Console.WriteLine(HandshakeString);
            Logger.WriteLine("System Initialized. All valves closed. MFCs are fully closed. PID is off.");
            Logger.writeCSVTableHeaders();
        }

        public Arduino(IEventAggregator ea)
        {
            _ea = ea;
            _serialPort = new SerialPort();
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.DtrEnable = true;
            //_serialPort.RtsEnable = true;
            reader = new Thread(read);
            reader.IsBackground = true;
            reader.Start();
            _ea.GetEvent<SuccessfulReadEvent>().Subscribe(OnMessageReceived);
        }

        private void OnMessageReceived(string package)
        {
            //Logger.WriteLine($"{package.package_number} - {package.porta}");
        }

        public void open()
        {
            if (PortName == "")
                throw new SystemException("First seet the port name");
            try
            {
                _serialPort.Open();
                System.Threading.Thread.Sleep(3000);
                _serialPort.DiscardInBuffer();
            }
            catch (SystemException ex)
            {
                Logger.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return; //throw ex;
            }
            Logger.WriteLine("Port is open");
            init();
        }

        ~Arduino()
        {
            reader.Abort();
            Logger.WriteLine("Closing com port...");
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
   
        public void setFlow(int channel, float flow)
        {
            
            var mainWnd = Factory.Container.Resolve<MainWindow>();
            float maxValue = 1;            
            foreach (var control in mainWnd.Scheme.Scheme.Controls)
            {
                if(control is mfc _mfc && _mfc.Channel == channel)
                {                    
                    mainWnd.Dispatcher.Invoke(() =>
                    {                        
                        if (_mfc.SCCM)
                        {
                            if (_mfc.ranges.ContainsKey(_mfc.Gas))
                            {
                                maxValue = (float)_mfc.ranges[_mfc.Gas] / 1000.0f;
                                if (flow > maxValue) 
                                {                                
                                    flow = maxValue;                                    
                                    Logger.WriteLine($"Such flow rate is not allowed for channel {channel}. The setpoint will be trimmed.");
                                }                                    
                            }
                           
                        }
                        else
                        {
                            if (flow > 1)
                            {                                
                                flow = 1;
                                Logger.WriteLine($"Such flow rate is not allowed for channel {channel}. The setpoint will be trimmed.");
                            }
                        }
                        _mfc.SollWert = flow;
                    });
                }
            }                                       
            var cmd = $"$b{channel}{flow}#";
            Console.WriteLine(cmd);
            _serialPort.WriteLine(cmd.Replace(',', '.'));
        }
        public void setMUX(byte addrValue, byte inputValue)
        {
            _serialPort.WriteLine($"$m{addrValue},{inputValue}#");
        }
        public void enableMUX(bool enable)
        {
            if (enable)
            {
                _serialPort.WriteLine("$e#");
            }
            else
            {
                _serialPort.WriteLine("$d#");
            }
        }
        public void openValve(int channel)
        {
            Console.WriteLine($"$f{channel}#");
            _serialPort.WriteLine($"$f{channel}#");
        }

        public void closeValve(int channel)
        {
            _serialPort.WriteLine($"$g{channel}#");
        }

        private void read()
        {
            while (true)
            {
                if (_serialPort.IsOpen)
                {
                    int expected_bytes = 36+3;
                    if (_serialPort.BytesToRead >= expected_bytes) 
                    {
                        //Console.WriteLine($"Bytes to read {_serialPort.BytesToRead}");
                        string message = "";
                        try
                        {
                            message = _serialPort.ReadLine();
                            int pckStart = message.LastIndexOf('$');
                            if (pckStart != -1)
                            {
                                int pckEnd = message.LastIndexOf('#');
                                if (pckEnd != -1)
                                {
                                    string extra = message.Substring(pckEnd + 1);
                                    if (extra.Length > 1)
                                        Console.WriteLine($"extra info: {extra}");
                                    string pckg = message.Substring(pckStart + 1, pckEnd - pckStart-1);
                                    //$FD 01 91B1 801E 7FFF 9256 231D 088A#
                                    if (pckg.Length == expected_bytes-1)
                                        _ea.GetEvent<SuccessfulReadEvent>().Publish(pckg);
                                    else
                                        Console.WriteLine(pckg);
                                }                                
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLine(ex.Message);
                            cleanInBuffer();
                        }
                    }
                }
            }
        }

        private class WaitTempArgs
        {
            public double temp;
            public double range;
        }

        private void checkTemp(object args)
        {
            if (Math.Abs(this.Temperature - ((WaitTempArgs)args).temp) <= ((WaitTempArgs)args).range)
            {
                Controls.JSList.IsPreviousStatemenComplete = true;
                tmr.Dispose();
            }
        }

        public void waitForTemp(double temp, double range)
        {
            Controls.JSList.IsPreviousStatemenComplete = false;
            WaitTempArgs args = new WaitTempArgs();
            args.temp = temp;
            args.range = range;
            tmr = new System.Threading.Timer(checkTemp, args, 500, 500);
        }


        private void cleanInBuffer()
        {
            try
            {
                _serialPort.DiscardInBuffer();
                Logger.WriteLine("InBuffer discarded");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        public void sendCustomCmd(string cmd)
        {
            try
            {
                _serialPort.DiscardInBuffer();
            }
            catch(SystemException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _serialPort.WriteLine(cmd);
            
        }

        public void setProportionalValve(int channel, float value)
        {
            if(channel<0 || channel>1)
            {
                Logger.WriteLine("setProportionalValve. Argument 'channel' is out of range");
                return;
            }
            if (value < 0 || value > 100)
            {
                Logger.WriteLine("setProportionalValve. Argument 'value' is out of range");
                return;
            }
            float setpoint = 5 * value / 100;
            //Console.WriteLine($"$v{channel}{setpoint}#");
            var cmd = $"$v{channel}{setpoint}#";
            _serialPort.WriteLine(cmd.Replace(',','.'));
        }

        public void setHumidity(int value)
        {
            _serialPort.WriteLine($"$h{value}#");
            Console.WriteLine($"$h{value}#");
            var mainWnd = Factory.Container.Resolve<MainWindow>();  
            foreach (var control in mainWnd.Scheme.Scheme.Controls)
            {
                if (control is mfc)
                {
                    var _mfc = (mfc)control;
                    if (_mfc.HumidityRegulator)
                    {
                        mainWnd.Dispatcher.Invoke(() =>
                        {
                            if (value > 0 && value < 10000)
                                _mfc.AutomaticMode = true;
                            else                            
                                _mfc.AutomaticMode = false;
                                
                            
                        });
                    }
                }
                else if (control is PID)
                {
                    var pid = (PID)control;
                    mainWnd.Dispatcher.Invoke(() =>
                    {
                        pid.TargetHumidity = value / 100;
                    });
                    
                }
            }
        }

        public void setPIDTerms(float c, float p, float i, float d)
        {
            var cmd = $"$k{c},{p},{i},{d}#";
            _serialPort.WriteLine(cmd.Replace(',','.'));
        }
    }

    public class SuccessfulReadEvent : PubSubEvent<string>
    {
    
    }
}
