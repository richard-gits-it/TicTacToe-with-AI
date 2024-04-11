using ClientProject;
using Human;
using System;
using System.Collections.Generic;
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
using TCP280Project;
using TicTacToe280Project;

namespace _280Final
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameState gs;
        private Client client;
        public MainWindow()
        {
            InitializeComponent();
            //foreach (var child in gameGrid.Children)
            //{
            //    if (child is Button)
            //    {
            //        ((Button)child).IsEnabled = false;
            //    }
            //}
        }

        private async void ConnectToServer()
        {
            client = new Client("localhost", 10000);
            client.ReceiveGameStateEvent += Client_ReceivePacket;
            Packet280 tmp = new Packet280();
            tmp.ContentType = MessageType.Connected;
            tmp.Payload = "Hello Server!";
            await client.SendMessage(tmp);

            ITicTacToePlayer p1 = new HumanPlayer(1);
            ITicTacToePlayer p2 = new HumanPlayer(-1);
            gs = new GameState(p1, p2);
            UpdateBoard(gs.board);

        }

        private void Client_ReceivePacket(GameState state)
        {
            gs = state;
            UpdateBoard(gs.board);
            foreach (var child in gameGrid.Children)
            {
                if (child is Button)
                {
                    ((Button)child).IsEnabled = true;
                }
            }
        }

        //private void btnPlay_Click(object sender, RoutedEventArgs e)
        //{
        //    ITicTacToePlayer p1 = null;
        //    switch (cmbp1.SelectedIndex)
        //    {
        //        case 0:
        //            p1 = new EasyPlayer(1);
        //            break;
        //        case 1:
        //            p1 = new ModeratePlayer(1);
        //            break;
        //        case 2:
        //            p1 = new DifficultPlayer(1);
        //            break;
        //    }
        //    ITicTacToePlayer p2 = null;
        //    switch (cmbp2.SelectedIndex)
        //    {
        //        case 0:
        //            p2 = new EasyPlayer(-1);
        //            break;
        //        case 1:
        //            p2 = new ModeratePlayer(-1);
        //            break;
        //        case 2:
        //            p2 = new DifficultPlayer(-1);
        //            break;
        //    }

        //    gs = new GameState(p1, p2);
        //    gs.Play();
        //    UpdateBoard(gs.board);
        //    if (gs.Winner() != null)
        //    {
        //        MessageBox.Show(gs.Winner().Name() + " Wins");
        //    }
        //    else
        //    {
        //        MessageBox.Show("Draw");
        //    }
        //}

        private void UpdateBoard(int[,] board)
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
        }

        //private void btnPlay2_Click(object sender, RoutedEventArgs e)
        //{
        //    ITicTacToePlayer p1 = null;
        //    ITicTacToePlayer p2 = new HumanPlayer(1);
        //    switch (cmbp3.SelectedIndex)
        //    {
        //        case 0:
        //            p1 = new EasyPlayer(-1);
        //            break;
        //        case 1:
        //            p1 = new ModeratePlayer(-1);
        //            break;
        //        case 2:
        //            p1 = new DifficultPlayer(-1);
        //            break;
        //    }

        //    gs = new GameState(p1, p2);
        //    gs.PlayOpponentTurn();
        //    UpdateBoard(gs.board);
        //    foreach (var child in gameGrid.Children)
        //    {
        //        if (child is Button)
        //        {
        //            ((Button)child).IsEnabled = true;
        //        }
        //    }
        //}

        private void btn00_Click(object sender, RoutedEventArgs e)
        {
            if (gs == null)
            {
                return;
            }
            Button clickedButton = sender as Button;

            // Extract row and column indices from the button's name
            string[] buttonNameParts = clickedButton.Name.Split(new char[] { 'n' });
            int row = int.Parse(buttonNameParts[1][0].ToString());
            int col = int.Parse(buttonNameParts[1][1].ToString());

            gs.PlayMyTurn(Tuple.Create(row, col));
            UpdateBoard(gs.board);
            gs.CheckForWinner();
            if (gs.Winner() != null)
            {
                MessageBox.Show(gs.Winner().Name() + " Wins");
                // Disable all buttons
                foreach (var child in gameGrid.Children)
                {
                    if (child is Button)
                    {
                        ((Button)child).IsEnabled = false;
                    }
                }
            }
            else if (gs.GetAvailableMoves().Count == 0)
            {
                MessageBox.Show("Draw");
                // Disable all buttons
                foreach (var child in gameGrid.Children)
                {
                    if (child is Button)
                    {
                        ((Button)child).IsEnabled = false;
                    }
                }
            }

            // Send the game state to the server
            client.SendGameState(gs);

            //disable the buttons until the server sends the game state back
            foreach (var child in gameGrid.Children)
            {
                if (child is Button)
                {
                    ((Button)child).IsEnabled = false;
                }
            }

        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
        }
    }
}
