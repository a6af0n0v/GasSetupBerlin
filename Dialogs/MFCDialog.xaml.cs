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
    /// <summary>
    /// Interaction logic for MFCDialog.xaml
    /// </summary>
    /// 
    public partial class MFCDialog : Window
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
                _value = value;
            } 
        }

        public MFCDialog(bool isMFC)
        {
            InitializeComponent();
            sValue.Maximum = 100;
            if (isMFC)
            {
                Title = "MFC";
            }
            else {
                Title = "Piezo";
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Value = Convert.ToInt32(sValue.Value);
            result = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Value = 0;
            result = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void btnCloseValve_Click(object sender, RoutedEventArgs e)
        {
            Value = 100;
            result = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
