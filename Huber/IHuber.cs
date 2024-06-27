using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureConsole
{
    interface IHuber
    {
        string PortName { set; get; }
        bool isOpen { get; }
        void open();
        short readSetPoint();
        void start();
        void stop();
        void setTemperature(int t); //in hundred of degree wo dec point
        char status();
    }
}
