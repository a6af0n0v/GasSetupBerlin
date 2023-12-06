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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MeasureConsole.Controls
{
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Spinner : UserControl
    {
        private int _value;
        private GridLength _column2Width;
        [Category("SpinnerControl")]
        public GridLength Column2Width
        {
            set
            {
                cdColumn2.Width = value;
                _column2Width = value;
            }
            get
            {
                return _column2Width;
            }
        }
        [Category("SpinnerControl")]
        public int Value
        {
            set
            {
                if (value > this.Maximum)
                        _value = this.Maximum;
                else if (value < this.Minimum)
                    _value = this.Minimum;
                else
                    _value = value;
                tbValue.Text = value.ToString();
                //Logger.WriteLine($"value :{_value}"); 
            }
            get
            {
                return _value;
            }
        }
        [Category("SpinnerControl")]
        public int Minimum
        {
            set;
            get;
        } = 0;
        [Category("SpinnerControl")]
        public int Maximum
        {
            set;
            get;
        } = 4;

        [Category("SpinnerControl")]
        public int Step
        {
            set;
            get;
        } = 1;
        

        public Spinner()
        {
            InitializeComponent();
            Value = 0;
            tbValue.VerticalAlignment = VerticalAlignment.Center;
        }
       

        private void btnIncrease_Click(object sender, RoutedEventArgs e)
        {
            this.Value += this.Step;
           

        }

        private void btnDecrease_Click(object sender, RoutedEventArgs e)
        {
            this.Value -= this.Step;
            
        }

        private void tbValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            List<Key> keys = new List<Key>();
            keys.Add(Key.D0);
            keys.Add(Key.D1);
            keys.Add(Key.D2);
            keys.Add(Key.D3);
            keys.Add(Key.D4);
            keys.Add(Key.D5);
            keys.Add(Key.D6);
            keys.Add(Key.D7);
            keys.Add(Key.D8);
            keys.Add(Key.D9);
            //keys.Add(Key.Decimal);
            keys.Add(Key.Left);
            keys.Add(Key.Right);
            keys.Add(Key.Delete);
            keys.Add(Key.Back);
            if (keys.Contains(e.Key))
            {
                e.Handled = false;
                
            }
            else
            {
                e.Handled = true;
                
            }
        }

        private void tbValue_KeyUp(object sender, KeyEventArgs e)
        {
            int _value = 0;
            try
            {
                _value = Convert.ToInt32(tbValue.Text);
            }
            catch (System.FormatException ex)
            {
                return;
            }
            this.Value = _value;
        }
    }
}
