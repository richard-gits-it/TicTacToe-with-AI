using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TicTacToe280Project;

namespace TicTacToe280Project
{
    public class GameState
    {
        private ITicTacToePlayer winner = null;
        public delegate void DeclareWinner(ITicTacToePlayer player);
        public event DeclareWinner WinnerIS;

        public int[,] board = new int[3, 3];
        List<Tuple<int, int>> availableMoves = new List<Tuple<int, int>>();
        Queue<ITicTacToePlayer> players = new Queue<ITicTacToePlayer>();

        // Serialize GameState object to JSON
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        // Deserialize JSON to GameState object
        public static GameState DeserializeFromJson(string json)
        {
            return JsonConvert.DeserializeObject<GameState>(json);
        }


        public GameState(ITicTacToePlayer p1, ITicTacToePlayer p2)
        {
            int[] tmp = { p1.Symbol(), p2.Symbol() };
            //Need to make sure both 1, and -1 is set as the symbol, or the game wont work
            if (!tmp.Contains(1) && !tmp.Contains(-1))
            {
                throw new Exception("Game must be started with players symbols 1 and -1");
            }

            int symbol1 = p1.Symbol();
            int symbol2 = p2.Symbol();

            // Ensure different symbols for self-play
            if (symbol1 == symbol2)
            {
                symbol2 = -symbol1;
                p2.ChangeSymbol(symbol2); // Update player2's symbol
            }

            players.Enqueue(p1);
            players.Enqueue(p2);

            // Set up the available moves
            for (int i = 0; i < 3; i++)
            {
                Tuple<int, int> m1 = new Tuple<int, int>(i, 0);
                Tuple<int, int> m2 = new Tuple<int, int>(i, 1);
                Tuple<int, int> m3 = new Tuple<int, int>(i, 2);
                availableMoves.Add(m1);
                availableMoves.Add(m2);
                availableMoves.Add(m3);
            }
        }

        public void CheckForWinner()
        {

            WinnerIS += GameState_WinnerIS;
            for (int i = 0; i < 3; i++)
            {
                //[0,0][0,1][0,2]
                //[1,0][1,1][1,2]
                //[2,0][2,1][2,2]
                if (Math.Abs(board[i, 0] + board[i, 1] + board[i, 2]) == 3)
                {
                    WinnerIS(players.Where(x => x.Symbol() == board[i, 0]).FirstOrDefault());
                    return;
                }
                else if (Math.Abs(board[0, i] + board[1, i] + board[2, i]) == 3)
                {
                    WinnerIS(players.Where(x => x.Symbol() == board[0, i]).FirstOrDefault());
                    return;
                }
            }
            if (Math.Abs(board[0, 0] + board[1, 1] + board[2, 2]) == 3)
            {
                WinnerIS(players.Where(x => x.Symbol() == board[1, 1]).FirstOrDefault());
                return;
            }
            else if (Math.Abs(board[2, 0] + board[1, 1] + board[0, 2]) == 3)
            {
                WinnerIS(players.Where(x => x.Symbol() == board[1, 1]).FirstOrDefault());
                return;
            }

        }

        private void GameState_WinnerIS(ITicTacToePlayer player)
        {
            winner = player;
        }

        public List<Tuple<int, int>> GetAvailableMoves()
        {
            return this.availableMoves;
        }

        public ITicTacToePlayer Winner()
        {
            return this.winner;
        }
        public void Play()
        {
            while (availableMoves.Count > 0 && winner == null)
            {
                //Create a variable to hold the move that will be made
                Tuple<int, int> move;
                //Get the players who currently has their turn
                ITicTacToePlayer p = players.Dequeue();
                //Run a task where the players class will come up with some move
                //Set a timeout
                var task = Task.Run(() => p.MakeMove());
                if (task.Wait(TimeSpan.FromSeconds(5)))
                {
                    //When the tuple gets returned
                    move = task.Result;
                }
                else
                {
                    //Get the first move form the list
                    move = availableMoves.FirstOrDefault();
                }
                //Check and make sure the move the players send us, is in the list of available
                if (availableMoves.Contains(move))
                {
                    //Set the board position at x,y to the players symbol
                    this.board[move.Item1, move.Item2] = p.Symbol();
                    //remove it to update the remaining
                    this.availableMoves.Remove(move);
                }
                else
                {
                    move = availableMoves.FirstOrDefault();
                    this.board[move.Item1, move.Item2] = p.Symbol();
                    //remove it to update the remaining
                    this.availableMoves.Remove(move);
                }
                players.Enqueue(p);
                //Update both players
                UpdateBothPlayers();
                //Check for winner
                CheckForWinner();
            }
        }
        public void PlayOpponentTurn()
        {
            if (winner != null)
                return;
            UpdateBothPlayers();
            ITicTacToePlayer p = players.Dequeue();
            Tuple<int, int> move = p.MakeMove();
            if (availableMoves.Contains(move))
            {
                this.board[move.Item1, move.Item2] = p.Symbol();
                this.availableMoves.Remove(move);
            }
            players.Enqueue(p);
            UpdateBothPlayers();
            CheckForWinner();
        }
        public void PlayHumanTurn(Tuple<int, int> move)
        {
            if (winner != null)
                return;

            ITicTacToePlayer p = players.Dequeue();
            if (availableMoves.Contains(move))
            {
                this.board[move.Item1, move.Item2] = p.Symbol();
                this.availableMoves.Remove(move);
            }
            players.Enqueue(p);
            UpdateBothPlayers();
            CheckForWinner();

            PlayOpponentTurn();
        }

        public void PlayMyTurn(Tuple<int, int> move)
        {
            if (winner != null)
                return;

            ITicTacToePlayer p = players.Dequeue();
            if (availableMoves.Contains(move))
            {
                this.board[move.Item1, move.Item2] = p.Symbol();
                this.availableMoves.Remove(move);
            }
            players.Enqueue(p);
            UpdateBothPlayers();
            CheckForWinner();
        }
        public void UpdateBothPlayers()
        {
            ITicTacToePlayer player = players.Dequeue();
            ITicTacToePlayer player2 = players.Dequeue();
            //If this.board if a reference, player can potentially change the game
            player.GameChanged(this.board, this.availableMoves);
            player2.GameChanged(this.board, this.availableMoves);
            players.Enqueue(player);
            players.Enqueue(player2);
        }
    }
}
