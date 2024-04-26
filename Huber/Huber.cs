using MeasureConsole.Bootstrap;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace MeasureConsole
{
    public class Huber : IHuber
    {
        
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
            get
            {
                return _serialPort.IsOpen;
            }
        }

        static SerialPort _serialPort;
        private string _portName = "";
        private int defaultT;
        private Thread reader;
        private IEventAggregator _ea;
        private Timer tmr = null;
        private static IContainer Container = Factory.Container;
        public Huber(IEventAggregator ea)
        {
            _ea = ea;
            _serialPort = new SerialPort();
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.ReadTimeout = 6000;
            _serialPort.WriteTimeout = 500;
            reader = new Thread(read);
            reader.IsBackground = true;
            reader.Start();
            _ea.GetEvent<HuberTChangeEvent>().Subscribe(OnMessageReceived);
        }
        private void OnMessageReceived(int t)
        {

        }
        public void status()
        {
            sendCmd("CA?\r\n");
        }
        private void readTemperature(object args)
        {
            sendCmd("TI?\r\n");
        }

        public void open()
        {
            if (PortName == "")
                throw new SystemException("First set the port name");
            try
            {
                _serialPort.Open();
            }
            catch (SystemException ex)
            {
                Logger.WriteLine(ex.Message);
                return; //throw ex;
            }
            Logger.WriteLine("Huber port is open");
            var settings = Container.Resolve<Properties.Settings>();
            int timerInterval = settings.HuberPollingInterval;
            defaultT = settings.huberFaultTValue;
            tmr = new Timer(readTemperature, null, timerInterval, timerInterval);
        }

        public string readSetPoint()
        {
            string setPoint = "";
            sendCmd("SP?\r\n");
            return setPoint;
        }
        public void start()
        {
            sendCmd("CA@ 00001\r\n");
        }
        public void stop()
        {
            sendCmd("CA@ 00000\r\n");
        }

        public void setTemperature(int temp)//in hundreds of a degree without dec point
        {
            if(temp<0 || temp > 4000)
            {
                Logger.WriteLine("Huber temperature should be in range 0..40C");
                return;
            }
            sendCmd($"SP@ {temp}\r\n");
        }

        private void sendCmd(string cmd)
        {
            if (_serialPort.IsOpen == false)
            {
                Logger.WriteLine("Huber chiller is not connected");
            }
            try
            {
                _serialPort.WriteLine(cmd);
                Console.WriteLine($"Huber cmd {cmd}");
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex.Message);
                throw ex;
            }
        }

        ~Huber()
        {
            if (tmr!=null) 
                tmr.Dispose();
            reader.Abort();
            Logger.WriteLine("Closing Huber com port...");
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        private void cleanInBuffer()
        {
            try
            {
                _serialPort.DiscardInBuffer();
                Logger.WriteLine("InBuffer (Huber) discarded");
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex.Message);
            }

        }

        private void read()
        {
            while (true)
            {
                if (_serialPort.IsOpen)
                {
                    //int expected_bytes = 3;
                    //if (_serialPort.BytesToRead >= expected_bytes)
                    //{
                        try
                        {
                            string message = _serialPort.ReadLine();
                            
                            if(message.Contains("TI"))
                            { //Huber replied with current internal temperature
                                int t = defaultT;
                                string t_str = message.Substring(3, message.Length - 3 -1);
                                try
                                {
                                    t = Convert.ToInt32(t_str);
                                }
                                catch(Exception ex)
                                {
                                    Logger.WriteLine($"cannot parse Huber temperature package {t_str}");
                                }
                                _ea.GetEvent<HuberTChangeEvent>().Publish(t);
                            }
                            Console.WriteLine(message);
                        }
                        catch (System.IO.IOException)
                        {
                        
                            Logger.WriteLine("Huber connection lost");
                            MainWindow main = Container.Resolve<MainWindow>();
                            main.setFlag(MainWindow.STATES.HUBER_CONNECTED, false);
                            if (tmr != null)
                            {
                                tmr.Dispose();
                                tmr = null;
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                                Logger.WriteLine(ex.Message);
                                cleanInBuffer();
                        }
                        catch(TimeoutException)
                        {
                        //it's ok
                        Console.WriteLine("Huber timeout");
                        }
                    //}
                }
            }
        }
    }
    public class HuberTChangeEvent : PubSubEvent<int>
    {

    }
}
