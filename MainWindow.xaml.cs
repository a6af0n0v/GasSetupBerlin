﻿using Autofac;
using MeasureConsole.Bootstrap;
using MeasureConsole.Controls;
using PalmSens.Core.Simplified.WPF;
using PalmSens.Devices;
using Prism.Events;
using System;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Controls.Ribbon;
using MeasureConsole.Dialogs;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using PalmSens.Units;
using MeasureConsole.Scene;
// Charts docs here: https://habr.com/ru/post/145343/
namespace MeasureConsole
{

    public interface IMainWindow
    {
        PSCommSimpleWPF pSCommSimpleWPF { get; }
    }

    public partial class MainWindow : Window, IMainWindow
    {
        IArduino arduino;
        IPalmsense palmsense;
        IHuber huber;
        private int currentHuberT = -27300;        
        private IEventAggregator _eventAggregator;
        private static IContainer Container = Factory.Container;
        private static FlowDocument flowDoc = new FlowDocument();        
        Queue<int> axisXData = new Queue<int>();        
        int tick = 0;
        public FlowDocument FlowDoc
        {
            get
            {
                return flowDoc;
            }
        }

        public PSCommSimpleWPF pSCommSimpleWPF
        {
            get
            {
                return psCommSimpleWPF;
            }
        }

        #region StateMachine
        public enum STATES { PALMSESNSE_ENABLED = 1, ARDUINO_CONNECTED, HUBER_CONNECTED };
        private int flags = 0;
        private Device[] devices;

        public void setFlag(STATES flag, bool value)
        {
            if (value)
                flags |= (1 << (int)flag);
            else
                flags ^= (1 << (int)flag);
            //Logger.WriteLine($"Flag set {flags:X}");
            setState();
        }
        public bool isFlagSet(int flag)
        {
            if ((flags & (1 << flag)) == 0)
                return false;
            else
                return true;
        }
        private void setState()
        {
            bool _arduino_state = ((flags & (1 << (int)STATES.ARDUINO_CONNECTED)) != 0);
            this.Dispatcher.Invoke(() =>
            {                
                btnSendCustomCmd.IsEnabled = _arduino_state;
            });
            bool _huber_state = ((flags & (1 << (int)STATES.HUBER_CONNECTED)) != 0);
            this.Dispatcher.Invoke(() =>
            {
                btnStartHuber.IsEnabled = _huber_state;
                btnStopHuber.IsEnabled = _huber_state;
                btnSetHuberT.IsEnabled = _huber_state;
            });
        }

        #endregion
        public void OnSettingsUpdated()
        {
            var settings = Container.Resolve<Properties.Settings>();

            if (settings.AutoConnect)
            {
                btnConnectLastDevices_Click(null, null);
            }

        }

        public MainWindow()
        {
            InitializeComponent();
            rtbStatusBar.Document = flowDoc;
            Logger.CleanLog(this);
            setState();
            arduino = Container.Resolve<IArduino>();
            palmsense = Container.Resolve<IPalmsense>();
            huber = Container.Resolve<IHuber>();
            palmsense.PSCommSimpleWPF = psCommSimpleWPF;
            _eventAggregator = Container.Resolve<IEventAggregator>();
            _eventAggregator.GetEvent<SuccessfulReadEvent>().Subscribe(OnSuccessfulRead);
            
            Loaded += OnMainWindowLoaded;
            Console.WriteLine("MainWindow Loaded event handler charged");
            UpdateApllicationMenu();           
            foreach(var area in Scheme.Scheme.ChartAreas)
            {                
                chart.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea(area.Label));
                chart.ChartAreas[area.Label].AxisX.Title = area.xTitle;
                chart.ChartAreas[area.Label].AxisY.Title = area.yTitle;
                chart.ChartAreas[area.Label].AxisY2.Title = area.y2Title;
                foreach (var line in area.Lines)
                {
                    chart.Series.Add(new Series(line.Label));
                    chart.Series[line.Label].ChartType = SeriesChartType.Line;
                    chart.Series[line.Label].ChartArea = "Default";
                    chart.Series[line.Label].BorderWidth = 3;
                    chart.Series[line.Label].ChartArea = area.Label;
                    if (line.yAxis == "secondary")
                    {
                        chart.Series[line.Label].YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
                    }                    
                }
                chart.Legends.Add(area.Label);
            }
            
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("MainWindow loaded");
            App app = (App) App.Current;
            app.CloseSplashScreen();
            var dlg = Container.Resolve<WhatsNew>();
            var settings = Container.Resolve<Properties.Settings>();
            if (dlg.Version != settings.Version)
            {
                dlg.Show();
            }
        }
        public void Plot()
        {
            var settings = Factory.Container.Resolve<Properties.Settings>();
            if (axisXData.Count >= settings.MaxPointsOnChart)
            {
                axisXData.Dequeue();               
            }
            axisXData.Enqueue(tick);
            tick++;
            foreach(Scene.ChartArea area in Scheme.Scheme.ChartAreas)
            foreach(ChartLine line in area.Lines)
            {
                try
                {
                    chart.Series[line.Label].Points.DataBindXY(axisXData, line.Data);
                }catch(Exception){ }
            }            
        }        

       
        
