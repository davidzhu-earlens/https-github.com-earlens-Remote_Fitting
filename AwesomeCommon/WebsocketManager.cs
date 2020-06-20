using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace AwesomeCommon
{
    public class WebsocketManager
    {
        WebSocket webSocket;
        private AutoResetEvent messageReceiveEvent = new AutoResetEvent(false);
        public string ServerAddr { get; set; }
        public int Port { get; set; }
        public AutoResetEvent MessageReceivedEvent => messageReceiveEvent;
        public string LastMessageReceived { get; set; }

        public WebsocketManager()
        {
            Port = 8088;
        }

        public void ConnectServer(string url)
        {
            webSocket = new WebSocket($"ws://{url}/");
            webSocket.Opened += new EventHandler(websocket_Opened);
            webSocket.Closed += new EventHandler(websocket_Closed);
            webSocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
            webSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);
            webSocket.DataReceived += new EventHandler<DataReceivedEventArgs>(websocket_DataReceived);
            webSocket.Open();
            while (webSocket.State == WebSocketState.Connecting) { };   // by default webSocket4Net has AutoSendPing=true, 
            // so we need to wait until connection established
            if (webSocket.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not opened.");
            }
            LastMessageReceived = $"Connected server: {url}";
            messageReceiveEvent.Set();
        }
        public string Send(string data)
        {
            Console.WriteLine("Client wants to send string:");
            Console.WriteLine(data);
            webSocket.Send(data);
            LastMessageReceived = $"Message send: {data}";
            messageReceiveEvent.Set();
            return LastMessageReceived;
        }

        public string Send(byte[] data)
        {
            Console.WriteLine("Client wants to send data:");
            Console.WriteLine(AwesomeExtensions.ByteArrayToString(data));

            webSocket.Send(data,0,data.Length);
            LastMessageReceived = $"Message send: {AwesomeExtensions.ByteArrayToString(data)}";
            messageReceiveEvent.Set();
            return LastMessageReceived;
        }
        public void Close()
        {
            Console.WriteLine("Closing websocket...");
            webSocket.Close();
            LastMessageReceived = $"Server closed";
            messageReceiveEvent.Set();
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Websocket is opened.");
        }

        private void websocket_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
            LastMessageReceived = $"server error: {e.Exception.Message}";
            messageReceiveEvent.Set();
        }
        private void websocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Websocket is closed.");
        }

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("Message received: " + e.Message);
            LastMessageReceived = $"Message received: {e.Message}";
            messageReceiveEvent.Set();
        }

        private void websocket_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("Message received: " + AwesomeExtensions.ByteArrayToString(e.Data));
            LastMessageReceived = $"Message received: {AwesomeExtensions.ByteArrayToString(e.Data)}";
            messageReceiveEvent.Set();
        }

        
    }
}
