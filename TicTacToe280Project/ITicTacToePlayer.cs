using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe280Project
{
    public interface ITicTacToePlayer
    {
        int Symbol();
        string Name();
        Tuple<int, int> MakeMove();
        //g will show the current state of the board
        //p list of all possible moves
        void GameChanged(int[,] g,List<Tuple<int, int>> p);
        void ChangeSymbol(int s);

    }
}
