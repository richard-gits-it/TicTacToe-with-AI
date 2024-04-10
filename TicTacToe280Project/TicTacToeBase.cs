namespace TicTacToe280Project
{
    public class TicTacToeBase : ITicTacToePlayer
    {
        protected Random rnd = new Random();
        protected int[,] board;
        protected string playerName;
        protected int symbol;
        protected List<Tuple<int, int>> availableMoves = new List<Tuple<int, int>>();
        public TicTacToeBase(int symbol)
        {
            this.symbol = symbol;
            //Name our player
            this.playerName = "BasePlayer";
        }
        public void ChangeSymbol(int s)
        {
            this.symbol = s;
        }
        public void GameChanged(int[,] g, List<Tuple<int, int>> p)
        {
            //get updated board state
            this.board = g;
            //Give the player an updated list of moves
            this.availableMoves = p;
        }

        public string Name()
        {
            return this.playerName;
        }

        public virtual Tuple<int, int> MakeMove()
        {
            //CHeck and see if any moves are left
            if(availableMoves.Count > 0)
            {
                //Choose a random move (unlikely to win)
                return availableMoves[rnd.Next(0, availableMoves.Count)];
            } else
            {
                //No moves left
                return null;
            }
        }

        public int Symbol()
        {
            return this.symbol;
        }
    }
}