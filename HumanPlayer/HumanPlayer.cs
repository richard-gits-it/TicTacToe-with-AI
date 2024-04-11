using TicTacToe280Project;

namespace Human
{
    public class HumanPlayer : TicTacToeBase
    {
        public HumanPlayer(int symbol) : base(symbol)
        {
            this.playerName = "Human Player";
        }
    }
}