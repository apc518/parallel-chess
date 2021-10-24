using Rudz.Chess.Types;
using Rudz.Chess;
using Rudz.Chess.Hash.Tables;
using Rudz.Chess.MoveGeneration;
using System.Collections.Generic;

namespace MultiChessConsole
{
    internal class PositionData
    {
        public int eval = 0;
        public int evalDepth = -1;
        public int ply = 0;
        public MoveList validMoves = null;

        public PositionData(int eval, int evalDepth, int ply)
        {
            this.eval = eval;
            this.evalDepth = evalDepth;
            this.ply = ply;
        }
    }
}
