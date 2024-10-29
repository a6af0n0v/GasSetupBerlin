using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MeasureConsole.Scene
{
    public interface ISceneControl: IXmlSerializable
    {
        Dictionary<string, string> Attributes { get; set; }
        string  ToString();
        void    Update(string package);
        void    OnAttributesReadHandler();
    }
}
