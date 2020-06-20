using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AwesomeCommon;
using Newtonsoft.Json;

namespace AwesomeClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private AutoResetEvent _stopEvent = new AutoResetEvent(false);
        public MainWindow()
        {
            InitializeComponent();
            txtAddr.Text = "127.0.0.1";
            txtUser.Text = "Client";
            DataContext = this;

            WaitHandle[] handle = new WaitHandle[2];
            handle[0] = _stopEvent;
            handle[1] = ControlManagerClient.Instance.TheWebsocketManager.MessageReceivedEvent;
            Thread t = new Thread(ThreadProc);
            t.Start(handle);
            ResultText = "Test";
        }
        private string _resultText;
        public string ResultText
        {
            get => _resultText;
            set
            {
                if (value != _resultText)
                {
                    _resultText = value;
                    OnPropertyChanged("ResultText");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            ControlManagerClient.Instance.Close();
            _stopEvent.Set();
        }

        private void BtnSend_OnClick(object sender, RoutedEventArgs e)
        {
            ControlManagerClient.Instance.Send(txtCommand.Text);
        }

        private void BtnConnect_OnClick(object sender, RoutedEventArgs e)
        {
            ControlManagerClient.Instance.CreateWebSocket(txtAddr.Text);
            CommandMessage cmd = new CommandMessage()
            {
                Command = 103,
                Target = 1
            };
            cmd.Payload.Add("room", txtRoom.Text);
            cmd.Payload.Add("clientname", txtUser.Text);
            cmd.Payload.Add("password", "");
            var msg = JsonConvert.SerializeObject(cmd);
            //string msg = $"";
            //msg = $"<EL><target>1</target><command>" +(int)CommandType.JOIN_CHATROOM+"</command>" +
            //      $"<payload><room>{txtRoom.Text}</room><clientname>{txtUser.Text}</clientname></payload></EL>";
            ControlManagerClient.Instance.Send(msg);
        }
        void ThreadProc(object data)
        {
            WaitHandle[] waitHandles = (WaitHandle[])data;
            while (true)
            {
                int index = WaitHandle.WaitAny(waitHandles);
                if (index == 0)
                    break;
                else if (index == 1)
                {
                    UpdateReceivedMessage();
                }
            }
        }
        private void UpdateReceivedMessage()
        {
            string newMessage = ControlManagerClient.Instance.TheWebsocketManager.LastMessageReceived;
            string tmp = "\r\n--->" + DateTime.Now.ToString("hh:mm:ss.fff") + "    " + newMessage;
            ResultText += tmp;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            _stopEvent.Set();
        }
    }
}
