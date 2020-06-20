using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeCommon
{
    public class CommandMessage
    {
        public int Target { get; set; }
        public int Command { get; set; }
        public Dictionary<string,string> Payload { get; set; }

        public CommandMessage()
        {
            Payload = new Dictionary<string, string>();
        }
    }
}