        private void OnSuccessfulRead(string package)
        {
            this.Dispatcher.Invoke(() =>
            {               
                foreach (ISceneControl control in Scheme.Scheme.Controls)
                {                    
                    control.Update(package);
                }
                foreach(Scene.ChartArea area in Scheme.Scheme.ChartAreas)
                {
                    foreach (ChartLine line in area.Lines)
                    {
                        line.Update(package);
                    }                    
                }
                Plot();
                Logger.LogToCSV(package);
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            arduino.close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            devices = psCommSimpleWPF.ConnectedDevices;
            cbPalmSensDevices.Items.Clear();
            foreach (Device dev in devices)
            {
                cbPalmSensDevices.Items.Add(dev.ToString());
            }

            Logger.WriteLine("Listing available serial ports:");
            catComPort.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())
            {
                Logger.WriteLine("{0}", s);
                System.Windows.Controls.Ribbon.RibbonGalleryItem item = new System.Windows.Controls.Ribbon.RibbonGalleryItem();
                item.Content = s;
                catComPort.Items.Add(item);
            }
            catHuberPort.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())
            {
                var item = new RibbonGalleryItem();
                item.Content = s;
                catHuberPort.Items.Add(item);
            }

            var settings = Container.Resolve<Properties.Settings>();
            try
            {               
                galComPort.SelectedValue = settings.ArduinoPort;
                galHuberPort.SelectedValue = settings.HuberPort;
                cbPalmSensDevices.SelectedIndex = settings.PalmsensePortIndex;
                
            }
            catch (Exception)
            {
                Logger.WriteLine("Can't restore previous values of com ports");
            }
        }
        private void onApplicationMenuItemClick(object sender, RoutedEventArgs e)
        {
            RibbonApplicationMenuItem item = (RibbonApplicationMenuItem)sender;
            try
            {
                LoadJSScript(item.Header.ToString());
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Cannot open JS file \n {ex}");
            }
            
        }

