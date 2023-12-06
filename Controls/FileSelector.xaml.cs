
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MeasureConsole.Controls
{
    /// <summary>
    /// Interaction logic for FileSelector.xaml
    /// </summary>
    public partial class FileSelector : System.Windows.Controls.UserControl
    {
        public FileSelector()
        {
            InitializeComponent();
        }

        public string SelectedPath
        {
            set
            {
                tbSelectedPath.Text = value;
            }
            get
            {
                return tbSelectedPath.Text;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {                    
                    tbSelectedPath.Text = dlg.SelectedPath;
                }
            }
        }
    }
}
