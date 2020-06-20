using Newtonsoft.Json;
using SuperWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AwesomeCommon;
using Newtonsoft.Json.Linq;

namespace AwesomeServer
{
    class ControlManager
    {
        private Hashtable _clientsList;
        private Hashtable _sessionsList;
        private HashSet<int> _roomList = new HashSet<int>();
        object _lockObj = new object();
        private static readonly Lazy<ControlManager> lazy = new Lazy<ControlManager>(() => new ControlManager());
        public static ControlManager Instance => lazy.Value;
        public delegate bool COMMAND_HANDLER(string comParameter, out string strResult);
        private Dictionary<int, COMMAND_HANDLER> _commandDictionary;

        private ControlManager()
        {
            Initializer();
        }

        private void Initializer()
        {
            _clientsList = new Hashtable();
            _sessionsList = new Hashtable();
            _commandDictionary = new Dictionary<int, COMMAND_HANDLER>()
            {
                { (int)CommandType.CREATE_CHATROOM, CreateChatRoom },
                { (int)CommandType.DELETE_CHATROOM, DeleteChatRoom },
                { (int)CommandType.JOIN_CHATROOM, JoinChatRoom },
                { (int)CommandType.ADD_TO_CHATROOM, AddToChatRoom },
                { (int)CommandType.REMOVE_FROM_CHATROOM, RemoveFromChatRoom },
                { (int)CommandType.SEND_MESSAGE, SendMessage },
                { (int)CommandType.SETUP_CLIENT, SetupClient },
            };
        }

        public int CommandID { get; set; }
        public string Payload { get; set; }
        public HandleClient FromClient { get; set; }

        public void OnNewSessionConnected(WebSocketSession session)
        {
            _sessionsList.Add(session.SessionID, session);
        }

        public void OnSessionClosed(WebSocketSession session)
        {
            if (_sessionsList.ContainsKey(session.SessionID))
            {
                _sessionsList.Remove(session.SessionID);
                if (_clientsList.ContainsKey(session.SessionID))
                    _clientsList.Remove(session.SessionID);
            }
        }

        public void OnNewMessageReceived(WebSocketSession session, string value)
        {
            lock (_lockObj)
            {
                if (!_clientsList.ContainsKey(session.SessionID))
                {
                    HandleClient client = new HandleClient(session);
                    _clientsList.Add(session.SessionID, client);
                    FromClient = client;
                }
                else
                    FromClient = (HandleClient)_clientsList[session.SessionID];

                if (ParseCommandJSON(value))
                //if (ParseCommand(value))
                    RunActions();
            }
        }

        public void OnNewDataReceived(WebSocketSession session, byte[] value)
        {
            if (_clientsList.ContainsKey(session.SessionID))
            {
                FromClient = (HandleClient)_clientsList[session.SessionID];
            }
            else
            {
                HandleClient client = new HandleClient(session);
                _clientsList.Add(session.SessionID, client);
                FromClient = client;
            }

            ((HandleClient)_clientsList[session.SessionID]).SendData(value);
        }

        private Boolean ParseCommandJSON(string jsonText)
        {
            try
            {

                JObject jo = JObject.Parse(jsonText);
                CommandMessage cmd = jo.ToObject<CommandMessage>();
                Payload = (string)jo["Payload"].ToString();
                CommandID = cmd.Command;
                //using (var reader = new JsonTextReader(new StringReader(jsonText)))
                //{
                //    while (reader.Read())
                //    {
                //        Console.WriteLine("{0} - {1} - {2}",
                //            reader.TokenType, reader.ValueType, reader.Value);
                //    }
                //}
            }
            catch (Exception ee)
            {
                Console.WriteLine("ParseCommandJSON(). error message :" + ee.Message + "\n");
                return false;
            }
            return true;
        }

