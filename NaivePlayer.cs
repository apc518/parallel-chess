namespace MultiChessConsole
{
    using Rudz.Chess;
    using Rudz.Chess.Types;
    using Rudz.Chess.Enums;
    using Rudz.Chess.MoveGeneration;
    using System;

    /*
     * Takes a piece if it can. Has no concept of piece values or search.
     */
    internal class NaivePlayer : IPlayer
    {
        static Random rand = new Random();
        public Players color;
        Players IPlayer.color { get => color; set => color = value; }

        public NaivePlayer(Players color)
        {
            this.color = color;
        }

        public Move nextMove(Position pos, State state)
        {
            var validMoves = pos.GenerateMoves();

            if (validMoves.Length == 0) return Move.EmptyMove; // checkmate or stalemate

            foreach (Move move in validMoves) if (pos.Board.PieceAt(move.ToSquare())) return move;

            return validMoves[rand.Next(0, validMoves.Length)];
        }
    }
}