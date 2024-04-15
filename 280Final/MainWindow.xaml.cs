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

        #region Properties and Fields
        //field for opponent score
        private int _opponentScore;
        public int OpponentScore
        {
            get { return _opponentScore; }
            set
            {
                _opponentScore = value;
                OnPropertyChanged(nameof(OpponentScore));
            }
        }
        //field for player score
        private int _playerScore;
        public int PlayerScore
        {
            get { return _playerScore; }
            set
            {
                _playerScore = value;
                OnPropertyChanged(nameof(PlayerScore));
            }
        }
        //field for opponent player symbol
        private string _opponentPlayerSymbol;
        public string OpponentPlayerSymbol
        {
            get { return _opponentPlayerSymbol; }
            set
            {
                _opponentPlayerSymbol = value;
                OnPropertyChanged(nameof(OpponentPlayerSymbol));
            }
        }
        //field for player symbol
        private string _playerSymbol;
        public string PlayerSymbol
        {
            get { return _playerSymbol; }
            set
            {
                _playerSymbol = value;
                OnPropertyChanged(nameof(PlayerSymbol));
            }
        }


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
        public string CurrentPlayer
        {
            get { return currentPlayer; }
            set
            {
                currentPlayer = value;
                OnPropertyChanged(nameof(CurrentPlayer));
            }
        }
        //field for tie score
        private int _tieScore;
        public int TieScore
        {
            get { return _tieScore; }
            set
            {
                _tieScore = value;
                OnPropertyChanged(nameof(TieScore));
            }
        }

        //field for who's turn with name of player
        private string _turn;
        public string Turn
        {
            get { return _turn; }
            set
            {
                _turn = value;
                OnPropertyChanged(nameof(Turn));
            }
        }

        #endregion

        private bool isConnected = false;

        private bool isTurn = true;

        public event PropertyChangedEventHandler PropertyChanged;


        //observable collection for the list of players
        public ObservableCollection<string> Players { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            //set the data context for the list of players
            Players = new ObservableCollection<string>();
            _opponentPlayer = "";
            currentPlayer = "";
            PlayerScore = 0;
            OpponentScore = 0;
            TieScore = 0;
            Turn = "";
            btnLeaveGame.IsEnabled = false;
            this.DataContext = this;

            //disable the buttons until the player is connected
            EnableBoard(false);
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
            OnPropertyChanged("CurrentPlayer");
            stkUsername.IsEnabled = false;
            isConnected = true;
            await client.SendMessage(tmp);

        }

        private void Client_ReceivePacket(Packet280 packet)
        {
            if (packet.ContentType == MessageType.Move)
            {
                isTurn = true;
                board = JsonConvert.DeserializeObject<int[,]>(packet.Payload);
                UpdateBoard(board);
                EnableBoard(true);

                //update turn
                Turn = $"It's your turn.";
                OnPropertyChanged("Turn");
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

                Console.WriteLine("Disconnected");
            }
            else if (packet.ContentType == MessageType.Win)
            {
                //create new board and add win to the player score
                board = JsonConvert.DeserializeObject<int[,]>(packet.Payload);

                //add the score to the player
                PlayerScore++;
                OnPropertyChanged("PlayerScore");
                UpdateBoard(board);

                NextRound();

                //message box to show that the player wins and disable the buttons to prevent further moves
                MessageBox.Show("You Win");
            }
            else if (packet.ContentType == MessageType.Lose)
            {
                board = JsonConvert.DeserializeObject<int[,]>(packet.Payload);
                //add the score to the opponent
                OpponentScore++;
                OnPropertyChanged("OpponentScore");
                UpdateBoard(board);

                NextRound();

                MessageBox.Show("You Lose");

            }
            else if (packet.ContentType == MessageType.Draw)
            {
                board = JsonConvert.DeserializeObject<int[,]>(packet.Payload);
                TieScore++;
                OnPropertyChanged("TieScore");
                UpdateBoard(board);

                NextRound();

                MessageBox.Show("Draw");
            }
            else if (packet.ContentType == MessageType.Accept)
            {
                isTurn = false;

                //reset the board
                board = new int[3, 3];
                //reset player score
                PlayerScore = 0;
                //reset opponent score
                OpponentScore = 0;
                UpdateBoard(board);

                //message box to show that the challenge is accepted
                MessageBox.Show("Challenge Accepted");

                //set the opponent player name from the payload which is a tuple of sender and receiver then enable the buttons
                Tuple<string, string> tmp = JsonConvert.DeserializeObject<Tuple<string, string>>(packet.Payload);
                OnPropertyChanged("OpponentPlayer");
                _opponentPlayer = tmp.Item1;
                player = -1;
                //update player and opponent symbols
                PlayerSymbol = "O";
                OpponentPlayerSymbol = "X";
                OnPropertyChanged("PlayerSymbol");
                OnPropertyChanged("OpponentPlayerSymbol");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //enable leave game button
                    btnLeaveGame.IsEnabled = true;

                    //enable stkPlayers
                    stkPlayers.IsEnabled = false;

                });

                //update turn
                Turn = $"It's {_opponentPlayer}'s turn.";
                OnPropertyChanged("Turn");

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
                        isTurn = true;
                        //update player and opponent symbols
                        PlayerSymbol = "X";
                        OpponentPlayerSymbol = "O";
                        OnPropertyChanged("PlayerSymbol");
                        OnPropertyChanged("OpponentPlayerSymbol");

                        EnableBoard(true);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            //enable leave game button
                            btnLeaveGame.IsEnabled = true;

                            //enable stkPlayers
                            stkPlayers.IsEnabled = false;

                        });

                        //update turn
                        Turn = $"It's your turn.";
                        OnPropertyChanged("Turn");
                    }
                    else
                    {
                        //send the decline message to the server
                        Packet280 tmp1 = new Packet280();
                        tmp1.ContentType = MessageType.Decline;
                        tmp1.Payload = sender.Item1;
                        client.SendMessage(tmp1);
                    }

                    //reset the board
                    board = new int[3, 3];
                    UpdateBoard(board);
                }
            }else if (packet.ContentType == MessageType.Error)
            {
                //message box to show the error message
                Application.Current.Dispatcher.Invoke(() =>
                {
                    stkUsername.IsEnabled = true;
                    MessageBox.Show(packet.Payload);
                });
                isConnected = false;

            }else if (packet.ContentType == MessageType.Leave)
            {
                //message box to show that the opponent has left the game
                MessageBox.Show($"{packet.Payload}");

                //reset the board
                board = new int[3, 3];
                UpdateBoard(board);
                //reset player score
                PlayerScore = 0;
                //reset opponent score
                OpponentScore = 0;
                OnPropertyChanged("PlayerScore");
                OnPropertyChanged("OpponentScore");
                //reset tie score
                TieScore = 0;
                OnPropertyChanged("TieScore");
                //reset the opponent player name
                _opponentPlayer = "";
                OnPropertyChanged("OpponentPlayer");
                //reset the player and opponent symbols
                PlayerSymbol = "";
                OpponentPlayerSymbol = "";
                OnPropertyChanged("PlayerSymbol");
                OnPropertyChanged("OpponentPlayerSymbol");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //enable leave game button
                    btnLeaveGame.IsEnabled = false;

                    //enable stkPlayers
                    stkPlayers.IsEnabled = true;

                });

                //reset the turn
                Turn = "";
                OnPropertyChanged("Turn");

                EnableBoard(false);
            }

        }

        private void NextRound()
        {
            player = -player;
            if (player > 0)
            {
                PlayerSymbol = "X";
                OpponentPlayerSymbol = "O";
                isTurn = true;
                //update turn
                Turn = $"It's your turn.";
                EnableBoard(true);
            }
            else
            {
                PlayerSymbol = "O";
                OpponentPlayerSymbol = "X";
                isTurn = false;
                //update turn
                Turn = $"It's {_opponentPlayer}'s turn.";
                EnableBoard(false);
            }
            OnPropertyChanged("PlayerSymbol");
            OnPropertyChanged("OpponentPlayerSymbol");
            OnPropertyChanged("Turn");
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
                isTurn = false;
                UpdateBoard(board);
                //send the move to the server
                Packet280 tmp = new Packet280();
                tmp.ContentType = MessageType.Move;
                tmp.Payload = JsonConvert.SerializeObject(tmp1);
                client.SendMessage(tmp);
                EnableBoard(false);

                //update turn
                Turn = $"It's {_opponentPlayer}'s turn.";
                OnPropertyChanged("Turn");
            }
            else
            {
                MessageBox.Show("Invalid Move");
            }


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


        private void EnableBoard(bool enable)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //toggle the view of the buttons
                foreach (var child in gameGrid.Children)
                {
                    if (child is Button)
                    {
                        ((Button)child).IsEnabled = enable;
                    }
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnLeaveGame_Click(object sender, RoutedEventArgs e)
        {
            //messsage box to confirm if the player wants to leave the game
            DialogResult dialogResult = MessageBox.Show("Do you want to leave the game?", "Leave Game", MessageBoxButtons.YesNo);

            //if the player wants to leave the game, send the leave message to the server
            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                Packet280 tmp = new Packet280();
                tmp.ContentType = MessageType.Leave;
                tmp.Payload = _opponentPlayer;
                client.SendMessage(tmp);
                //reset the board
                board = new int[3, 3];
                UpdateBoard(board);
                //reset player score
                PlayerScore = 0;
                //reset opponent score
                OpponentScore = 0;
                OnPropertyChanged("PlayerScore");
                OnPropertyChanged("OpponentScore");
                //reset tie score
                TieScore = 0;
                OnPropertyChanged("TieScore");
                //reset the opponent player name
                _opponentPlayer = "";
                OnPropertyChanged("OpponentPlayer");
                //reset the player and opponent symbols
                PlayerSymbol = "";
                OpponentPlayerSymbol = "";
                OnPropertyChanged("PlayerSymbol");
                OnPropertyChanged("OpponentPlayerSymbol");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //enable leave game button
                    btnLeaveGame.IsEnabled = false;

                    //enable stkPlayers
                    stkPlayers.IsEnabled = true;

                });

                //reset the turn
                Turn = "";
                OnPropertyChanged("Turn");
                EnableBoard(false);

            }
            else
            {
                return;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            client.DisconnectClient();
        }
    }
}