        private Boolean ParseCommand(string command)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(command);
                int iTemp;
                XmlNode xmlNode = xmlDoc.DocumentElement.SelectSingleNode("//EL/command");
                CommandID = (int)CommandType.UNDEFINED;
                if (xmlNode != null && xmlNode.InnerText.Length > 0)
                {
                    bool bValid = int.TryParse(xmlNode.InnerText, out iTemp);
                    if (bValid)
                    {
                        CommandID = iTemp;
                        xmlNode = xmlDoc.DocumentElement.SelectSingleNode("//EL/payload");
                        if (xmlNode != null && xmlNode.InnerText.Length > 0)
                            Payload = xmlNode.InnerXml.Trim();
                    }
                }

            }
            catch (Exception ee)
            {
                Console.WriteLine("ParseCommand(). error message :" + ee.Message + "\n");
                return false;
            }
            return true;
        }

        private void RunActions()
        {
            var strResult = String.Empty;
            bool bRtn;
            COMMAND_HANDLER handler = null;
            try
            {
                handler = _commandDictionary[CommandID];
            }
            catch
            {
                handler = null;
            }
            if (handler != null)
                bRtn = handler(Payload, out strResult);
            
        }

        private bool CreateChatRoom(string comParameter, out string strResult)
        {
            strResult = String.Empty;
            Random random = new Random();
            int room = random.Next(100, 999);
            while (_roomList.Contains(room))
                room = random.Next(100, 999);
            _roomList.Add(room);
            FromClient.Room = room;
            FromClient.ClientMode = ClientType.ADMIN;
            CommandMessage cmd = new CommandMessage()
            {
                Command = (int)CommandType.CREATE_CHATROOM,
                Target = 3
            };
            cmd.Payload.Add("room", room.ToString());
            strResult = JsonConvert.SerializeObject(cmd);

            //strResult =
            //    $"<EL><target>3</target><command>101</command><response>0</response><payload>{room}</payload></EL>";
            FromClient.SendMessage(strResult);
            Console.WriteLine($"room {room} created");
            return true;
        }

        private bool DeleteChatRoom(string comParameter, out string strResult)
        {
            var result = false;
            int iTemp;
            strResult = String.Empty;
            int room;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml($"<EL>{comParameter}</EL>");
            XmlNode xmlNode = xmlDoc.DocumentElement.SelectSingleNode("//EL/room");
            if (xmlNode != null && xmlNode.InnerText.Length > 0)
            {
                bool bValid = int.TryParse(xmlNode.InnerText, out iTemp);
                if (bValid)
                {
                    room = iTemp;
                    if (_roomList.Contains(room))
                    {

                    }
                }
            }

            return result;
        }

        private bool JoinChatRoom(string jsonText, out string strResult)
        {
            var result = false;
            strResult = String.Empty;
            int iTemp;
            strResult = String.Empty;
            int room;

            JObject jo = JObject.Parse(jsonText);

            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
            {
                string strTemp = (string) jo["room"].ToString();
                bool bValid = int.TryParse(strTemp, out iTemp);
                if (bValid)
                {
                    room = iTemp;
                    if (_roomList.Contains(room))
                    {
                        FromClient.Room = room;
                        FromClient.ClientMode = ClientType.CLIENT;
                        FromClient.ClientName = (string)jo["clientname"].ToString();
                        CommandMessage cmd = new CommandMessage()
                        {
                            Command = (int)CommandType.JOIN_CHATROOM,
                            Target = 2
                        };
                        cmd.Payload.Add("success", "pass");
                        strResult = JsonConvert.SerializeObject(cmd);
                        FromClient.SendMessage(strResult);

                        //notice admin
                        HandleClient AdminClient = GetClientHandleByRoomAndMode(room, ClientType.ADMIN);
                        Console.WriteLine($" client {FromClient.ClientName} joined room {room}");
                        cmd = new CommandMessage()
                        {
                            Command = (int)CommandType.ADD_TO_CHATROOM,
                            Target = 1
                        };
                        cmd.Payload.Add("room", room.ToString());
                        cmd.Payload.Add("clientname", FromClient.ClientName);
                        strResult = JsonConvert.SerializeObject(cmd);
                        AdminClient.SendMessage(strResult);
                    }
                }
            }
            return result;
        }

        private bool AddToChatRoom(string comParameter, out string strResult)
        {
            strResult = String.Empty;
            return true;
        }

        private bool RemoveFromChatRoom(string comParameter, out string strResult)
        {
            strResult = String.Empty;
            return true;
        }

        private bool SendMessage(string comParameter, out string strResult)
        {
            strResult = String.Empty;
            return true;
        }

        private bool SetupClient(string comParameter, out string strResult)
        {
            strResult = String.Empty;
            return true;
        }

        private HandleClient GetClientHandleByRoomAndMode(int room, ClientType mode)
        {
            HandleClient client = null;
            foreach (DictionaryEntry entry in _clientsList)
            {
                client = (HandleClient) entry.Value;
                if (client.Room == room && client.ClientMode == mode)
                    return client;
            }

            return client;
        }
    }
}
