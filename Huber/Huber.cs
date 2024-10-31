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
using System.Reflection.Emit;
using PalmSens.Fitting.Models.Circuits.Elements;
using Org.BouncyCastle.Crypto.Paddings;
using System.IO;




namespace MeasureConsole
{
    public class Huber : IHuber
    {
        byte[] message = new byte[20];
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
        string huberResponseString = "[S01G";
        static int huberResponseReadPos = 0;
        static SerialPort _serialPort;
        private string _portName = "";
        private int defaultT;
        private short istWert;  //
        private short sollWert;      //real setPoint used by Huber
        private int setPoint = -273; //desired setPoint, when there is no communication failure they should be =
        private char  mode;
        private Thread reader;
        private Mutex readerBusyMutex = new Mutex();
        private IEventAggregator _ea;
        private Timer tmr = null;
        private static IContainer Container = Factory.Container;
        public Huber(IEventAggregator ea)
        {
            _ea = ea;
            _serialPort = new SerialPort();
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.ReadTimeout  = 500;
            _serialPort.WriteTimeout = 500;
            reader = new Thread(read);
            reader.IsBackground = true;
            reader.Start();
            _ea.GetEvent<HuberTChangeEvent>().Subscribe(OnMessageReceived);
        }
        private void OnMessageReceived(int t)
        {

        }
        public char status()
        {
            return mode;
        }
        private void readTemperature(object args)
        {
            if (setPoint!=-273)
            {
                sendCmd($"[M01G0D**{setPoint,4:X4}");
            }
            else
                sendCmd("[M01G0D******");  //if setpoint has not been set
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

        public short readSetPoint()
        {
            return sollWert;
        }
        public void start()
        {
            sendCmd("[M01G0DC*****");
        }
        public void stop()
        {
            sendCmd("[M01G0DO*****");
        }

        public void setTemperature(int temp)//in hundreds of a degree without dec point
        {
            if(temp<0 || temp > 4000)
            {
                Logger.WriteLine("Huber temperature should be in range 0..40C");
                return;
            }
            setPoint = temp;
            sendCmd($"[M01G0D**{temp,4:X4}");
        }

        private string calculateCRC(string cmd)
        {
            
            byte crc = 0;
            foreach(var element in cmd)
            {
                crc += (byte)element;
            }
            return $"{cmd}{crc,2:X2}\r\n";
        }

        private void sendCmd(string cmd)
        {
            if (_serialPort.IsOpen == false)
            {
                Logger.WriteLine("Huber chiller is not connected");
            }
            try
            {
                string cmdWCrc = calculateCRC(cmd);
                readerBusyMutex.WaitOne(1000);
                _serialPort.WriteLine(cmdWCrc);
                //Console.WriteLine($"Huber cmd {cmdWCrc}");
                readerBusyMutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex.Message);
                if (!(ex is System.TimeoutException))
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
        private static byte[] HexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
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
                    int expected_bytes = 23; //expected answer [S01G15O0FE7009A4C504E7\r, at least 23
                    if (_serialPort.BytesToRead >= expected_bytes)
                    {
                        try
                        {
                            readerBusyMutex.WaitOne(2000);
                            string line = _serialPort.ReadTo("\r");
                            //Console.Write(line);
                            short setPoint = 0;
                            int packageHeaderIndex = line.IndexOf("[S01G15");
                            if (packageHeaderIndex != -1)
                            {
                                string tString  = line.Substring(packageHeaderIndex + 13, 4); //Inner Istwert
                                string setPointString = line.Substring(packageHeaderIndex + 9, 4);
                                mode = line.ElementAt(packageHeaderIndex + 7);
                            try
                            {
                                byte[] byteArrayT = HexStringToByteArray(tString);
                                byte[] byteArraySetPoint = HexStringToByteArray(setPointString);
                                Array.Reverse(byteArrayT); Array.Reverse(byteArraySetPoint);
                                istWert  = BitConverter.ToInt16(byteArrayT, 0);
                                sollWert = BitConverter.ToInt16(byteArraySetPoint, 0);
                                //Console.WriteLine($"Inner t: {istWert}, setpoint: {sollWert}, mode: {mode}");
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteLine($"cannot parse Huber temperature package {tString}");
                            }
                            }
                            else
                            {
                                Console.WriteLine("nothing found");
                            }
                            _ea.GetEvent<HuberTChangeEvent>().Publish(istWert);

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
                        //Console.WriteLine("Huber timeout");
                        }
                        finally
                        {
                            readerBusyMutex.ReleaseMutex();
                        }
                    }
                }
            }
        }
    }
    public class HuberTChangeEvent : PubSubEvent<int>
    {

    }
}
