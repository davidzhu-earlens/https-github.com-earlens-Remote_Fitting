using SuperWebSocket;
using System;

namespace AwesomeServer
{
    class Program
    {
        private static WebSocketServer wsServer;
        static void Main(string[] args)
        {
            wsServer = new WebSocketServer();
            int port = 8088;
            wsServer.Setup(port);
            wsServer.NewSessionConnected += WsServer_OnNewSessionConnected;
            wsServer.NewMessageReceived += WsServer_NewMessageReceived;
            wsServer.NewDataReceived += WsServer_OnNewDataReceived;
            wsServer.SessionClosed += WsServer_SessionClosed;
            wsServer.Start();
            Console.WriteLine($"Server is running on port {port}. Press ENTER to exit...");
            Console.ReadKey();
            wsServer.Stop();

        }

        private static void WsServer_OnNewSessionConnected(WebSocketSession session)
        {
            Console.WriteLine($"ID {session.SessionID} OnNewSessionConnected");
            ControlManager.Instance.OnNewSessionConnected(session);
        }

        private static void WsServer_OnNewDataReceived(WebSocketSession session, byte[] value)
        {
            Console.WriteLine($"ID {session.SessionID} OnNewDataReceived: {value}");
            ControlManager.Instance.OnNewDataReceived(session,value);
        }

        private static void WsServer_NewMessageReceived(WebSocketSession session, string value)
        {
            Console.WriteLine($"ID {session.SessionID} NewMessageReceived: " + value);
            if (value == "Hello server")
            {
                session.Send("Hello client");
            }
            ControlManager.Instance.OnNewMessageReceived(session,value);
        }

        private static void WsServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            Console.WriteLine($"ID {session.SessionID} SessionClosed");
            ControlManager.Instance.OnSessionClosed(session);
        }

    }
}
