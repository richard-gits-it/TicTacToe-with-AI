using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe280Project
{
    public class TicTacToeTournament
    {
        public List<ITicTacToePlayer> players = new List<ITicTacToePlayer>();
        public Dictionary<string, int> wins = new Dictionary<string, int>();
        public Dictionary<string, Tuple<int, int, int>> matchResults = new Dictionary<string, Tuple<int, int, int>>();

        // Method to add a player to the tournament
        public void AddPlayer(ITicTacToePlayer player)
        {
            players.Add(player);
        }

        // Method to play games among the players
        //public void Play(int numberOfGames)
        //{
        //    Random rnd = new Random();
        //    for (int i = 0; i < numberOfGames; i++)
        //    {
        //        for (int j = 0; j < players.Count; j++)
        //        {
        //            ITicTacToePlayer player1 = players[j];
        //            ITicTacToePlayer player2 = players[(j + 1) % players.Count]; // Ensure opponents are different
        //            player1.ChangeSymbol(1); // Assign symbols alternately
        //            player2.ChangeSymbol(-1); // Assign symbols alternately
        //            // Assign symbols alternately
        //            int symbol1 = (j % 2 == 0) ? 1 : -1;
        //            int symbol2 = -symbol1;

        //            GameState gameState = new GameState(player1, player2);
        //            gameState.WinnerIS += (winner) =>
        //            {
        //                string matchKey = $"{player1.Name()} vs {player2.Name()}";
        //                wins[winner.Name()]++;
        //                matchResults[matchKey] = Tuple.Create(player1 == winner ? matchResults[matchKey].Item1 + 1 : matchResults[matchKey].Item1,
        //                                                     player2 == winner ? matchResults[matchKey].Item2 + 1 : matchResults[matchKey].Item2,
        //                                                     gameState.board.Cast<int>().Count(x => x == 0) == 0 ? matchResults[matchKey].Item3 + 1 : matchResults[matchKey].Item3);
        //            };
        //            gameState.Play();
        //        }
        //    }
        //}

        // Method to play games among the players
        public void Play(int numberOfGames)
        {
            // Initialize dictionaries
            foreach (var player in players)
            {
                wins.Add(player.Name(), 0);
                foreach (var opponent in players)
                {
                    matchResults.Add($"{player.Name()} vs {opponent.Name()}", Tuple.Create(0, 0, 0));
                }
            }

            // Play games
            for (int i = 0; i < numberOfGames; i++)
            {
                foreach (var player1 in players)
                {
                    foreach (var player2 in players)
                    {
                        GameState gameState = new GameState(player1, player2);
                        gameState.WinnerIS += (winner) =>
                        {
                            string matchKey = $"{player1.Name()} vs {player2.Name()}";
                            wins[winner.Name()]++;
                            matchResults[matchKey] = Tuple.Create(player1 == winner ? matchResults[matchKey].Item1 + 1 : matchResults[matchKey].Item1,
                                                                 player2 == winner ? matchResults[matchKey].Item2 + 1 : matchResults[matchKey].Item2,
                                                                 gameState.board.Cast<int>().Count(x => x == 0) == 0 ? matchResults[matchKey].Item3 + 1 : matchResults[matchKey].Item3);
                        };
                        gameState.Play();
                    }
                }
            }

            // Print match results
            Console.WriteLine("Match Results:");
            foreach (var matchResult in matchResults)
            {
                Console.WriteLine($"{matchResult.Key}: {matchResult.Value.Item1} wins, {matchResult.Value.Item2} losses, {matchResult.Value.Item3} ties");
            }
        }
    }
}
