using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using AwesomeCommon;

namespace AwesomeAdmin
{
    class ControlManagerClient
    {
        private WebsocketManager _websocketManager;
        private static readonly Lazy<ControlManagerClient> lazy = new Lazy<ControlManagerClient>(() => new ControlManagerClient());
        public static ControlManagerClient Instance => lazy.Value;

        private ControlManagerClient()
        {
            Initializer();
        }

        private void Initializer()
        {
            //_clientsList = new Hashtable();
            //_sessionsList = new Hashtable();
            _websocketManager = new WebsocketManager();
        }
        public WebsocketManager TheWebsocketManager => _websocketManager;
        

        public void CreateWebSocket(string url)
        {
            _websocketManager.ConnectServer($"{url}:8088");
        }

        public void Send(string data)
        {
            _websocketManager.Send(data);
        }

        public void Close()
        {
            _websocketManager.Close();
        }

        public void Send(byte[] data)
        {
            _websocketManager.Send(data);
        }
    }
}
