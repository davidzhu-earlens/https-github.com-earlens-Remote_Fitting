using SuperWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AwesomeServer
{
    public enum ClientType
    {
        ADMIN,
        CLIENT
    }

    public class HandleClient
    {
        private WebSocketSession _session;
        object _lockObj = new object();

        public string SessionID { get; set; }
        public string ClientNo { get; set; }
        public int Room { get; set; } 
        public string ClientName { get; set; }
        public ClientType ClientMode { get; set; }

        public HandleClient(WebSocketSession session)
        {
            _session = session;
            SessionID = _session.SessionID;
        }

        public async void SendMessage(string message)
        {
            lock (_lockObj)
            {
                _session.Send(message);
            }
        }

        public async void SendData(byte[] data)
        {
            lock (_lockObj)
            {
                _session.Send(data, 0, data.Length);
            }
        }

 
    }
}
