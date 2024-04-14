using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCP280Project;
using TicTacToe280Project;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.Forms.MessageBox;

namespace _280Final
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int[,] board = new int[3, 3];
        private Client client;
        private int player = 1;

        private string _opponentPlayer;
        public string OpponentPlayer
        {
            get { return _opponentPlayer; }
            set
            {
                _opponentPlayer = value;
                OnPropertyChanged(nameof(OpponentPlayer));
            }
        }

        private string currentPlayer;
        private bool isConnected = false;

        public event PropertyChangedEventHandler PropertyChanged;


        //observable collection for the list of players
        public ObservableCollection<string> Players { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            //set the data context for the list of players
            Players = new ObservableCollection<string>();
            _opponentPlayer = "";
            this.DataContext = this;

            //disable the buttons until the player is connected
            ToggleView();
        }

        private async void ConnectToServer()
        {
            if (isConnected)
            {
                MessageBox.Show("You are already connected");
                return;
            }
            client = new Client("localhost", 10000);
            client.ReceivePacket += Client_ReceivePacket;
            Packet280 tmp = new Packet280();
            tmp.ContentType = MessageType.Connected;
            tmp.Payload = txtUsername.Text;
            currentPlayer = txtUsername.Text;
            isConnected = true;
            await client.SendMessage(tmp);

        }

        private void Client_ReceivePacket(Packet280 packet)
        {
            if (packet.ContentType == MessageType.Move)
            {
                board = JsonConvert.DeserializeObject<int[,]>(packet.Payload);
                UpdateBoard(board);
                ToggleView();
            }
            else if (packet.ContentType == MessageType.Connected)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var newPlayers = JsonConvert.DeserializeObject<ObservableCollection<string>>(packet.Payload);
                    Players.Clear();
                    foreach (var player in newPlayers)
                    {
                        //add the players to the list except the current player
                        if (player != currentPlayer)
                        {
                            Players.Add(player);
                        }
                    }
                });
            }
            else if (packet.ContentType == MessageType.Disconnected)
            {
                //update players list
                Players.Remove(packet.Payload);

                Console.WriteLine("Disconnected");
            }
            else if (packet.ContentType == MessageType.Win)
            {
                MessageBox.Show("You Win");
            }
            else if (packet.ContentType == MessageType.Lose)
            {
                MessageBox.Show("You Lose");
            }
            else if (packet.ContentType == MessageType.Draw)
            {
                MessageBox.Show("Draw");
            }
            else if (packet.ContentType == MessageType.Accept)
            {
                //message box to show that the challenge is accepted
                MessageBox.Show("Challenge Accepted");

                //set the opponent player name from the payload which is a tuple of sender and receiver then enable the buttons
                Tuple<string, string> tmp = JsonConvert.DeserializeObject<Tuple<string, string>>(packet.Payload);
                OnPropertyChanged("OpponentPlayer");
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

                if (sender.Item2 == currentPlayer)
                {
                    //show a message box to accept or reject the challenge
                    DialogResult dialogResult = MessageBox.Show("Do you want to accept the challenge from " + sender.Item1, "Challenge", MessageBoxButtons.YesNo);

                    if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        //send the accept message to the server
                        Packet280 tmp1 = new Packet280();
                        tmp1.ContentType = MessageType.Accept;
                        tmp1.Payload = sender.Item1;
                        client.SendMessage(tmp1);

                        OnPropertyChanged("OpponentPlayer");
                        _opponentPlayer = sender.Item1;
                        player = 1;
                        ToggleView();
                    }
                    else
                    {
                        //send the decline message to the server
                        Packet280 tmp1 = new Packet280();
                        tmp1.ContentType = MessageType.Decline;
                        tmp1.Payload = sender.Item1;
                        client.SendMessage(tmp1);
                    }
                }
            }else if (packet.ContentType == MessageType.Error)
            {
                //message box to show the error message
                MessageBox.Show(packet.Payload);
                isConnected = false;
            }
            
        }


        private void UpdateBoard(int[,] board)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //Update the board
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (board[i, j] == 1)
                        {
                            //X
                            ((Button)FindName("btn" + i + j)).Content = "X";
                        }
                        else if (board[i, j] == -1)
                        {
                            //O
                            ((Button)FindName("btn" + i + j)).Content = "O";
                        }
                        else
                        {
                            ((Button)FindName("btn" + i + j)).Content = "";
                        }
                    }
                }
            });
        }

        private void btn00_Click(object sender, RoutedEventArgs e)
        {
            if (board == null || !isConnected)
            {
                return;
            }
            Button clickedButton = sender as Button;

            // Extract row and column indices from the button's name
            string[] buttonNameParts = clickedButton.Name.Split(new char[] { 'n' });
            int row = int.Parse(buttonNameParts[1][0].ToString());
            int col = int.Parse(buttonNameParts[1][1].ToString());

            Tuple<int, int> move = new Tuple<int, int>(row, col);
            //check if the move is valid  and update the board
            if (client.CheckAvailableMoves().Contains(move))
            {
                board[row, col] = player;

                //tuple for board and opponent player
                Tuple < int[,], string> tmp1 = new Tuple<int[,], string>(board, _opponentPlayer);

                UpdateBoard(board);
                //send the move to the server
                Packet280 tmp = new Packet280();
                tmp.ContentType = MessageType.Move;
                tmp.Payload = JsonConvert.SerializeObject(tmp1);
                client.SendMessage(tmp);
            }
            else
            {
                MessageBox.Show("Invalid Move");
            }

            ToggleView();

        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
        }

        private void btnChallenge_Click(object sender, RoutedEventArgs e)
        {
            //send a challenge to the selected player from the listbox
            if (client != null)
            {
                if (lstPlayers.SelectedItem != null)
                {
                    Packet280 tmp = new Packet280();
                    tmp.ContentType = MessageType.Invite;
                    tmp.Payload = lstPlayers.SelectedItem.ToString();
                    client.SendMessage(tmp);
                }
            }
        }

        private void ToggleView()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //toggle the view of the buttons
                foreach (var child in gameGrid.Children)
                {
                    if (child is Button)
                    {
                        ((Button)child).IsEnabled = !((Button)child).IsEnabled;
                    }
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
