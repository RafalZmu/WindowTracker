using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using WindowTracker.MVVM.Core;
using WindowTracker.MVVM.Model;
using WindowTracker.Net;


namespace WindowTracker.MVVM.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand ChangeListCommand { get; set; }
        public RelayCommand DisconnectFromServerCommand { get; set; }
        public string Name { get; set; }
        public string OpenWindow { get; set; }
        private Server _server;

        private string _connectionButtonContent = "Connect";
        public string ConnectionButtonContent
        {
            get => _connectionButtonContent;
            set
            {
                _connectionButtonContent = value;
                OnPropertyChanged(nameof(ConnectionButtonContent));
            }
        } 

        private string _displayLive = "Collapsed";
        public string DisplayLive
        {
            get => _displayLive;
            set
            {
                _displayLive = value;
                OnPropertyChanged(nameof(DisplayLive));

            }
        }
        private string _displayLog = "Visible";
        public string DisplayLog
        {
            get => _displayLog;
            set
            {
                _displayLog = value;
                OnPropertyChanged(nameof(DisplayLog));

            } }
        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected= value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }



        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();

            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectedEvent += RemoveUser;
            ConnectToServerCommand = new RelayCommand(o => {
                _server.ConnectToServer(char.ToUpper(Name[0]) + Name.Substring(1));
                ConnectButtonChange();
                });

            ChangeListCommand = new RelayCommand(o => MenuItemClick());

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(SendWindowInFocusToServer);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();

        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RemoveUser()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Users.Where(x => x.UID == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            var message = _server.PacketReader.ReadMessage();
            var userUID = message.Split(":")[0];
            var userWindow = message.Split(":")[1];

            var userToUpdate = Users.IndexOf(Users.FirstOrDefault(x => x.UID == userUID));
            if (userToUpdate != -1)
            {
                Users[userToUpdate].LastMessageReceived = DateTime.Now.ToString("HH:mm:ss");
                if (Users[userToUpdate].WindowInFocus != userWindow)
                {
                    Users[userToUpdate].WindowInFocus = userWindow;
                    Application.Current.Dispatcher.Invoke(() => Messages.Add($"[{DateTime.Now:HH:mm:ss}]  User: {Users[userToUpdate].Name}  Window chenged to: {userWindow}"));
                }
            }
        }

        private void UserConnected()
        {
            var user = new UserModel
            {
                Name = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage(),
                
            };
            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
                MessageBox.Show($"{Name} :added {user.Name}");
            }
        }
        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
        private void SendWindowInFocusToServer(object sender, EventArgs e)
        {
            //Sends message to server containing current focused window name every 10 seconds
            OpenWindow = GetActiveWindowTitle();
            if (_server.client.Connected == true && OpenWindow != null)
            {
                _server.SendMessageToServer(OpenWindow);
            }
        }
        public void MenuItemClick()
        {
            DisplayLive = (DisplayLive == "Collapsed") ? "Visible" : "Collapsed";
            DisplayLog = (DisplayLog == "Visible") ? "Collapsed" : "Visible";
            
        }

        private void ConnectButtonChange()
        {
            if (!IsConnected)
            {
                IsConnected= true;
                ConnectionButtonContent = "Disconnect";
            }
            else
            {
                IsConnected= false;
                Messages.Clear();  
                Users.Clear();
                ConnectionButtonContent = "Connect";
            }
        }

    }
}
