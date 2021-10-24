namespace MultiChessConsole
{
    using Rudz.Chess.Enums;
    using Rudz.Chess;
    using Rudz.Chess.Types;
    using Rudz.Chess.MoveGeneration;
    using System;

    internal class RandomPlayer : IPlayer
    {
        Random rand = new Random();
        private Players color;
        Players IPlayer.color { get => color; set => color = value; }

        public RandomPlayer(Players color)
        {
            this.color = color;
        }

        public Move nextMove(Position pos, State state)
        {
            var validMoves = pos.GenerateMoves();
            if (validMoves.Length == 0) return Move.EmptyMove; // checkmate or stalemate
            return validMoves[rand.Next(0, validMoves.Length)];
        }
    }
}