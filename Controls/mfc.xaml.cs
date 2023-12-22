
using Autofac;
using MeasureConsole.Bootstrap;
using MeasureConsole.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MeasureConsole.Controls
{
    /// <summary>
    /// Interaction logic for mfc.xaml
    /// </summary>
    public partial class mfc : UserControl
    {
        public mfc()
        {

            InitializeComponent();
        }

        private float _value;

        public int Channel { get; set; }
        public bool isMFC { get; set; }

        public float Value // in l/min
        {
            get 
            {    
                return _value; 
            }    
            set 
            { 
                _value = value;
                if(isMFC)
                    lbValue.Content = (_value*100).ToString("N2");
            }
        }

        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var container = Factory.Container;
            var mainWnd = container.Resolve<MainWindow>();
            if (mainWnd.Scheme.PIDEnabled == true)
            {
                Logger.WriteLine("PID activated. To control valves manually disable PID.");
                return;
            }
            MFCDialog dlg = new MFCDialog(isMFC);
            var pos = Mouse.GetPosition(null);
            dlg.Left = pos.X;
            dlg.Top = pos.Y;
            dlg.ShowDialog();
            var arduino = container.Resolve<IArduino>();
            {
                if ((dlg.result == System.Windows.Forms.DialogResult.OK)||
                    (dlg.result == System.Windows.Forms.DialogResult.Abort))
                {
                    if (arduino.isOpen)
                    {
                        float setValue = 0;
                        if (dlg.result == System.Windows.Forms.DialogResult.Abort)
                            setValue = 0;
                        else
                        {
                            setValue = ((float)dlg.Value / 100);
                            if (isMFC)
                            {
                                
                                arduino.setFlow(Channel, setValue);
                            }
                            else //proportional valve
                            {
                                arduino.setProportionalValve(Channel, setValue);
                                if(Channel == 0)
                                {
                                    mainWnd.Scheme.PropValve1SetValue = setValue;
                                }
                                else if(Channel==1)
                                {
                                    mainWnd.Scheme.PropValve2SetValue = setValue;
                                }
                            }
                        }
                        
                        //Console.WriteLine($"value raw {setValue}");
                    }
                    else
                    {
                        Logger.WriteLine("Can't set value. Arduino is not connected.");
                    }
                }


            }
        }
    }
}
