
using System;
using System.IO.Ports;
using System.Windows;
using System.Threading;
using Prism.Events;
using System.Runtime.Remoting.Channels;
using static MeasureConsole.JSONNode.JSONMeasurement.DatasetNode.Values;

namespace MeasureConsole
{
    public class Arduino : IArduino
    {
        static SerialPort _serialPort;
        private string _portName = "";
        private double _temperature = 0;
        private Thread reader;
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

        public void init()
        {
            //set mfc 0
            _serialPort.WriteLine($"$b10#");
            _serialPort.WriteLine($"$b20#");
            //set props 0
            _serialPort.WriteLine($"$v00#");
            _serialPort.WriteLine($"$v10#");
            //set pid off
            _serialPort.WriteLine($"$h0#");
            Logger.WriteLine("Arduino in default state: MFCs closed, target humidity 0%");
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
            reader = new Thread(read);
            reader.IsBackground = true;
            reader.Start();
            _ea.GetEvent<SuccessfulReadEvent>().Subscribe(OnMessageReceived);
        }

        private void OnMessageReceived(Package package)
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
            _serialPort.WriteLine($"$b{channel}{flow}#");
            Console.WriteLine($"$b{channel}{flow}#");
        }

        public void openValve(int channel)
        {
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
                    int expected_bytes = 56;
                    if (_serialPort.BytesToRead >= expected_bytes) 
                    {
                        //Console.WriteLine($"Bytes to read {_serialPort.BytesToRead}");
                        string message = "";
                        try
                        {
                            message = _serialPort.ReadLine();
                            //Console.WriteLine(message);
                            var package = new Package();                           
                            package.package_number = Convert.ToInt32(message.Substring(0, 2), 16);
                            package.porta = Convert.ToInt32(message.Substring(3, 2), 16);
                            byte[] mfc1_array = {   Convert.ToByte(message.Substring(12, 2), 16),
                                                    Convert.ToByte(message.Substring(10,2), 16),
                                                    Convert.ToByte(message.Substring(8,2), 16),
                                                    Convert.ToByte(message.Substring(6,2), 16)};
                            byte[] mfc2_array = {   Convert.ToByte(message.Substring(21, 2), 16),
                                                    Convert.ToByte(message.Substring(19,2), 16),
                                                    Convert.ToByte(message.Substring(17,2), 16),
                                                    Convert.ToByte(message.Substring(15,2), 16)};
                            package.mfc1 = System.BitConverter.ToSingle(mfc1_array,0);
                            package.mfc2 = System.BitConverter.ToSingle(mfc2_array, 0);
                            //Console.WriteLine($"{package.mfc1 } = {package.mfc2 }");                           
                            package.temperature = Convert.ToInt32(message.Substring(24, 4), 16);
                            Temperature = package.temperature / 100;

                            package.humidity = Convert.ToInt32(message.Substring(29, 8), 16);
                            Humidity = package.humidity / 1000;
                            
                            package.pressure = Convert.ToInt32(message.Substring(38, 8), 16);
                            Pressure = package.pressure / 100;
                            //Logger.WriteLine("t - {}", package.temperature);
                            package.shtHumidity = Convert.ToInt32(message.Substring(47, 4), 16);
                            SHTHumidity = package.shtHumidity / 1000;
                            package.shtTemperature = Convert.ToInt32(message.Substring(52, 4), 16);
                            SHTTemperature = package.shtTemperature / 100;
                            _ea.GetEvent<SuccessfulReadEvent>().Publish(package);
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
            _serialPort.WriteLine($"$v{channel}{setpoint}#");
        }

        public void setHumidity(int value)
        {
            _serialPort.WriteLine($"$h{value}#");
        }

        public void setPIDTerms(float c, float p, float i, float d)
        {
            _serialPort.WriteLine($"$k{c},{p},{i},{d}#");
        }
    }

    public class SuccessfulReadEvent : PubSubEvent<Package>
    {
    
    }
}
