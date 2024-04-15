using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TCP280Project;

namespace GameDemo
{
    /// <summary>
    /// Interaction logic for OnlineMenu.xaml
    /// </summary>
    public partial class OnlineMenu : UserControl
    {
        private int[,] board = new int[3, 3];

        private int player = 1;
        private string _opponentPlayer;
        private bool isTurn = true;

        public delegate void CloseApp();
        public event CloseApp OnClose;


        public OnlineMenu()
        {
            InitializeComponent();
        }

        public OnlineMenu(string username, Client client)
        {
            InitializeComponent();
            MyDataHelper._client = client;
            MyDataHelper.currentPlayer = username;
            MyDataHelper.Players = new ObservableCollection<string>();
        }

        private async void ConnectToServer()
        {
            MyDataHelper._client.ReceivePacket += Client_ReceivePacket;
            Packet280 tmp = new Packet280();
            tmp.ContentType = MessageType.Connected;
            tmp.Payload = MyDataHelper.currentPlayer;
            await MyDataHelper._client.SendMessage(tmp);
        }

        private void Client_ReceivePacket(Packet280 packet)
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
                //update players list
                MyDataHelper.Players.Remove(packet.Payload);
            }
            else if (packet.ContentType == MessageType.Accept)
            {
                isTurn = false;

                //reset the board
                board = new int[3, 3];

                //message box to show that the challenge is accepted
                MessageBox.Show("Challenge Accepted");

                //set the opponent player name from the payload which is a tuple of sender and receiver then enable the buttons
                Tuple<string, string> tmp = JsonConvert.DeserializeObject<Tuple<string, string>>(packet.Payload);
                _opponentPlayer = tmp.Item1;
                player = -1;
            }
            else if (packet.ContentType == MessageType.Decline)
            {
                MessageBox.Show($"{packet.Payload}");
            }
            else if (packet.ContentType == MessageType.Invite)
            {
                //deserialize the payload to get the sender name
                var sender = JsonConvert.DeserializeObject<Tuple<string, string>>(packet.Payload);

                if (sender.Item2 == MyDataHelper.currentPlayer)
                {
                    //show a message box to accept or reject the challenge
                    var dialogResult = MessageBox.Show($"{sender.Item1} has challenged you. Do you accept?", "Challenge", MessageBoxButton.YesNo);

                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        //send the accept message to the server
                        Packet280 tmp1 = new Packet280();
                        tmp1.ContentType = MessageType.Accept;
                        tmp1.Payload = sender.Item1;
                        MyDataHelper._client.SendMessage(tmp1);
                    }
                    else
                    {
                        //send the decline message to the server
                        Packet280 tmp1 = new Packet280();
                        tmp1.ContentType = MessageType.Decline;
                        tmp1.Payload = sender.Item1;
                        MyDataHelper._client.SendMessage(tmp1);
                    }

                    //reset the board
                    board = new int[3, 3];
                }
            }
            else if (packet.ContentType == MessageType.Error)
            {
                //message box to show the error message
                MessageBox.Show(packet.Payload);
            }

        }

        private void btnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (OnClose != null)
            {
                OnClose();
            }
        }

        private void btnStartup_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = (MainWindow)Application.Current.MainWindow;
            main.LoadStartupPage();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnChallenge_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
