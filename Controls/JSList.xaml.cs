using Autofac;
using Jint;
using MeasureConsole.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using JsTools;
using Jint.Runtime.Debugger;
using PalmSens.Core.Simplified.WPF;

namespace MeasureConsole.Controls
{
    public partial class JSList : UserControl, IJSList
    {
        private string _text;
        private Engine engine;
        public static ManualResetEvent mre = new ManualResetEvent(false);
        private static bool _isPreviousStatementComplete = true;
        Thread jsThread = null;
        private CancellationTokenSource cancellationTokenSource;
        
        private class EachStepConstraint: Jint.Constraint
        {
            public override void Reset()
            {

            }
            public override void Check()
            {
                
                if (IsPreviousStatemenComplete == false)
                {
                    mre.WaitOne();
                }
            }
        }

        public static bool IsPreviousStatemenComplete {
            get
            {
                return _isPreviousStatementComplete;
            }
            set
            {
                _isPreviousStatementComplete = value;
                if (value == true)
                {
                    mre.Set();
                }
                else
                {
                    mre.Reset();
                }
                
            }
        }

        public string Text {
            get
            {
                _text = tbScript.Text;
                return _text;
            }
            set
            {
                tbScript.Text = value;
                _text = value;
            }
        }
        public void Execute()
        {
            jsThread = new Thread(Go);
            jsThread.IsBackground = true;
            jsThread.Start();
                        
        }
        public void Terminate()
        {
            cancellationTokenSource.Cancel();
        }

        public void Abort()
        {
            try
            {
                if (jsThread.IsAlive)
                {
                    jsThread.Abort();
                    Logger.Cleanup();
                    jsThread = null;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                restoreDefaultState();
                
            }
        }

        private void restoreDefaultState()
        {
            this.Dispatcher.Invoke(() =>
            {
                tbScript.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                tbScript.IsEnabled = true;
                rHeader.Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0xC4, 0xD1, 0xEC));
                var mw = Factory.Container.Resolve<MainWindow>();
                mw.btnScriptExecute.IsEnabled = true;
                mw.btnScriptStop.IsEnabled = false;
                mw.btnScriptKill.IsEnabled = false;
            });
        }

        private void Go()
        {
            string script = "";
            IsPreviousStatemenComplete = true;
            this.Dispatcher.Invoke(() =>
            {
                tbScript.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                tbScript.IsEnabled = false;
                rHeader.Fill = new SolidColorBrush(Color.FromRgb(250, 150, 150));
                var mw = Factory.Container.Resolve<MainWindow>();
                mw.btnScriptExecute.IsEnabled = false;
                mw.btnScriptStop.IsEnabled = true;
                mw.btnScriptKill.IsEnabled = true;

            });
            this.Dispatcher.Invoke(() =>
            {
                script = tbScript.Text + "\n log(\"Script finished\");";
            });

            try
            {
                //Console.WriteLine(script);
                engine.Execute(script);
                //engine.GetCompletionValue();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message +  e.StackTrace);      
            }

            restoreDefaultState();
        }

        private void LoggerWrapper(string str, params object[] objs)
        {
            this.Dispatcher.Invoke(() =>
            {
                Logger.WriteLine(str, objs);
            });
        }

        public JSList()
        {
            InitializeComponent();
            cancellationTokenSource = new CancellationTokenSource();
            engine = new Engine(/*c => c.AllowClr(typeof(JsTimer).Assembly)*/ c =>
            {
                c.DebugMode(true);
                c.CancellationToken(cancellationTokenSource.Token);
                c.Constraint(new EachStepConstraint());
            }).
            SetValue("log", new Action<string, object[]>(LoggerWrapper)).
            SetValue("wait", new Action<int>(Logger.Wait)).
            SetValue("ChangePotentialinCA", new Action<string, string, double>(MethodCreation.ChangePotentialinCA)).
            SetValue("ChangePotentialinCAmultistep", new Action<string, string, double>(MethodCreation.ChangePotentialinCAmultistep)).
            SetValue("DumpPotentialinFile", new Action<double, string>(DataDump.DumpPotentialinFile)).
            SetValue("CreateInitFile", new Action<string, string, double>(DataDump.CreateInitFile)).
            SetValue("CreateEndFile", new Action<string>(DataDump.CreateEndFile)).
            SetValue("arduino", Factory.Container.Resolve<IArduino>()).
            SetValue("convert", new Action<string, string>(Logger.PSSessionToCSV)).
            SetValue("palmsense", Factory.Container.Resolve<IPalmsense>()).
            SetValue("huber", Factory.Container.Resolve<IHuber>());
            //engine.BreakPoints.Add(new BreakPoint(0, 0)); //Breakpoints are not supported in Jint v3.!!!!
            //engine.Step += OnJSStep;
            
        }
       
        private StepMode OnJSStep(object sender, DebugInformation e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Logger.WriteLine($"Script line #{e.Location.Start.Line.ToString()} executed");
                
            });

            if(IsPreviousStatemenComplete==false)
            {
                mre.WaitOne();
            }
            return StepMode.Over;
        }
    }
}
