using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MeasureConsole.Dialogs
{
    public partial class PIDDialog : Window
    {
        public DialogResult result { get; set; }
        private int _value = 0;

        public int Value {
            get
            {
                return _value;
            }
            set
            {
                sValue.Value = value;
                _value = value;
            } 
        }

        public PIDDialog()
        {
            InitializeComponent();
            sValue.Maximum = 100;
        }

        private void StopDryClicked(object sender, RoutedEventArgs e)
        {
            Value = 0;
            result = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void StopWetClicked(object sender, RoutedEventArgs e)
        {
            Value = 100;
            result = System.Windows.Forms.DialogResult.OK;
            Close();
        }
        private void PIDStartClicked(object sender, RoutedEventArgs e)
        {
            Value = Convert.ToInt32(sValue.Value);
            result = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
