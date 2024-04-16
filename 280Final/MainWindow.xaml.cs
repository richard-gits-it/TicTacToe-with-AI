using Difficult;
using Easy;
using Human;
using Moderate;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Serialization;
using TCP280Project;
using TicTacToe280Project;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;

namespace _280Final
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private GameState gs;
        private bool isPlayingAI = false;

        private int[,] board = new int[3, 3];
        private Client client;
        private int player = 1;

        #region Properties and Fields
        //field for connection status
        private string _connectionStatus;
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

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

        public bool isConnected = false;

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
            ConnectionStatus = "TicTacToe - Offline";
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

            //check if client is already instantiated
            if (client == null)
            {
                client = new Client("localhost", 10000);
                client.ReceivePacket += Client_ReceivePacket;
            }

            //check if client is connected before sending a message
            if (!client._client.Connected)
            {
                MessageBox.Show("Error connecting to server.");
                return;
            }

            Packet280 tmp = new Packet280();
            tmp.ContentType = MessageType.Connected;
            tmp.Payload = txtUsername.Text;
            currentPlayer = txtUsername.Text;
            OnPropertyChanged("CurrentPlayer");
            stkUsername.IsEnabled = false;
            isConnected = true;
            ConnectionStatus = "TicTacToe - Online";
            OnPropertyChanged("ConnectionStatus");

            await client.SendMessage(tmp);
        }

        private void Client_ReceivePacket(Packet280 packet)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (packet.ContentType)
                {
                    case MessageType.Move:
                        HandleMovePacket(packet);
                        break;
                    case MessageType.Connected:
                    case MessageType.Disconnected:
                        HandlePlayerListPacket(packet);
                        break;
                    case MessageType.Win:
                        HandleWinPacket(packet);
                        break;
                    case MessageType.Lose:
                        HandleLosePacket(packet);
                        break;
                    case MessageType.Draw:
                        HandleDrawPacket(packet);
                        break;
                    case MessageType.Accept:
                        HandleAcceptPacket(packet);
                        break;
                    case MessageType.Decline:
                        MessageBox.Show(packet.Payload);
                        break;
                    case MessageType.Invite:
                        HandleInvitePacket(packet);
                        break;
                    case MessageType.Error:
                        HandleErrorPacket(packet);
                        break;
                    case MessageType.Leave:
                        HandleLeavePacket(packet);
                        break;
                }
            });
        }

        #region handle packets
        private void HandleMovePacket(Packet280 packet)
        {
            isTurn = true;
            board = JsonConvert.DeserializeObject<int[,]>(packet.Payload);
            UpdateBoard(board);
            EnableBoard(true);
            Turn = $"It's your turn.";
            OnPropertyChanged("Turn");
        }

        private void HandlePlayerListPacket(Packet280 packet)
        {
            var newPlayers = JsonConvert.DeserializeObject<ObservableCollection<string>>(packet.Payload);
            Players.Clear();
            foreach (var player in newPlayers)
            {
                if (player != currentPlayer)
                {
                    Players.Add(player);
                }
            }
            if (packet.ContentType == MessageType.Disconnected)
            {
                Console.WriteLine("Disconnected");
            }
        }

        private void HandleWinPacket(Packet280 packet)
        {
            board = JsonConvert.DeserializeObject<int[,]>(packet.Payload);
            PlayerScore++;
            OnPropertyChanged("PlayerScore");
            UpdateBoard(board);
            NextRound();
            MessageBox.Show("You Win");
        }

        private void HandleLosePacket(Packet280 packet)
        {
            board = JsonConvert.DeserializeObject<int[,]>(packet.Payload);
            OpponentScore++;
            OnPropertyChanged("OpponentScore");
            UpdateBoard(board);
            NextRound();
            MessageBox.Show("You Lose");
        }

        private void HandleDrawPacket(Packet280 packet)
        {
            board = JsonConvert.DeserializeObject<int[,]>(packet.Payload);
            TieScore++;
            OnPropertyChanged("TieScore");
            UpdateBoard(board);
            NextRound();
            MessageBox.Show("Draw");
        }

        private void HandleAcceptPacket(Packet280 packet)
        {
            isTurn = false;
            board = new int[3, 3];
            PlayerScore = 0;
            OpponentScore = 0;
            UpdateBoard(board);
            MessageBox.Show("Challenge Accepted");
            var tmp = JsonConvert.DeserializeObject<Tuple<string, string>>(packet.Payload);
            _opponentPlayer = tmp.Item1;
            OnPropertyChanged("OpponentPlayer");
            player = -1;
            PlayerSymbol = "O";
            OpponentPlayerSymbol = "X";
            OnPropertyChanged("PlayerSymbol");
            OnPropertyChanged("OpponentPlayerSymbol");
            btnLeaveGame.IsEnabled = true;
            stkPlayers.IsEnabled = false;
            Turn = $"It's {_opponentPlayer}'s turn.";
            OnPropertyChanged("Turn");
        }

        private void HandleInvitePacket(Packet280 packet)
        {
            var sender = JsonConvert.DeserializeObject<Tuple<string, string>>(packet.Payload);
            if (sender.Item2 == currentPlayer)
            {
                var dialogResult = MessageBox.Show($"{sender.Item1} has challenged you to a game. Do you accept?", "Challenge", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    Packet280 tmp1 = new Packet280();
                    tmp1.ContentType = MessageType.Accept;
                    tmp1.Payload = sender.Item1;
                    client.SendMessage(tmp1);
                    _opponentPlayer = sender.Item1;
                    OnPropertyChanged("OpponentPlayer");
                    player = 1;
                    isTurn = true;
                    PlayerSymbol = "X";
                    OpponentPlayerSymbol = "O";
                    OnPropertyChanged("PlayerSymbol");
                    OnPropertyChanged("OpponentPlayerSymbol");
                    EnableBoard(true);
                    btnLeaveGame.IsEnabled = true;
                    stkPlayers.IsEnabled = false;
                    Turn = $"It's your turn.";
                    OnPropertyChanged("Turn");
                }
                else
                {
                    Packet280 tmp1 = new Packet280();
                    tmp1.ContentType = MessageType.Decline;
                    tmp1.Payload = sender.Item1;
                    client.SendMessage(tmp1);
                }
                board = new int[3, 3];
                UpdateBoard(board);
            }
        }

        private void HandleErrorPacket(Packet280 packet)
        {
            stkUsername.IsEnabled = true;
            isConnected = false;
            ConnectionStatus = "TicTacToe - Offline";
            OnPropertyChanged("ConnectionStatus");
            //if payload is "Server has stopped", close the client
            if (packet.Payload == "Server has stopped")
            {
                client.DisconnectClient();
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

                //clear players list
                Players.Clear();
            }
            MessageBox.Show(packet.Payload);
        }

        private void HandleLeavePacket(Packet280 packet)
        {
            MessageBox.Show(packet.Payload);
            board = new int[3, 3];
            UpdateBoard(board);
            PlayerScore = 0;
            OpponentScore = 0;
            OnPropertyChanged("PlayerScore");
            OnPropertyChanged("OpponentScore");
            TieScore = 0;
            OnPropertyChanged("TieScore");
            _opponentPlayer = "";
            OnPropertyChanged("OpponentPlayer");
            PlayerSymbol = "";
            OpponentPlayerSymbol = "";
            OnPropertyChanged("PlayerSymbol");
            OnPropertyChanged("OpponentPlayerSymbol");
            btnLeaveGame.IsEnabled = false;
            stkPlayers.IsEnabled = true;
            Turn = "";
            OnPropertyChanged("Turn");
            EnableBoard(false);
        }

        #endregion

        #region methods
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
        #endregion

        #region events
        private void btn00_Click(object sender, RoutedEventArgs e)
        {
            if (board == null)
            {
                return;
            }
            Button clickedButton = sender as Button;

            // Extract row and column indices from the button's name
            string[] buttonNameParts = clickedButton.Name.Split(new char[] { 'n' });
            int row = int.Parse(buttonNameParts[1][0].ToString());
            int col = int.Parse(buttonNameParts[1][1].ToString());

            Tuple<int, int> move = new Tuple<int, int>(row, col);
            if (isPlayingAI)
            {
                PlayAgainstAI(move);
            }
            else if(isConnected && !isPlayingAI)
                PlayAgainstPlayer(row, col, move);

        }

        private async void PlayAgainstAI(Tuple<int, int> move)
        {
            gs.PlayHumanTurn(move);
            UpdateBoard(gs.board);
            Turn = $"It's the AI's turn.";
            OnPropertyChanged("Turn");

            //disable the board
            EnableBoard(false);
            await Task.Delay(TimeSpan.FromSeconds(2));

            gs.PlayOpponentTurn();
            UpdateBoard(gs.board);
            Turn = $"It's your turn.";
            OnPropertyChanged("Turn");
            EnableBoard(true);

            gs.CheckForWinner();
            if (gs.Winner() != null)
            {
                if (gs.Winner().Symbol() == player)
                {
                    MessageBox.Show("You Win");
                    PlayerScore++;
                    OnPropertyChanged("PlayerScore");
                }
                else
                {
                    MessageBox.Show("You Lose");
                    OpponentScore++;
                    OnPropertyChanged("OpponentScore");
                }

                if (player == -1)
                {
                    StartAIGame(true);
                }
                else
                {
                    StartAIGame(false);
                }


            }
            else if (gs.GetAvailableMoves().Count == 0)
            {
                MessageBox.Show("Draw");
                TieScore++;
                OnPropertyChanged("TieScore");
                if (player == -1)
                {
                    StartAIGame(true);
                }
                else
                {
                    StartAIGame(false);
                }
            }
        }

        private void PlayAgainstPlayer(int row, int col, Tuple<int, int> move)
        {
            //check if the move is valid  and update the board
            if (client.CheckAvailableMoves().Contains(move))
            {
                board[row, col] = player;

                //tuple for board and opponent player
                Tuple<int[,], string> tmp1 = new Tuple<int[,], string>(board, _opponentPlayer);
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

        private void btnLeaveGame_Click(object sender, RoutedEventArgs e)
        {
            //messsage box to confirm if the player wants to leave the game
            var dialogResult = MessageBox.Show("Do you want to leave the game?", "Leave Game", MessageBoxButton.YesNo);

            //if the player wants to leave the game, send the leave message to the server
            if (dialogResult == MessageBoxResult.Yes)
            {
                try
                {
                    if(isPlayingAI)
                    {
                        if (client != null)
                        {
                            isPlayingAI = false;
                            Packet280 tmp = new Packet280();
                            tmp.ContentType = MessageType.AI;
                            tmp.Payload = "Stop";
                            client.SendMessage(tmp);
                        }
                    }
                    else
                    {

                        Packet280 tmp = new Packet280();
                        tmp.ContentType = MessageType.Leave;
                        tmp.Payload = _opponentPlayer;
                        client.SendMessage(tmp);
                    }

                }
                catch
                {
                    throw;
                }

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

                    //enable stkUser
                    stkUser.IsEnabled = true;

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
        private void btnPlay2_Click(object sender, RoutedEventArgs e)
        {
            StartAIGame(true);

            //disable stkUser
            stkUser.IsEnabled = false;
            btnLeaveGame.IsEnabled = true;

            isPlayingAI = true;

            if (client != null)
            {
                //send packet to server that the player is playing against AI
                Packet280 tmp = new Packet280();
                tmp.ContentType = MessageType.AI;
                tmp.Payload = "Start";
                client.SendMessage(tmp);

            }

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (client != null)
                client.DisconnectClient();
        }
        #endregion

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private async void StartAIGame(bool first)
        {
            ITicTacToePlayer p1 = null;
            ITicTacToePlayer p2 = null;
            if (first)
            {
                player = 1;
                p1 = new HumanPlayer(player);
            }
            else{
                player = -1;
                p2 = new HumanPlayer(player);
            }

            string pc = "";
            switch (cmbp3.SelectedIndex)
            {
                case 0:
                    if (first)
                        p2 = new EasyPlayer(-player);
                    else
                        p1 = new EasyPlayer(-player);
                    pc = "Easy";
                    break;
                case 1:
                    if (first)
                        p2 = new ModeratePlayer(-player);
                    else
                        p1 = new ModeratePlayer(-player);
                    pc = "Moderate";
                    break;
                case 2:
                    if (first)
                        p2 = new DifficultPlayer(-player);
                    else
                        p1 = new DifficultPlayer(-player);
                    pc = "Difficult";
                    break;
            }

            gs = new GameState(p1, p2);
            UpdateBoard(gs.board);

            if (first)
            {
                Turn = $"It's your turn.";
                //symbol for player
                PlayerSymbol = "X";
                OnPropertyChanged("PlayerSymbol");
                //symbol for AI
                OpponentPlayerSymbol = "O";
                OnPropertyChanged("OpponentPlayerSymbol");
                EnableBoard(true);

            }
            else
            {
                EnableBoard(false);

                PlayerSymbol = "O";
                OnPropertyChanged("PlayerSymbol");
                OpponentPlayerSymbol = "X";
                OnPropertyChanged("OpponentPlayerSymbol");

                Turn = $"It's the AI's turn.";
                OnPropertyChanged("Turn");

                await Task.Delay(TimeSpan.FromSeconds(1));

                gs.PlayOpponentTurn();
                UpdateBoard(gs.board);

                Turn = $"It's your turn.";
                OnPropertyChanged("Turn");
                EnableBoard(true);
            }

            //set opponent player name
            _opponentPlayer = pc;
            OnPropertyChanged("OpponentPlayer");

        }
    }
}
