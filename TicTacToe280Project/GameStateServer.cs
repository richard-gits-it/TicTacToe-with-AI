using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe280Project
{
    public class GameStateServer
    {
        public int[,] board = new int[3, 3];

        public void CheckForWinner()
        {
            // Check rows
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] != 0 && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
                {
                    Console.WriteLine("Winner is " + board[i, 0]);
                    return;
                }
            }

            // Check columns
            for (int i = 0; i < 3; i++)
            {
                if (board[0, i] != 0 && board[0, i] == board[1, i] && board[1, i] == board[2, i])
                {
                    Console.WriteLine("Winner is " + board[0, i]);
                    return;
                }
            }

            // Check diagonals
            if (board[0, 0] != 0 && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
            {
                Console.WriteLine("Winner is " + board[0, 0]);
                return;
            }

            if (board[0, 2] != 0 && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
            {
                Console.WriteLine("Winner is " + board[0, 2]);
                return;
            }

            // Check for a draw
            bool draw = true;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == 0)
                    {
                        draw = false;
                        break;
                    }
                }
            }

            if (draw)
            {
                Console.WriteLine("It's a draw!");
            }
        }
    }
}
