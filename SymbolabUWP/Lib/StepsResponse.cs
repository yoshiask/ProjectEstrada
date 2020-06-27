using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolabUWP.Lib
{
    public class StepsResponse
    {
        public string stepLang { get; set; }
        public bool isFromCache { get; set; }
        public bool isInNotebook { get; set; }
        public string standardQuery { get; set; }
        public object[] relatedProblems { get; set; }
        public string subject { get; set; }
        public string topic { get; set; }
        public string subTopic { get; set; }
    }
}
