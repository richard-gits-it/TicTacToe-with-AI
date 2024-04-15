using DataClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TCP280Project;

namespace GameDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StartupControl startupControl;
        LoginControl loginPage;
        

        public MainWindow()
        {
            InitializeComponent();
            LoadStartupPage();
        }

        private void OnClose()
        {
            Close();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region load pages

        public void LoadStartupPage()
        {
            startupControl = new StartupControl();
            startupControl.OnClose += OnClose;
            this.grid.Children.Clear();
            this.grid.Children.Add(startupControl);

        }
        public void LoadLoginPage()
        {
            loginPage = new LoginControl();
            loginPage.OnClose += OnClose;
            this.grid.Children.Clear();
            this.grid.Children.Add(loginPage);

            loginPage.txtUsername.Clear();
        }
        //load onlinemenu
        public async Task LoadOnlineMenuAsync(string username)
        {
            MyDataHelper._client = new Client("localhost", 10000);
            MyDataHelper._client.ReceivePacket += _client_ReceivePacket;
            Packet280 tmp = new Packet280();
            tmp.ContentType = MessageType.Connected;
            tmp.Payload = MyDataHelper.currentPlayer;
            MyDataHelper.isConnected = true;
            await MyDataHelper._client.SendMessage(tmp);


            //isConnected = true load the online menu
            if (MyDataHelper.isConnected)
            {
                OnlineMenu onlineMenu = new OnlineMenu(username, MyDataHelper._client);
                onlineMenu.OnClose += OnClose;
                this.grid.Children.Clear();
                this.grid.Children.Add(onlineMenu);
            }
            else
            {
                MessageBox.Show("Could not connect to the server");
            }
        }

        private void _client_ReceivePacket(Packet280 packet)
        {
            if (packet.ContentType == MessageType.Connected)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var newPlayers = JsonConvert.DeserializeObject<ObservableCollection<string>>(packet.Payload);
                    MyDataHelper.Players.Clear();
                    foreach (var player in newPlayers)
                    {
                        //add the players to the list except the current player
                        if (player != MyDataHelper.currentPlayer)
                        {
                            MyDataHelper.Players.Add(player);
                        }
                    }
                });
            }
            else if (packet.ContentType == MessageType.Disconnected)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var newPlayers = JsonConvert.DeserializeObject<ObservableCollection<string>>(packet.Payload);
                    MyDataHelper.Players.Clear();
                    foreach (var player in newPlayers)
                    {
                        //add the players to the list except the current player
                        if (player != MyDataHelper.currentPlayer)
                        {
                            MyDataHelper.Players.Add(player);
                        }
                    }
                });
            }else if (packet.ContentType == MessageType.Error)
            {
                MyDataHelper.Players.Clear();
                MyDataHelper.isConnected = false;
                MessageBox.Show(packet.Payload);
            }
        }

        //load game
        public void LoadGame()
        {
            GameControl gameControl = new GameControl();
            gameControl.OnClose += OnClose;
            this.grid.Children.Clear();
            this.grid.Children.Add(gameControl);
        }

#endregion
    }
}