        private void UpdateApllicationMenu()
        {
            var settings = Factory.Container.Resolve<Properties.Settings>();
            menuApplication.Items.Clear();
            foreach (var file_name in settings.LastFiles)
            {
                var item = new RibbonApplicationMenuItem();
                item.Header = file_name;
                item.Click += onApplicationMenuItemClick;
                menuApplication.Items.Add(item);
            }
        }
        private void btnScriptOpen_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "js files (*.js) |*.js";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    
                    var settings = Factory.Container.Resolve<Properties.Settings>();
                    if (settings.LastFiles.Contains(dlg.FileName) == false)
                    {
                        settings.LastFiles.Add(dlg.FileName);
                        if (settings.LastFiles.Count >= settings.LastFilesMaxCount)
                        {
                            settings.LastFiles.RemoveAt(0);
                        }
                        settings.Save();
                        UpdateApllicationMenu();
                    }
                    LoadJSScript(dlg.FileName);
                }
            }
            

        }
        private void LoadJSScript(string js_file_name)
        {
            jsList.Text = File.ReadAllText(js_file_name);
            //Console.WriteLine(jsList.Text);
        }

        private void btnScriptSave_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "js files (*.js) |*.js";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var fs = new FileStream(dlg.FileName, FileMode.OpenOrCreate))
                    {
                        using (var ts = new StreamWriter(fs))
                        {
                            //Console.WriteLine(jsList.Text);
                            ts.WriteLine(jsList.Text);
                        }
                    }
                }
            }
        }



        private void btnScriptExecute_Click(object sender, RoutedEventArgs e)
        {
            jsList.Execute();
        }

        private void btnScriptStop_Click(object sender, RoutedEventArgs e)
        {
            jsList.Terminate();
        }
        private void btnScriptKill_Click(object sender, RoutedEventArgs e)
        {
            jsList.Abort();
        }

        private void btnWhatsNew_Click(object sender, RoutedEventArgs e)
        {
            
            var dlg = Container.Resolve<WhatsNew>();
            dlg.Show();
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            var dlg = Container.Resolve<HelpWindow>();
            dlg.Show();
        }

        #region Palmsense

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //devices[cbPalmSensDevices.SelectedIndex].Open(230400);
                palmsense.Connect(devices[cbPalmSensDevices.SelectedIndex]);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Invalid port number. Palmsense not connected?");
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        
        private void btnSaveMeasurement_Click(object sender, RoutedEventArgs e)
        {
            using (SaveFileDialog fileDialog = new SaveFileDialog())
            {
                fileDialog.Filter = "Palmsense data files (*.pssession)|*.pssession";
                fileDialog.FilterIndex = 1;
                fileDialog.RestoreDirectory = true;
                fileDialog.AddExtension = true;

                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    palmsense.SaveMeasurements(fileDialog.FileName);
                }
            }
        }

        private void OnMethodClickedEvent()
        {
            btnSelectMethodFile_Click(null, null);
        }
        private void OnSaveClickedEvent()
        {
            btnSaveMeasurement_Click(null, null);
        }

        private void OnMeasureClickedEvent()
        {
            btnMeasure_Click(null, null);
        }
        private void btnSelectMethodFile_Click(object sender, RoutedEventArgs e)
        {
            string filepath = string.Empty;
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Palmsense method files (*.psmethod)|*.psmethod";
                fileDialog.FilterIndex = 1;
                fileDialog.RestoreDirectory = true;
                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    palmsense.LoadMethod(fileDialog.FileName);
                }
            }
        }

        private void psCommSimpleWPF_Disconnected(object sender, Exception CommErrorException)
        {
            Logger.WriteLine("Palmsense disconnected");
            setFlag(STATES.PALMSESNSE_ENABLED, false);
        }

       

        private void btnMeasure_Click(object sender, RoutedEventArgs e)
        {
            palmsense.Measure();
        }
        #endregion

        #region Arduino
        private void btnPortConnect_Click(object sender, RoutedEventArgs e)
        /*Click event handler of the button in Arduino Port Ribbon group*/
        {
            if (cbComPorts.Text == "")
            {
                Logger.WriteLine("Select port first.");
                return;
            }

            Logger.WriteLine($"Trying to open selected COM port {cbComPorts.Text}");
            arduino.PortName = cbComPorts.Text;
            try
            {
                arduino.open();
            }
            catch
            {
                return;
            }
            setFlag(STATES.ARDUINO_CONNECTED, true);
        }


        private void btnSendCustomCmd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                arduino.sendCustomCmd(tbCustomCommand.Text);
            }
            catch (SystemException ex)
            {
                Logger.WriteLine(ex.Message);
            }
        }

        #endregion

        private void cbComPorts_DropDownClosed(object sender, EventArgs e)
        {
            var settings = Container.Resolve<Properties.Settings>();
            settings.ArduinoPort = cbComPorts.Text;
            settings.Save();
        }

        private void cbHuberPorts_DropDownClosed(object sender, EventArgs e)
        {
            var settings = Container.Resolve<Properties.Settings>();
            settings.HuberPort = cbHuberPorts.Text;
            settings.Save();
        }

        private void cbPalmSensDevices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var settings = Container.Resolve<Properties.Settings>();
            settings.PalmsensePortIndex = cbPalmSensDevices.SelectedIndex;
            settings.Save();
        }

        private void btnConnectLastDevices_Click(object sender, RoutedEventArgs e)
        {
            
            btnConnect_Click(null, null);     //Palmsens
            if (arduino.isOpen == false)
                btnPortConnect_Click(null, null); //Arduino
            if(huber.isOpen==false) 
                btnHuberConnect_Click(null, null);
        }

        private void btnHuberConnect_Click(object sender, RoutedEventArgs e)
        {
            if (cbHuberPorts.Text == "")
            {
                Logger.WriteLine("Select Huber port first.");
                return;
            }
            Logger.WriteLine($"Trying to open selected COM port {cbHuberPorts.Text}");
            huber.PortName = cbHuberPorts.Text;
            try
            {
                huber.open();
            }
            catch
            {
                return;
            }
            setFlag(STATES.HUBER_CONNECTED, true);
        }

        private void btnStartHuber_Click(object sender, RoutedEventArgs e)
        {
            huber.start();
        }

        private void btnStopHuber_Click(object sender, RoutedEventArgs e)
        {
            huber.stop();
        }

        private void btnUserSettings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = Container.Resolve<Dialogs.SettingsWindow>();
            dlg.Show();
        }

        private void btnSetHuberT_Click(object sender, RoutedEventArgs e)
        {
            if(huber.isOpen)
            {
                var dlg = Container.Resolve<HuberSetTDlg>();
                dlg.Show();
            }
            else
            {
                Logger.WriteLine("Connect Huber device first");
            }
            
        }
    }
}
