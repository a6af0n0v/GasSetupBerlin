using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureConsole.Demonstrators
{
    public class DemonstratorDescriptor
    {
        public string      comName;
        public string      demoId;
        public SerialPort  portHandle;
    }

    internal interface IDemonstrator
    {
        DemonstratorDescriptor[]    Discover        ();
        void                        Open            (string comName);
        void                        Close           (string comName);
        void                        RunMethodScript (string comName, string methodScriptPath, string csv);
    }
}
