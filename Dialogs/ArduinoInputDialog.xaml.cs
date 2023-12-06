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
using System.Windows.Shapes;
using System.Windows.Forms;

namespace MeasureConsole
{
    /// <summary>
    /// Interaction logic for ArduinoInputDialog.xaml
    /// </summary>
    public partial class ArduinoInputDialog : Window
    {
        public DialogResult result
        {
            get;
            set;
        }

        public ArduinoInputDialog()
        {
            InitializeComponent();
            result = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            result = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
