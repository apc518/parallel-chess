using Rudz.Chess;
using Rudz.Chess.Types;
using Rudz.Chess.Enums;

namespace MultiChessConsole
{
    interface IPlayer
    {
        public Players color { get; set; }
        public Move nextMove(Position pos, State state);
    }
}
